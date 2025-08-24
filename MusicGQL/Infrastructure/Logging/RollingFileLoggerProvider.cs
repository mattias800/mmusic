using System.Collections.Concurrent;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MusicGQL.Infrastructure.Logging;

public sealed class RollingFileLoggerProvider : ILoggerProvider
{
    private readonly string _directory;
    private readonly string _baseName;
    private readonly string _extension;
    private readonly Encoding _encoding;
    private readonly LogLevel _minLevel;
    private readonly ConcurrentDictionary<string, RollingFileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();

    private StreamWriter? _writer;
    private DateTime _currentHourUtc;
    private string? _currentFilePath;

    public RollingFileLoggerProvider(
        string directory,
        string baseFileName,
        LogLevel minLevel = LogLevel.Information,
        Encoding? encoding = null,
        string extension = "log")
    {
        _directory = directory;
        _baseName = baseFileName;
        _extension = string.IsNullOrWhiteSpace(extension) ? "log" : extension.Trim('.');
        _minLevel = minLevel;
        _encoding = encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        System.IO.Directory.CreateDirectory(_directory);
        OpenNewWriter(initial: true);
    }

    private static DateTime TruncateToHourUtc(DateTime utcNow)
        => new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, DateTimeKind.Utc);

    private void OpenNewWriter(bool initial)
    {
        lock (_sync)
        {
            try
            {
                _writer?.Flush();
                _writer?.Dispose();
            }
            catch { }

            var now = DateTime.UtcNow;
            _currentHourUtc = TruncateToHourUtc(now);
            // Filename time: on initial creation use now (with seconds); on rotation use top-of-hour
            var stamp = initial ? now : _currentHourUtc;
            var fileName = $"{_baseName}-{stamp:yyyyMMdd-HHmmss}.{_extension}";
            _currentFilePath = System.IO.Path.Combine(_directory, fileName);
            _writer = new StreamWriter(new FileStream(_currentFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), _encoding)
            {
                AutoFlush = true,
                NewLine = Environment.NewLine
            };
            try { _writer.WriteLine($"==== mmusic log started {DateTime.UtcNow:O} (UTC) ===="); } catch { }
        }
    }

    private void MaybeRotate()
    {
        var now = DateTime.UtcNow;
        // Rotate once we cross into the next hour
        if (now >= _currentHourUtc.AddHours(1))
        {
            OpenNewWriter(initial: false);
        }
    }

    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, name => new RollingFileLogger(name, this));

    internal bool IsEnabled(LogLevel level) => level >= _minLevel;

    internal void Write(string category, LogLevel level, EventId eventId, string message, Exception? exception)
    {
        if (!IsEnabled(level)) return;
        lock (_sync)
        {
            MaybeRotate();
            var timestamp = DateTime.UtcNow.ToString("O");
            var lvl = level.ToString();
            var line = exception == null
                ? $"{timestamp} [{lvl}] {category}: {message}"
                : $"{timestamp} [{lvl}] {category}: {message} | {exception.GetType().Name}: {exception.Message}";
            _writer?.WriteLine(line);
            if (exception != null)
            {
                try { _writer?.WriteLine(exception.StackTrace); } catch { }
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            try
            {
                _writer?.Flush();
                _writer?.Dispose();
                _writer = null;
            }
            catch { }
        }
    }

    private sealed class RollingFileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly RollingFileLoggerProvider _provider;

        public RollingFileLogger(string categoryName, RollingFileLoggerProvider provider)
        {
            _categoryName = categoryName;
            _provider = provider;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => _provider.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            var message = formatter(state, exception);
            _provider.Write(_categoryName, logLevel, eventId, message, exception);
        }
    }
}
