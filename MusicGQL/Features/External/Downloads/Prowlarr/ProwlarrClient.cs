using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Sockets;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;
using Microsoft.Extensions.Options;
using MusicGQL.Features.ServerSettings;
using MusicGQL.Features.Downloads.Services;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

public interface IProwlarrClient
{
    Task<IReadOnlyList<ProwlarrRelease>> SearchAlbumAsync(string artistName, string releaseTitle, int? year, CancellationToken cancellationToken, IDownloadLogger? relLogger = null);
    Task<(bool ok, string message)> TestConnectivityAsyncPublic(CancellationToken cancellationToken);
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    string GetClientInfo();
}

public class ProwlarrClient(
    HttpClient httpClient,
    IOptions<ProwlarrOptions> options,
    ILogger<ProwlarrClient> logger,
    DownloadLogPathProvider logPathProvider,
    ServerSettingsAccessor serverSettingsAccessor) : IProwlarrClient
{
    private DownloadLogger? DownloadLogger { get; set; }

    public async Task<DownloadLogger> GetLogger()
    {
        if (DownloadLogger == null)
        {
            var path = await logPathProvider.GetServiceLogFilePathAsync("prowlarr", CancellationToken.None);
            DownloadLogger = new DownloadLogger(path);
        }

        return DownloadLogger;
    }

    private async Task<bool> TestConnectivityAsync(CancellationToken cancellationToken)
    {
        var serviceLogger = await GetLogger();

        try
        {
            var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.LogWarning("[Prowlarr] BaseUrl is not configured");
                serviceLogger.Error("[Prowlarr] BaseUrl is not configured");
                return false;
            }

            // Try a simple ping to the server. If we have an API key, include it to avoid 401s.
            var apiKey = options.Value.ApiKey;
            var pingUrl = string.IsNullOrWhiteSpace(apiKey)
                ? $"{baseUrl}/api/v1/system/status"
                : $"{baseUrl}/api/v1/system/status?apikey={Uri.EscapeDataString(apiKey)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, pingUrl);
            req.Headers.Accept.Clear();
            req.Headers.Accept.ParseAdd("application/json");
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                req.Headers.Add("X-Api-Key", apiKey);
            }

            serviceLogger.Info($"Connectivity: GET {pingUrl}");

            logger.LogDebug("[Prowlarr] Testing connectivity to {Url}", pingUrl);
            serviceLogger.Info($"[Prowlarr] Testing connectivity to {pingUrl}");

            // Create a timeout for just the connectivity test (shorter than the HttpClient default)
            var connectivityTimeoutSeconds = 10;
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(connectivityTimeoutSeconds));
            using var combinedCts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var startTime = DateTime.UtcNow;
            var resp = await httpClient.SendAsync(req, combinedCts.Token);
            var duration = DateTime.UtcNow - startTime;

            if (resp.IsSuccessStatusCode)
            {
                logger.LogInformation(
                    "[Prowlarr] Connectivity test to {BaseUrl}: {Status} in {Duration:0.00}s (Connected)",
                    baseUrl, (int)resp.StatusCode, duration.TotalSeconds);
                serviceLogger.Info(
                    $"Connectivity OK HTTP {(int)resp.StatusCode} in {duration.TotalSeconds:0.00}s");
                return true;
            }
            else
            {
                // Distinguish between different types of failures
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // Server is reachable but auth is required/denied. Treat as CONNECTED for the purposes of reachability
                    // and let the search endpoint test validate the API key correctness.
                    logger.LogWarning(
                        "[Prowlarr] Auth response from {BaseUrl}: {Status} in {Duration:0.00}s. Server is reachable.",
                        baseUrl, (int)resp.StatusCode, duration.TotalSeconds);

                    serviceLogger.Warn(
                        $"Connectivity auth response HTTP {(int)resp.StatusCode} in {duration.TotalSeconds:0.00}s");

                    return true;
                }

                logger.LogWarning(
                    "[Prowlarr] Connectivity test to {BaseUrl}: {Status} in {Duration:0.00}s (Failed - Server responded but with error)",
                    baseUrl, (int)resp.StatusCode, duration.TotalSeconds);
                serviceLogger.Warn(
                    $"Connectivity failed HTTP {(int)resp.StatusCode} in {duration.TotalSeconds:0.00}s");

                return false;
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TaskCanceledException)
        {
            var connectivityTimeoutSeconds = 10; // matches the CTS above
            logger.LogWarning(
                "[Prowlarr] Connectivity test failed to {BaseUrl} - Request timed out after {Timeout}s. This usually indicates network connectivity issues or the server is not responding.",
                options.Value.BaseUrl, connectivityTimeoutSeconds);

            serviceLogger.Warn($"Connectivity timeout after {connectivityTimeoutSeconds}s");

            // Add specific timeout diagnostics
            logger.LogWarning("[Prowlarr] TIMEOUT DIAGNOSTICS:");
            logger.LogWarning("[Prowlarr] - Request was sent but no response received within {Timeout}s",
                connectivityTimeoutSeconds);
            logger.LogWarning("[Prowlarr] - This suggests the server at {BaseUrl} is either:", options.Value.BaseUrl);
            logger.LogWarning("[Prowlarr]   * Not running (check if Prowlarr service is started)");
            logger.LogWarning("[Prowlarr]   * Not accessible from this machine (check network/firewall)");
            logger.LogWarning("[Prowlarr]   * Overloaded and not responding (check server resources)");

            return false;
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning(
                "[Prowlarr] Connectivity test failed to {BaseUrl} - Request was canceled. This may indicate network connectivity issues.",
                options.Value.BaseUrl);

            serviceLogger.Warn("Connectivity request canceled");

            return false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex,
                "[Prowlarr] Connectivity test failed to {BaseUrl} - HTTP request error. This may indicate network connectivity issues or the server is unreachable.",
                options.Value.BaseUrl);
            serviceLogger.Error($"Connectivity HTTP exception: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "[Prowlarr] Connectivity test failed to {BaseUrl} - Unexpected error during connectivity test.",
                options.Value.BaseUrl);
            serviceLogger.Error($"Connectivity exception: {ex.Message}");

            return false;
        }
    }

    private async Task<bool> TestSearchEndpointAsync(CancellationToken cancellationToken)
    {
        var serviceLogger = await GetLogger();

        try
        {
            var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
            var apiKey = options.Value.ApiKey;
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
                return false;

// Test the actual search endpoint with a simple query
            // Use query and repeated Audio categories; optionally restrict to configured indexers
            var testUrlBase = $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query=test&categories=3000&categories=3010&categories=3040&categories=3050";
            var indexers = options.Value.IndexerIds;
            var testUrl = testUrlBase + (indexers is { Length: > 0 }
                ? string.Concat(indexers.Select(i => $"&indexers={i}"))
                : string.Empty);
            using var req = new HttpRequestMessage(HttpMethod.Get, testUrl);
            req.Headers.Accept.Clear();
            req.Headers.Accept.ParseAdd("application/json");

            var start = DateTime.UtcNow;
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var dur = DateTime.UtcNow - start;
            if (resp.IsSuccessStatusCode)
            {
                serviceLogger.Info($"Search test OK HTTP {(int)resp.StatusCode} in {dur.TotalSeconds:0.00}s");
                return true;
            }
            else
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                serviceLogger.Warn($"Search test failed HTTP {(int)resp.StatusCode}. Body: {body}");

                return false;
            }
        }
        catch (Exception ex)
        {
            serviceLogger.Error($"Search test exception: {ex.Message}");
            return false;
        }
    }

    public void LogCurrentConfiguration()
    {
        logger.LogInformation(
            "[Prowlarr] Current configuration: BaseUrl={BaseUrl}, ApiKeyConfigured={HasApiKey}, Timeout={TimeoutSeconds}s, MaxRetries={MaxRetries}, TestConnectivityFirst={TestConnectivity}",
            options.Value.BaseUrl, !string.IsNullOrWhiteSpace(options.Value.ApiKey), options.Value.TimeoutSeconds,
            options.Value.MaxRetries, options.Value.TestConnectivityFirst);
    }

    public string GetConfigurationSummary()
    {
        return string.Join("; ", new[]
        {
            $"BaseUrl={options.Value.BaseUrl}",
            $"ApiKeyConfigured={!string.IsNullOrWhiteSpace(options.Value.ApiKey)}",
            $"HttpClient Timeout: {httpClient.Timeout.TotalSeconds}s",
            $"MaxRetries={options.Value.MaxRetries}",
            $"TestConnectivityFirst={options.Value.TestConnectivityFirst}",
            $"RetryDelay={options.Value.RetryDelaySeconds}s"
        });
    }

    public async Task<(bool ok, string message)> TestConnectivityAsyncPublic(CancellationToken cancellationToken)
    {
        try
        {
            var isConnected = await TestConnectivityAsync(cancellationToken);
            if (!isConnected) return (false, "Connectivity failed");
            var isSearchOk = await TestSearchEndpointAsync(cancellationToken);
            return (isSearchOk, isSearchOk ? "OK" : "Search endpoint failed");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Provides a summary of the current Prowlarr configuration for debugging purposes.
    /// </summary>
    // Duplicate method removed (single summary method above)

    /// <summary>
    /// Tests basic connectivity to the Prowlarr server and returns diagnostic information.
    /// </summary>
    public async Task<string> GetDiagnosticInfoAsync(CancellationToken cancellationToken = default)
    {
        var info = new List<string>
        {
            GetConfigurationSummary(),
            $"HttpClient Timeout: {httpClient.Timeout.TotalSeconds}s",
            $"Current UTC Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC",
            $"Machine Name: {Environment.MachineName}",
            $"OS: {Environment.OSVersion}"
        };

        try
        {
            var isConnected = await TestConnectivityAsync(cancellationToken);
            info.Add($"Connectivity Test: {(isConnected ? "SUCCESS" : "FAILED")}");

            if (isConnected)
            {
                var isSearchWorking = await TestSearchEndpointAsync(cancellationToken);
                info.Add($"Search Endpoint Test: {(isSearchWorking ? "SUCCESS" : "FAILED")}");
            }
            else
            {
                // Add troubleshooting suggestions for connectivity failures
                info.Add("");
                info.Add("TROUBLESHOOTING SUGGESTIONS:");
                info.Add("1. Verify Prowlarr server is running and accessible");
                info.Add("2. Check if the IP address 192.168.11.174 is correct");
                info.Add("3. Verify port 9696 is open and not blocked by firewall");
                info.Add("4. Test network connectivity: ping 192.168.11.174");
                info.Add("5. Check if you can access http://192.168.11.174:9696 in a web browser");
                info.Add("6. Verify the API key is correct");
                info.Add("7. Check Prowlarr server logs for any errors");
            }
        }
        catch (Exception ex)
        {
            info.Add($"Diagnostic Tests: FAILED - {ex.Message}");
            info.Add($"Exception Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                info.Add($"Inner Exception: {ex.InnerException.Message}");
            }
        }

        return string.Join(Environment.NewLine, info);
    }

    /// <summary>
    /// Performs a quick health check of the Prowlarr service.
    /// </summary>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var isConnected = await TestConnectivityAsync(cancellationToken);
            if (!isConnected) return false;

            var isSearchWorking = await TestSearchEndpointAsync(cancellationToken);
            return isSearchWorking;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the current HttpClient timeout setting.
    /// </summary>
    public TimeSpan GetCurrentTimeout()
    {
        return httpClient.Timeout;
    }

    /// <summary>
    /// Tests basic network connectivity to the Prowlarr server IP address.
    /// This can help diagnose if the issue is network-level or application-level.
    /// </summary>
    public async Task<string> TestNetworkConnectivityAsync()
    {
        var info = new List<string>
        {
            "Network Connectivity Test Results:",
            "================================"
        };

        try
        {
            var baseUrl = options.Value.BaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                info.Add("ERROR: BaseUrl not configured");
                return string.Join(Environment.NewLine, info);
            }

            // Extract IP address from URL
            var uri = new Uri(baseUrl);
            var host = uri.Host;
            var port = uri.Port;

            info.Add($"Target Host: {host}");
            info.Add($"Target Port: {port}");
            info.Add($"Full URL: {baseUrl}");

            // Test DNS resolution
            try
            {
                var addresses = await System.Net.Dns.GetHostAddressesAsync(host);
                info.Add($"DNS Resolution: SUCCESS - {addresses.Length} address(es) found");
                foreach (var addr in addresses)
                {
                    info.Add($"  - {addr} ({addr.AddressFamily})");
                }
            }
            catch (Exception ex)
            {
                info.Add($"DNS Resolution: FAILED - {ex.Message}");
            }

            // Test basic connectivity (this is a simple test, not a full HTTP request)
            try
            {
                using var client = new System.Net.Sockets.TcpClient();
                var connectTask = client.ConnectAsync(host, port);
                var timeoutTask = Task.Delay(5000); // 5 second timeout

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == connectTask)
                {
                    await connectTask; // Ensure any exceptions are thrown
                    info.Add($"TCP Connection: SUCCESS - Connected to {host}:{port}");
                    client.Close();
                }
                else
                {
                    info.Add($"TCP Connection: TIMEOUT - Could not connect to {host}:{port} within 5 seconds");
                }
            }
            catch (Exception ex)
            {
                info.Add($"TCP Connection: FAILED - {ex.Message}");
            }

            info.Add("");
            info.Add("Note: These tests check basic network connectivity.");
            info.Add("HTTP-level issues (like authentication, API errors) require additional testing.");
        }
        catch (Exception ex)
        {
            info.Add($"Network Test Error: {ex.Message}");
        }

        return string.Join(Environment.NewLine, info);
    }

    /// <summary>
    /// Tests basic network connectivity to the Prowlarr server
    /// </summary>
    public async Task<string> TestBasicNetworkConnectivityAsync()
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return "ERROR: BaseUrl is not configured";
        }

        var uri = new Uri(baseUrl);
        var host = uri.Host;
        var port = uri.Port;

        var info = new List<string>
        {
            $"Basic Network Connectivity Test for {baseUrl}",
            "=============================================",
            $"Target Host: {host}",
            $"Target Port: {port}",
            ""
        };

        try
        {
            // Test 1: DNS Resolution
            info.Add("1. DNS Resolution Test:");
            try
            {
                var addresses = await System.Net.Dns.GetHostAddressesAsync(host);
                info.Add($"   ✅ SUCCESS: Resolved {host} to {addresses.Length} address(es):");
                foreach (var addr in addresses)
                {
                    info.Add($"      - {addr} ({addr.AddressFamily})");
                }
            }
            catch (Exception ex)
            {
                info.Add($"   ❌ FAILED: Could not resolve {host}: {ex.Message}");
                info.Add($"   💡 SUGGESTION: Check if the hostname is correct and DNS is working");
                return string.Join(Environment.NewLine, info);
            }

            info.Add("");

            // Test 2: Port Connectivity
            info.Add("2. Port Connectivity Test:");
            try
            {
                using var tcpClient = new System.Net.Sockets.TcpClient();
                var connectTask = tcpClient.ConnectAsync(host, port);

                // Use a shorter timeout for basic connectivity test
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == connectTask)
                {
                    await connectTask; // Ensure any exceptions are thrown
                    info.Add($"   ✅ SUCCESS: Port {port} is reachable on {host}");
                    info.Add($"   💡 This means the network path is working, but Prowlarr may not be responding");
                }
                else
                {
                    info.Add($"   ❌ FAILED: Port {port} connection timed out after 10s");
                    info.Add($"   💡 SUGGESTION: Check if Prowlarr service is running and listening on port {port}");
                }
            }
            catch (Exception ex)
            {
                info.Add($"   ❌ FAILED: Port {port} connection failed: {ex.Message}");
                info.Add($"   💡 SUGGESTION: Check firewall rules and if Prowlarr is listening on port {port}");
            }

            info.Add("");
            info.Add("SUMMARY:");
            info.Add("If DNS resolution works but port connectivity fails, the issue is likely:");
            info.Add("- Prowlarr service not running");
            info.Add("- Prowlarr not listening on the expected port");
            info.Add("- Firewall blocking the connection");
            info.Add("");
            info.Add("If both tests pass but HTTP requests timeout, the issue is likely:");
            info.Add("- Prowlarr service overloaded");
            info.Add("- Application-level blocking");
            info.Add("- Incorrect API endpoint configuration");
        }
        catch (Exception ex)
        {
            info.Add($"ERROR during network test: {ex.Message}");
        }

        return string.Join(Environment.NewLine, info);
    }

    /// <summary>
    /// Attempts to diagnose the specific issue with the Prowlarr connection.
    /// </summary>
    public async Task<string> DiagnoseConnectionIssueAsync(CancellationToken cancellationToken = default)
    {
        var diagnostics = new List<string>
        {
            "=== Prowlarr Connection Diagnostics ===",
            GetConfigurationSummary(),
            $"HttpClient Timeout: {httpClient.Timeout.TotalSeconds}s",
            $"Current UTC Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
        };

        try
        {
            // Test basic connectivity
            diagnostics.Add("--- Connectivity Test ---");
            var isConnected = await TestConnectivityAsync(cancellationToken);
            diagnostics.Add($"Basic connectivity: {(isConnected ? "SUCCESS" : "FAILED")}");

            if (isConnected)
            {
                // Test search endpoint
                diagnostics.Add("--- Search Endpoint Test ---");
                var isSearchWorking = await TestSearchEndpointAsync(cancellationToken);
                diagnostics.Add($"Search endpoint: {(isSearchWorking ? "SUCCESS" : "FAILED")}");

                if (!isSearchWorking)
                {
                    diagnostics.Add("Search endpoint is reachable but not working properly.");
                    diagnostics.Add("This may indicate API configuration issues or server problems.");
                }
            }
            else
            {
                diagnostics.Add("Cannot reach Prowlarr server.");
                diagnostics.Add("Check if the server is running and accessible from this machine.");
                diagnostics.Add($"Verify the BaseUrl: {options.Value.BaseUrl}");
            }
        }
        catch (Exception ex)
        {
            diagnostics.Add($"Diagnostic tests failed with exception: {ex.Message}");
            diagnostics.Add($"Exception type: {ex.GetType().Name}");
        }

        diagnostics.Add("=== End Diagnostics ===");
        return string.Join(Environment.NewLine, diagnostics);
    }

    /// <summary>
    /// Provides recommendations for fixing common Prowlarr connection issues.
    /// </summary>
    public string GetTroubleshootingRecommendations()
    {
        var recommendations = new List<string>
        {
            "=== Prowlarr Troubleshooting Recommendations ===",
            "",
            "1. Check Server Status:",
            $"   - Verify Prowlarr is running at {options.Value.BaseUrl}",
            "   - Check Prowlarr logs for errors",
            "   - Ensure the service is not overloaded",
            "",
            "2. Verify Configuration:",
            "   - Check if the API key is correct",
            "   - Verify the BaseUrl is accessible from this machine",
            "   - Ensure network connectivity between machines",
            "",
            "3. Adjust Timeout Settings:",
            $"   - Current timeout: {options.Value.TimeoutSeconds}s",
            $"   - Current max retries: {options.Value.MaxRetries}",
            "   - Try increasing TimeoutSeconds if server is slow",
            "   - Try increasing MaxRetries for transient failures",
            "",
            "4. Network Diagnostics:",
            "   - Test basic connectivity with TestConnectivityAsync()",
            "   - Test search endpoint with TestSearchEndpointAsync()",
            "   - Use DiagnoseConnectionIssueAsync() for comprehensive diagnostics",
            "",
            "5. Common Issues:",
            "   - Firewall blocking connections",
            "   - Prowlarr server overloaded",
            "   - Network latency issues",
            "   - API rate limiting",
            "",
            "=== End Recommendations ==="
        };

        return string.Join(Environment.NewLine, recommendations);
    }

    /// <summary>
    /// Performs a quick test to see if the current configuration is valid.
    /// </summary>
    public bool IsConfigurationValid()
    {
        return !string.IsNullOrWhiteSpace(options.Value.BaseUrl) &&
               !string.IsNullOrWhiteSpace(options.Value.ApiKey) &&
               options.Value.TimeoutSeconds > 0 &&
               options.Value.MaxRetries >= 0 &&
               options.Value.RetryDelaySeconds >= 0;
    }

    /// <summary>
    /// Gets a summary of the current Prowlarr client status.
    /// </summary>
    public async Task<string> GetStatusSummaryAsync(CancellationToken cancellationToken = default)
    {
        var status = new List<string>
        {
            "=== Prowlarr Client Status Summary ===",
            $"Configuration Valid: {IsConfigurationValid()}",
            GetConfigurationSummary(),
            $"HttpClient Timeout: {httpClient.Timeout.TotalSeconds}s",
            $"Current UTC Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"
        };

        if (IsConfigurationValid())
        {
            try
            {
                var isHealthy = await IsHealthyAsync(cancellationToken);
                status.Add($"Service Health: {(isHealthy ? "HEALTHY" : "UNHEALTHY")}");

                if (!isHealthy)
                {
                    status.Add("Service is not healthy. Use DiagnoseConnectionIssueAsync() for details.");
                }
            }
            catch (Exception ex)
            {
                status.Add($"Health check failed: {ex.Message}");
            }
        }
        else
        {
            status.Add("Configuration is invalid. Check BaseUrl, ApiKey, and timeout settings.");
        }

        status.Add("=== End Status Summary ===");
        return string.Join(Environment.NewLine, status);
    }

    /// <summary>
    /// Attempts to reset the HttpClient connection to resolve potential connection issues.
    /// </summary>
    public void ResetConnection()
    {
        // Note: This is a simple approach. In a production environment, you might want to
        // implement a more sophisticated connection pooling strategy.
        logger.LogInformation("[Prowlarr] Resetting HttpClient connection");

        // The HttpClient will be recreated by the DI container on the next request
        // This method is mainly for logging and potential future enhancements
    }

    /// <summary>
    /// Provides a quick health check that can be used for monitoring.
    /// </summary>
    public async Task<(bool IsHealthy, string Details)> QuickHealthCheckAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConfigurationValid())
            {
                return (false, "Configuration is invalid");
            }

            var isConnected = await TestConnectivityAsync(cancellationToken);
            if (!isConnected)
            {
                return (false, "Prowlarr server is not accessible (network issue or authentication failed)");
            }

            var isSearchWorking = await TestSearchEndpointAsync(cancellationToken);
            if (!isSearchWorking)
            {
                return (false, "Search endpoint is not working properly");
            }

            return (true, "All health checks passed");
        }
        catch (Exception ex)
        {
            return (false, $"Health check failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current Prowlarr client version and capabilities.
    /// </summary>
    public string GetClientInfo()
    {
        return "ProwlarrClient v1.0 - Enhanced with timeout handling, retry logic, and comprehensive diagnostics";
    }

    public async Task<IReadOnlyList<ProwlarrRelease>> SearchAlbumAsync(string artistName, string releaseTitle,
        int? year, CancellationToken cancellationToken, IDownloadLogger? relLogger = null)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("[Prowlarr] Missing configuration: BaseUrl={BaseUrl}, ApiKey={HasApiKey}",
                baseUrl ?? "null", !string.IsNullOrWhiteSpace(apiKey));
            return Array.Empty<ProwlarrRelease>();
        }

        logger.LogInformation(
            "[Prowlarr] Starting search for '{Artist} - {Release}' using {BaseUrl} (timeout: {Timeout}s, max retries: {MaxRetries})",
            artistName, releaseTitle, baseUrl, options.Value.TimeoutSeconds, options.Value.MaxRetries);

        // Log current configuration for debugging
        LogCurrentConfiguration();


        // Build search query set with broad-first strategy
        logger.LogInformation("[Prowlarr] ===== BUILDING SEARCH QUERY =====");
        relLogger?.Info("[Prowlarr] ===== BUILDING SEARCH QUERY =====");
        logger.LogInformation("[Prowlarr] Original search: Artist='{Artist}', Release='{Release}', Year={Year}",
            artistName, releaseTitle, year?.ToString() ?? "null");
        relLogger?.Info(
            $"[Prowlarr] Original search: Artist='{artistName}', Release='{releaseTitle}', Year={year?.ToString() ?? "null"}");

        var baseBroadQuery = ($"{artistName} {releaseTitle}").Trim();
        logger.LogInformation("[Prowlarr] Broad base query (no year/quality): '{Query}'", baseBroadQuery);
        relLogger?.Info($"[Prowlarr] Broad base query (no year/quality): '{baseBroadQuery}'");

        var searchQueries = ProwlarrSearchStrategy.BuildQueries(artistName, releaseTitle, year, logger).ToList();

        logger.LogInformation(
            "[Prowlarr] Broad-first strategy for '{Artist} - {Release}': base → base+year → base+320 → base+FLAC",
            artistName, releaseTitle);
        relLogger?.Info(
            $"[Prowlarr] Broad-first strategy for '{artistName} - {releaseTitle}': base → base+year → base+320 → base+FLAC");
        logger.LogInformation("[Prowlarr] Will try search queries in order: {Queries}",
            string.Join(" | ", searchQueries));
        relLogger?.Info($"[Prowlarr] Will try search queries in order: {string.Join(" | ", searchQueries)}");

        logger.LogInformation("[Prowlarr] ===== STARTING SEARCH EXECUTION =====");
        relLogger?.Info("[Prowlarr] ===== STARTING SEARCH EXECUTION =====");

        // Try each search query until we find results, with retry logic
        foreach (var searchQuery in searchQueries)
        {
            logger.LogInformation("[Prowlarr] Trying quality level: '{Query}'", searchQuery);

            // Retry logic for transient failures
            var maxRetries = options.Value.MaxRetries;
            for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
            {
                try
                {
                    logger.LogInformation("[Prowlarr] Executing search for query: '{Query}'", searchQuery);
                    relLogger?.Info($"[Prowlarr] Executing search for query: '{searchQuery}'");
                    var results = await TrySearchWithQueryAsync(searchQuery, artistName, releaseTitle,
                        cancellationToken, relLogger);

                    logger.LogInformation("[Prowlarr] Query '{Query}' returned {Count} results", searchQuery,
                        results.Count);
                    relLogger?.Info($"[Prowlarr] Query '{searchQuery}' returned {results.Count} results");

                    if (results.Count > 0)
                    {
                        string qualityLevel;
                        if (searchQuery.Equals(baseBroadQuery, StringComparison.OrdinalIgnoreCase))
                        {
                            qualityLevel = "broad (no year/quality)";
                        }
                        else if (year.HasValue && searchQuery.Equals($"{baseBroadQuery} {year.Value}", StringComparison.OrdinalIgnoreCase))
                        {
                            qualityLevel = "with year";
                        }
                        else if (searchQuery.Contains(" 320", StringComparison.OrdinalIgnoreCase))
                        {
                            qualityLevel = "320 kbps";
                        }
                        else if (searchQuery.Contains(" FLAC", StringComparison.OrdinalIgnoreCase))
                        {
                            qualityLevel = "FLAC";
                        }
                        else
                        {
                            qualityLevel = "unknown";
                        }
                        logger.LogInformation(
                            "[Prowlarr] ✅ SUCCESS! Found {Count} results with {QualityLevel} search: '{Query}'",
                            results.Count, qualityLevel, searchQuery);
                        relLogger?.Info(
                            $"[Prowlarr] ✅ SUCCESS! Found {results.Count} results with {qualityLevel} search: '{searchQuery}'");

                        // LOG EVERY RESULT THAT PROWLARR RETURNED
                        logger.LogInformation("[Prowlarr] ===== PROWLARR RETURNED RESULTS =====");
                        relLogger?.Info("[Prowlarr] ===== PROWLARR RETURNED RESULTS =====");
                        for (int i = 0; i < results.Count; i++)
                        {
                            var result = results[i];
                            logger.LogInformation(
                                "[Prowlarr] RESULT #{Index}: '{Title}' (Size: {Size}, Magnet: {HasMagnet}, Download: {HasDownload})",
                                i + 1, result.Title ?? "null", result.Size,
                                !string.IsNullOrWhiteSpace(result.MagnetUrl),
                                !string.IsNullOrWhiteSpace(result.DownloadUrl));
                            relLogger?.Info(
                                $"[Prowlarr] RESULT #{i + 1}: '{result.Title ?? "null"}' (Size: {result.Size}, Magnet: {!string.IsNullOrWhiteSpace(result.MagnetUrl)}, Download: {!string.IsNullOrWhiteSpace(result.DownloadUrl)})");
                        }

                        return results;
                    }
                    else
                    {
                        logger.LogInformation("[Prowlarr] ❌ No results with '{Query}', trying next quality level",
                            searchQuery);
                        relLogger?.Info($"[Prowlarr] ❌ No results with '{searchQuery}', trying next quality level");
                        break; // No results, try next quality level
                    }
                }
                catch (TaskCanceledException) when (attempt <= maxRetries)
                {
                    logger.LogWarning(
                        "[Prowlarr] Attempt {Attempt} timed out for '{Query}' after {Timeout}s, retrying... ({Remaining} attempts remaining). " +
                        "This may indicate the Prowlarr server is slow or overloaded.",
                        attempt, searchQuery, options.Value.TimeoutSeconds, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    var delaySeconds = Math.Max(2, options.Value.RetryDelaySeconds);
                    logger.LogInformation("[Prowlarr] Waiting {Delay}s before retry...", delaySeconds);
await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    continue;
                }
                catch (HttpRequestException) when (attempt <= maxRetries)
                {
                    logger.LogWarning(
                        "[Prowlarr] Attempt {Attempt} failed with HTTP error for '{Query}', retrying... ({Remaining} attempts remaining)",
                        attempt, searchQuery, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    var delaySeconds = Math.Max(2, options.Value.RetryDelaySeconds);
                    logger.LogInformation("[Prowlarr] Waiting {Delay}s before retry...", delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    continue;
                }
                catch (Exception ex) when (attempt <= maxRetries)
                {
                    logger.LogWarning(ex,
                        "[Prowlarr] Attempt {Attempt} failed for '{Query}', retrying... ({Remaining} attempts remaining)",
                        attempt, searchQuery, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    var delaySeconds = Math.Max(2, options.Value.RetryDelaySeconds);
                    logger.LogInformation("[Prowlarr] Waiting {Delay}s before retry...", delaySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                    continue;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Prowlarr] Final attempt failed for '{Query}' after {Attempt} attempts",
                        searchQuery, attempt);
                    break; // Final attempt failed, try next quality level
                }
            }
        }

        logger.LogInformation("[Prowlarr] ===== SEARCH COMPLETE - NO RESULTS =====");
        relLogger?.Info("[Prowlarr] ===== SEARCH COMPLETE - NO RESULTS =====");
        logger.LogInformation(
            "[Prowlarr] ❌ No results found with any quality level (320, FLAC, or no quality specified)");
        relLogger?.Info("[Prowlarr] ❌ No results found with any quality level (320, FLAC, or no quality specified)");
        logger.LogInformation("[Prowlarr] Search queries tried:");
        relLogger?.Info("[Prowlarr] Search queries tried:");
        foreach (var query in searchQueries)
        {
            logger.LogInformation("[Prowlarr]   - '{Query}'", query);
            relLogger?.Info($"[Prowlarr]   - '{query}'");
        }

        // Post-failure diagnostics: run connectivity and search endpoint tests to surface reasons
        try
        {
            relLogger?.Info("[Prowlarr] Running post-failure diagnostics (connectivity + search endpoint)");
            var connected = await TestConnectivityAsync(cancellationToken);
            if (connected)
            {
                logger.LogInformation("[Prowlarr] Post-diagnostic: connectivity OK");
                relLogger?.Info("[Prowlarr] Post-diagnostic: connectivity OK");
            }
            else
            {
                logger.LogWarning("[Prowlarr] Post-diagnostic: connectivity FAILED");
                relLogger?.Warn("[Prowlarr] Post-diagnostic: connectivity FAILED");
            }

            var searchOk = await TestSearchEndpointAsync(cancellationToken);
            if (searchOk)
            {
                logger.LogInformation("[Prowlarr] Post-diagnostic: search endpoint OK");
                relLogger?.Info("[Prowlarr] Post-diagnostic: search endpoint OK");
            }
            else
            {
                logger.LogWarning("[Prowlarr] Post-diagnostic: search endpoint FAILED");
                relLogger?.Warn("[Prowlarr] Post-diagnostic: search endpoint FAILED");
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "[Prowlarr] Post-failure diagnostics threw");
            relLogger?.Warn($"[Prowlarr] Post-diagnostic exception: {ex.Message}");
        }

        logger.LogInformation("[Prowlarr] Returning empty results array");
        relLogger?.Info("[Prowlarr] Returning empty results array");
        return Array.Empty<ProwlarrRelease>();
    }

    private async Task<IReadOnlyList<ProwlarrRelease>> TrySearchWithQueryAsync(string query, string artistName,
        string releaseTitle, CancellationToken cancellationToken, IDownloadLogger? relLogger = null)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            return Array.Empty<ProwlarrRelease>();
        }

        // Build candidate URLs via shared builder for verifiable composition
        var candidateUrls = ProwlarrQueryBuilder.BuildCandidateUrls(baseUrl!, apiKey!, query, options.Value.IndexerIds, logger);

        int attemptNumber = 0;
        foreach (var url in candidateUrls)
        {
            DateTime attemptStart = DateTime.UtcNow;
            try
            {
                attemptNumber++;
                logger.LogInformation("[Prowlarr] ===== URL ATTEMPT #{Attempt}/{Total} =====", attemptNumber,
                    candidateUrls.Count);
                relLogger?.Info($"[Prowlarr] ===== URL ATTEMPT #{attemptNumber}/{candidateUrls.Count} =====");

                var executor = new ProwlarrRequestExecutor(httpClient, options, logger);
                attemptStart = DateTime.UtcNow;
                var result = await executor.GetJsonAsync(url, cancellationToken, relLogger);
                if (!result.Success)
                {
                    var delaySeconds = Math.Max(2, options.Value.RetryDelaySeconds);
                    logger.LogInformation("[Prowlarr] Waiting {Delay}s before next attempt...", delaySeconds);
                    try { relLogger?.Info($"[Prowlarr] Waiting {delaySeconds}s before next attempt..."); } catch { }
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                    continue;
                }

                // Success - parse and handle JSON
                var json = result.Json!;
                logger.LogInformation("[Prowlarr] Response JSON length: {Length} characters", json.Length);
                try { relLogger?.Info($"[Prowlarr] Response JSON length: {json.Length} characters"); } catch { }

                var preview = json.Length > 1000 ? json[..1000] + "…" : json;
                logger.LogInformation("[Prowlarr] Response JSON preview: {Preview}", preview);
                try { relLogger?.Info($"[Prowlarr] Response JSON preview: {preview}"); } catch { }

                logger.LogInformation("[Prowlarr] Parsing JSON response...");
                try { relLogger?.Info("[Prowlarr] Parsing JSON response..."); } catch { }
                using var doc = JsonDocument.Parse(json);
                var list = ProwlarrJsonParser.ParseResults(doc.RootElement, artistName, releaseTitle, logger);

                logger.LogInformation("[Prowlarr] ===== PARSING RESULTS =====");
                relLogger?.Info("[Prowlarr] ===== PARSING RESULTS =====");
                logger.LogInformation("[Prowlarr] Parsed {Count} results from response", list.Count);
                relLogger?.Info($"[Prowlarr] Parsed {list.Count} results from response");

                if (list.Count > 0)
                {
                    logger.LogInformation("[Prowlarr] ✅ SUCCESS! Found {Count} results from this URL", list.Count);
                    relLogger?.Info($"[Prowlarr] ✅ SUCCESS! Found {list.Count} results from this URL");
                    return list;
                }
                else
                {
                    logger.LogInformation("[Prowlarr] ❌ No valid results from this URL variant - trying next");
                    relLogger?.Info("[Prowlarr] ❌ No valid results from this URL variant - trying next");
                    try
                    {
                        ProwlarrJsonParser.LogRootShape(doc.RootElement, logger);
                    }
                    catch
                    {
                    }

                    continue;
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TaskCanceledException)
            {
                var dur = DateTime.UtcNow - attemptStart;
                logger.LogWarning(ex,
                    "[Prowlarr] Request timed out or was canceled for {Url} after {Seconds:0.00}s - server may be unreachable or overloaded. BaseUrl={BaseUrl}",
                    url, dur.TotalSeconds, options.Value.BaseUrl);
                // Wait a bit before trying the next URL variant
                var delaySeconds = Math.Max(2, options.Value.RetryDelaySeconds);
                logger.LogInformation("[Prowlarr] Waiting {Delay}s before next attempt...", delaySeconds);
                try
                {
                    relLogger?.Info($"[Prowlarr] Waiting {delaySeconds}s before next attempt...");
                }
                catch
                {
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                continue;
            }
            catch (TaskCanceledException ex)
            {
                logger.LogWarning(ex,
                    "[Prowlarr] Request was canceled for {Url}. This may indicate the configured timeout of {Timeout}s was reached.",
                    url, options.Value.TimeoutSeconds);
                // Wait a bit before trying the next URL variant
                var delaySeconds = Math.Max(2, options.Value.RetryDelaySeconds);
                logger.LogInformation("[Prowlarr] Waiting {Delay}s before next attempt...", delaySeconds);
                try
                {
                    relLogger?.Info($"[Prowlarr] Waiting {delaySeconds}s before next attempt...");
                }
                catch
                {
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                continue;
            }
            catch (HttpRequestException ex)
            {
                var innerMsg = ex.InnerException?.Message;
                if (ex.InnerException is SocketException sock)
                {
                    logger.LogWarning(ex,
                        "[Prowlarr] HTTP request failed for {Url}. SocketError={SocketError} ({SocketCode}). Reason={Reason}. Inner={Inner}",
                        url, sock.SocketErrorCode, (int)sock.SocketErrorCode, ex.Message, innerMsg ?? "<none>");
                }
                else
                {
                    logger.LogWarning(ex, "[Prowlarr] HTTP request failed for {Url}. Reason={Reason}. Inner={Inner}",
                        url, ex.Message, innerMsg ?? "<none>");
                }

                // Wait a bit before trying the next URL variant
                var delaySeconds = Math.Max(2, options.Value.RetryDelaySeconds);
                logger.LogInformation("[Prowlarr] Waiting {Delay}s before next attempt...", delaySeconds);
                try
                {
                    relLogger?.Info($"[Prowlarr] Waiting {delaySeconds}s before next attempt...");
                }
                catch
                {
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[Prowlarr] Search attempt failed for {Url}", url);
                // Wait a bit before trying the next URL variant
                var delaySeconds = Math.Max(2, options.Value.RetryDelaySeconds);
                logger.LogInformation("[Prowlarr] Waiting {Delay}s before next attempt...", delaySeconds);
                try
                {
                    relLogger?.Info($"[Prowlarr] Waiting {delaySeconds}s before next attempt...");
                }
                catch
                {
                }

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                continue;
            }
        }

        logger.LogInformation("Prowlarr returned no results for query '{Query}' across all URL variants. " +
                              "If you expected results, verify that audio indexers and category mappings are configured correctly, and consider loosening the search terms. " +
                              "Configuration: Timeout={Timeout}s, MaxRetries={MaxRetries}, TestConnectivityFirst={TestConnectivity}. " +
                              "Configuration summary: {ConfigSummary}. " +
                              "Health check result: {HealthStatus}. " +
                              "HttpClient timeout: {HttpTimeout}s. " +
                              "Configuration valid: {ConfigValid}. " +
                              "Client info: {ClientInfo}",
            query, options.Value.TimeoutSeconds, options.Value.MaxRetries,
            options.Value.TestConnectivityFirst,
            GetConfigurationSummary(), await IsHealthyAsync(cancellationToken) ? "HEALTHY" : "UNHEALTHY",
            GetCurrentTimeout().TotalSeconds, IsConfigurationValid(), GetClientInfo());
        return Array.Empty<ProwlarrRelease>();
    }












}

