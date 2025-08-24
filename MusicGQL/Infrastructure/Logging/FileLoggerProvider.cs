using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Path = System.IO.Path;

namespace MusicGQL.Infrastructure.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly Encoding _encoding;
    private readonly LogLevel _minLevel;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();
    private StreamWriter? _writer;

    public FileLoggerProvider(string filePath, LogLevel minLevel = LogLevel.Information, Encoding? encoding = null)
    {
        _filePath = filePath;
        _minLevel = minLevel;
        _encoding = encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
        _writer = new StreamWriter(new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), _encoding)
        {
            AutoFlush = true,
            NewLine = Environment.NewLine
        };
        WritePreamble();
    }

    private void WritePreamble()
    {
        try
        {
            _writer?.WriteLine($"==== mmusic log started {DateTime.UtcNow:O} (UTC) ====");
        }
        catch { }
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, this));
    }

    internal bool IsEnabled(LogLevel level) => level >= _minLevel;

    internal void Write(string category, LogLevel level, EventId eventId, string message, Exception? exception)
    {
        if (!IsEnabled(level)) return;
        var timestamp = DateTime.UtcNow.ToString("O");
        var lvl = level.ToString();
        var line = exception == null
            ? $"{timestamp} [{lvl}] {category}: {message}"
            : $"{timestamp} [{lvl}] {category}: {message} | {exception.GetType().Name}: {exception.Message}";
        lock (_sync)
        {
            _writer?.WriteLine(line);
            if (exception != null)
            {
                try { _writer?.WriteLine(exception.StackTrace); } catch { }
            }
        }
    }

    public void Dispose()
    {
        try
        {
            lock (_sync)
            {
                _writer?.Flush();
                _writer?.Dispose();
                _writer = null;
            }
        }
        catch { }
    }

    private sealed class FileLogger(string categoryName, FileLoggerProvider provider) : ILogger
    {
        private readonly string _categoryName = categoryName;
        private readonly FileLoggerProvider _provider = provider;

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
