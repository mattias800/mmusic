using System.Net.Http.Json;
using System.Text.Json;
using MusicGQL.Features.External.Downloads.Prowlarr.Configuration;
using Microsoft.Extensions.Options;

namespace MusicGQL.Features.External.Downloads.Prowlarr;

public class ProwlarrClient(HttpClient httpClient, IOptions<ProwlarrOptions> options, ILogger<ProwlarrClient> logger)
{
    private async Task<bool> TestConnectivityAsync(CancellationToken cancellationToken)
    {
        try
        {
            var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.LogWarning("[Prowlarr] BaseUrl is not configured");
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
                // Some setups expect the API key in a header
                try { req.Headers.Add("X-Api-Key", apiKey); } catch { }
            }
            
            logger.LogDebug("[Prowlarr] Testing connectivity to {Url}", pingUrl);
            
            // Create a timeout for just the connectivity test (shorter than the HttpClient default)
            var connectivityTimeoutSeconds = 10;
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(connectivityTimeoutSeconds));
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            var startTime = DateTime.UtcNow;
            var resp = await httpClient.SendAsync(req, combinedCts.Token);
            var duration = DateTime.UtcNow - startTime;
            
            if (resp.IsSuccessStatusCode)
            {
                logger.LogInformation("[Prowlarr] Connectivity test to {BaseUrl}: {Status} in {Duration:0.00}s (Connected)", 
                    baseUrl, (int)resp.StatusCode, duration.TotalSeconds);
                return true;
            }
            else
            {
                // Distinguish between different types of failures
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized || resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // Server is reachable but auth is required/denied. Treat as CONNECTED for the purposes of reachability
                    // and let the search endpoint test validate the API key correctness.
                    logger.LogWarning("[Prowlarr] Auth response from {BaseUrl}: {Status} in {Duration:0.00}s. Server is reachable.", 
                        baseUrl, (int)resp.StatusCode, duration.TotalSeconds);
                    return true;
                }
                
                logger.LogWarning("[Prowlarr] Connectivity test to {BaseUrl}: {Status} in {Duration:0.00}s (Failed - Server responded but with error)", 
                    baseUrl, (int)resp.StatusCode, duration.TotalSeconds);
                return false;
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TaskCanceledException)
        {
            var connectivityTimeoutSeconds = 10; // matches the CTS above
            logger.LogWarning("[Prowlarr] Connectivity test failed to {BaseUrl} - Request timed out after {Timeout}s. This usually indicates network connectivity issues or the server is not responding.", 
                options.Value.BaseUrl, connectivityTimeoutSeconds);
            
            // Add specific timeout diagnostics
            logger.LogWarning("[Prowlarr] TIMEOUT DIAGNOSTICS:");
            logger.LogWarning("[Prowlarr] - Request was sent but no response received within {Timeout}s", connectivityTimeoutSeconds);
            logger.LogWarning("[Prowlarr] - This suggests the server at {BaseUrl} is either:", options.Value.BaseUrl);
            logger.LogWarning("[Prowlarr]   * Not running (check if Prowlarr service is started)");
            logger.LogWarning("[Prowlarr]   * Not accessible from this machine (check network/firewall)");
            logger.LogWarning("[Prowlarr]   * Overloaded and not responding (check server resources)");
            
            return false;
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("[Prowlarr] Connectivity test failed to {BaseUrl} - Request was canceled. This may indicate network connectivity issues.", 
                options.Value.BaseUrl);
            return false;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "[Prowlarr] Connectivity test failed to {BaseUrl} - HTTP request error. This may indicate network connectivity issues or the server is unreachable.", 
                options.Value.BaseUrl);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Prowlarr] Connectivity test failed to {BaseUrl} - Unexpected error during connectivity test.", 
                options.Value.BaseUrl);
            return false;
        }
    }

    private async Task<bool> TestSearchEndpointAsync(CancellationToken cancellationToken)
    {
        try
        {
            var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
            var apiKey = options.Value.ApiKey;
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
                return false;

            // Test the actual search endpoint with a simple query
            var testUrl = $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&term=test&categories=3000&categories=3010&categories=3030";
            using var req = new HttpRequestMessage(HttpMethod.Get, testUrl);
            req.Headers.Accept.Clear();
            req.Headers.Accept.ParseAdd("application/json");
            
            logger.LogDebug("[Prowlarr] Testing search endpoint: {Url}", testUrl);
            var startTime = DateTime.UtcNow;
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var duration = DateTime.UtcNow - startTime;
            
            var isWorking = resp.IsSuccessStatusCode;
            logger.LogInformation("[Prowlarr] Search endpoint test: {Status} in {Duration:0.00}s ({Working})", 
                (int)resp.StatusCode, duration.TotalSeconds, isWorking ? "Working" : "Failed");
            return isWorking;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Prowlarr] Search endpoint test failed");
            return false;
        }
    }

    private void LogCurrentConfiguration()
    {
        if (options.Value.EnableDetailedLogging)
        {
            logger.LogDebug("[Prowlarr] Current configuration: BaseUrl={BaseUrl}, Timeout={Timeout}s, MaxRetries={MaxRetries}, " +
                "TestConnectivityFirst={TestConnectivity}, RetryDelay={RetryDelay}s, EnableDetailedLogging={DetailedLogging}, MaxConcurrent={MaxConcurrent}",
                options.Value.BaseUrl, options.Value.TimeoutSeconds, options.Value.MaxRetries, 
                options.Value.TestConnectivityFirst, options.Value.RetryDelaySeconds, 
                options.Value.EnableDetailedLogging, options.Value.MaxConcurrentRequests);
        }
    }

    /// <summary>
    /// Provides a summary of the current Prowlarr configuration for debugging purposes.
    /// </summary>
    public string GetConfigurationSummary()
    {
        return $"Prowlarr Configuration: BaseUrl={options.Value.BaseUrl}, Timeout={options.Value.TimeoutSeconds}s, " +
               $"MaxRetries={options.Value.MaxRetries}, TestConnectivityFirst={options.Value.TestConnectivityFirst}, " +
               $"RetryDelay={options.Value.RetryDelaySeconds}s, EnableDetailedLogging={options.Value.EnableDetailedLogging}, " +
               $"MaxConcurrentRequests={options.Value.MaxConcurrentRequests}";
    }

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
    public async Task<(bool IsHealthy, string Details)> QuickHealthCheckAsync(CancellationToken cancellationToken = default)
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

    public async Task<IReadOnlyList<ProwlarrRelease>> SearchAlbumAsync(string artistName, string releaseTitle, CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("[Prowlarr] Missing configuration: BaseUrl={BaseUrl}, ApiKey={HasApiKey}", 
                baseUrl ?? "null", !string.IsNullOrWhiteSpace(apiKey));
            return Array.Empty<ProwlarrRelease>();
        }

        logger.LogInformation("[Prowlarr] Starting search for '{Artist} - {Release}' using {BaseUrl} (timeout: {Timeout}s, max retries: {MaxRetries})", 
            artistName, releaseTitle, baseUrl, options.Value.TimeoutSeconds, options.Value.MaxRetries);

        // Log current configuration for debugging
        LogCurrentConfiguration();

        // Test basic connectivity first
        if (options.Value.TestConnectivityFirst)
        {
            var isConnected = await TestConnectivityAsync(cancellationToken);
            if (!isConnected)
            {
                logger.LogWarning("[Prowlarr] Prowlarr server is not accessible at {BaseUrl}. This may be due to network issues, authentication problems, or the server being unavailable. Skipping search.", baseUrl);
                return Array.Empty<ProwlarrRelease>();
            }
            
            // Also test the search endpoint specifically
            var isSearchWorking = await TestSearchEndpointAsync(cancellationToken);
            if (!isSearchWorking)
            {
                logger.LogWarning("[Prowlarr] Prowlarr server is reachable but search endpoint is not working properly. " +
                    "This may indicate API configuration issues or server problems.");
                // Continue anyway as the server might be temporarily slow
            }
        }

        // Build search query with quality preferences
        var baseQuery = artistName + " " + releaseTitle;
        
        // Create multiple search variants with different quality priorities
        var searchQueries = new List<string>
        {
            // 1. Search for 320 kbps MP3 first (good quality, reasonable file size)
            baseQuery + " 320",
            // 2. Search for FLAC (lossless quality)
            baseQuery + " FLAC",
            // 3. Fallback to base query (no quality specified)
            baseQuery
        };
        
        logger.LogInformation("[Prowlarr] Quality-based search strategy for '{Artist} - {Release}': 320 → FLAC → no quality", artistName, releaseTitle);
        logger.LogInformation("[Prowlarr] Will try search queries in order: {Queries}", string.Join(" | ", searchQueries));
        
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
                    var results = await TrySearchWithQueryAsync(searchQuery, artistName, releaseTitle, cancellationToken);
                    if (results.Count > 0)
                    {
                        var qualityLevel = searchQuery == baseQuery ? "no quality specified" : 
                                         searchQuery.Contains("320") ? "320 kbps" : 
                                         searchQuery.Contains("FLAC") ? "FLAC" : "unknown";
                        logger.LogInformation("[Prowlarr] Found {Count} results with {QualityLevel} search: '{Query}'", results.Count, qualityLevel, searchQuery);
                        return results;
                    }
                    else
                    {
                        logger.LogInformation("[Prowlarr] No results with '{Query}', trying next quality level", searchQuery);
                        break; // No results, try next quality level
                    }
                }
                catch (TaskCanceledException) when (attempt <= maxRetries)
                {
                    logger.LogWarning("[Prowlarr] Attempt {Attempt} timed out for '{Query}' after {Timeout}s, retrying... ({Remaining} attempts remaining). " +
                        "This may indicate the Prowlarr server is slow or overloaded.", 
                        attempt, searchQuery, options.Value.TimeoutSeconds, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.RetryDelaySeconds), cancellationToken);
                    continue;
                }
                catch (HttpRequestException) when (attempt <= maxRetries)
                {
                    logger.LogWarning("[Prowlarr] Attempt {Attempt} failed with HTTP error for '{Query}', retrying... ({Remaining} attempts remaining)", 
                        attempt, searchQuery, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.RetryDelaySeconds), cancellationToken);
                    continue;
                }
                catch (Exception ex) when (attempt <= maxRetries)
                {
                    logger.LogWarning(ex, "[Prowlarr] Attempt {Attempt} failed for '{Query}', retrying... ({Remaining} attempts remaining)", 
                        attempt, searchQuery, maxRetries - attempt + 1);
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.RetryDelaySeconds), cancellationToken);
                    continue;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Prowlarr] Final attempt failed for '{Query}' after {Attempt} attempts", searchQuery, attempt);
                    break; // Final attempt failed, try next quality level
                }
            }
        }
        
        logger.LogInformation("[Prowlarr] No results found with any quality level (320, FLAC, or no quality specified)");
        return Array.Empty<ProwlarrRelease>();
    }
    
    private async Task<IReadOnlyList<ProwlarrRelease>> TrySearchWithQueryAsync(string query, string artistName, string releaseTitle, CancellationToken cancellationToken)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            return Array.Empty<ProwlarrRelease>();
        }

        // Try a few parameter variants to handle API changes/configs
        var candidateUrls = new List<string>
        {
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query={Uri.EscapeDataString(query)}&type=search",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&query={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030&limit=50",
            // Alternate param names some setups accept
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&term={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&term={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&term={Uri.EscapeDataString(query)}&categories=3000&categories=3010&categories=3030&limit=50",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&q={Uri.EscapeDataString(query)}",
            $"{baseUrl}/api/v1/search?apikey={Uri.EscapeDataString(apiKey)}&Query={Uri.EscapeDataString(query)}",
        };

        foreach (var url in candidateUrls)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Accept.Clear();
                req.Headers.Accept.ParseAdd("application/json");
                // Some setups prefer API key via header
                try { req.Headers.Add("X-Api-Key", apiKey); } catch { }
                
                var attemptNumber = candidateUrls.IndexOf(url) + 1;
                logger.LogInformation("[Prowlarr] GET {Url} (attempt {Attempt}/{MaxAttempts})", url, attemptNumber, candidateUrls.Count);
                var startTime = DateTime.UtcNow;
                
                // Log request details for debugging
                if (options.Value.EnableDetailedLogging)
                {
                    logger.LogDebug("[Prowlarr] Request details: Method={Method}, Headers={Headers}, Timeout={Timeout}s", 
                        req.Method, string.Join(", ", req.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}")), options.Value.TimeoutSeconds);
                }
                
                var resp = await httpClient.SendAsync(req, cancellationToken);
                var duration = DateTime.UtcNow - startTime;
                
                logger.LogInformation("[Prowlarr] GET completed in {Duration:0.00}s with status {Status}", duration.TotalSeconds, (int)resp.StatusCode);
                
                if (!resp.IsSuccessStatusCode)
                {
                    string? body = null;
                    try { body = await resp.Content.ReadAsStringAsync(cancellationToken); } catch { }
                    if (!string.IsNullOrWhiteSpace(body) && body.Length > 500) body = body[..500] + "…";
                    logger.LogInformation("[Prowlarr] GET HTTP {Status} for {Url}. Body: {Body}", (int)resp.StatusCode, url, body);
                    continue;
                }
                
                // Success - read and parse response
                logger.LogDebug("[Prowlarr] Reading response content for {Url}", url);
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                var preview = json.Length > 600 ? json[..600] + "…" : json;
                logger.LogInformation("[Prowlarr] GET body preview: {Preview}", preview);
                using var doc = JsonDocument.Parse(json);
                var list = ParseProwlarrResults(doc.RootElement, artistName, releaseTitle, logger);
                logger.LogInformation("[Prowlarr] Parsed {Count} results from GET", list.Count);
                if (list.Count == 0)
                {
                    logger.LogInformation("[Prowlarr] No results from this GET variant; trying next, if any.");
                    try { LogRootShape(doc.RootElement, logger); } catch { }
                    continue;
                }
                return list;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TaskCanceledException)
            {
                logger.LogWarning(ex, "Prowlarr search timed out for {Url} - this may indicate network issues or server unresponsiveness. " +
                    "Check if Prowlarr server at {BaseUrl} is running and accessible.", url, options.Value.BaseUrl);
                continue;
            }
            catch (TaskCanceledException ex)
            {
                logger.LogWarning(ex, "Prowlarr search was canceled for {Url}. " +
                    "This may indicate the request took longer than the configured timeout of {Timeout}s.", url, options.Value.TimeoutSeconds);
                continue;
            }
            catch (HttpRequestException ex)
            {
                logger.LogDebug(ex, "Prowlarr search HTTP exception for {Url}", url);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Prowlarr search failed for {Url}", url);
                continue;
            }
        }
        // Try a different search approach - the POST endpoint seems to be for grabbing specific releases
        // Let's try using the same endpoint but with different parameters or a different method
        try
        {
            var baseUrl2 = options.Value.BaseUrl!.TrimEnd('/');
            var apiKey2 = options.Value.ApiKey!;
            // Try using the search endpoint with query parameters instead of POST body
            var url = $"{baseUrl2}/api/v1/search?apikey={Uri.EscapeDataString(apiKey2)}&query={Uri.EscapeDataString(query)}&type=search";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Accept.Clear();
            req.Headers.Accept.ParseAdd("application/json");
            try { req.Headers.Add("X-Api-Key", apiKey2); } catch { }
            
            logger.LogInformation("[Prowlarr] Alternative GET {Url}", url);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            if (resp.IsSuccessStatusCode)
            {
                var json = await resp.Content.ReadAsStringAsync(cancellationToken);
                var preview = json.Length > 600 ? json[..600] + "…" : json;
                logger.LogInformation("[Prowlarr] Alternative GET body preview: {Preview}", preview);
                using var doc = JsonDocument.Parse(json);
                var list = ParseProwlarrResults(doc.RootElement, artistName, releaseTitle, logger);
                logger.LogInformation("[Prowlarr] Parsed {Count} results from alternative GET", list.Count);
                if (list.Count > 0)
                {
                    return list;
                }
            }
            else
            {
                string? body = null;
                try { body = await resp.Content.ReadAsStringAsync(cancellationToken); } catch { }
                if (!string.IsNullOrWhiteSpace(body) && body.Length > 500) body = body[..500] + "…";
                logger.LogInformation("[Prowlarr] Alternative GET HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Prowlarr alternative search failed");
        }
        logger.LogWarning("Prowlarr search failed for all URL variants for query '{Query}'. " +
            "This may indicate: 1) Prowlarr server is down/unreachable, 2) API key is invalid, 3) Server is overloaded, or 4) Network connectivity issues. " +
            "Troubleshooting: Check if Prowlarr is running at {BaseUrl}, verify the API key, check server logs for errors, and ensure network connectivity. " +
            "Current configuration: Timeout={Timeout}s, MaxRetries={MaxRetries}, TestConnectivityFirst={TestConnectivity}. " +
            "Try increasing TimeoutSeconds or MaxRetries in configuration if the server is slow. " +
            "Configuration summary: {ConfigSummary}. " +
            "Use GetDiagnosticInfoAsync() method for detailed diagnostics. " +
            "Health check result: {HealthStatus}. " +
            "HttpClient timeout: {HttpTimeout}s. " +
            "Use DiagnoseConnectionIssueAsync() for comprehensive diagnostics. " +
            "Use GetTroubleshootingRecommendations() for step-by-step fixes. " +
            "Configuration valid: {ConfigValid}. " +
            "Use GetStatusSummaryAsync() for current status. " +
            "Consider calling ResetConnection() if experiencing persistent connection issues. " +
            "Use QuickHealthCheckAsync() for monitoring purposes. " +
            "Client info: {ClientInfo}", 
            query, options.Value.BaseUrl, options.Value.TimeoutSeconds, options.Value.MaxRetries, options.Value.TestConnectivityFirst, 
            GetConfigurationSummary(), await IsHealthyAsync(cancellationToken) ? "HEALTHY" : "UNHEALTHY", GetCurrentTimeout().TotalSeconds, IsConfigurationValid(), GetClientInfo());
        return Array.Empty<ProwlarrRelease>();
    }

    private static List<ProwlarrRelease> ParseProwlarrResults(JsonElement root, string artistName, string releaseTitle, ILogger logger)
    {
        // Root can be array or object with Results/results
        JsonElement array = root;
        if (root.ValueKind == JsonValueKind.Object)
        {
            if (!TryGetPropertyCI(root, "results", out array) && !TryGetPropertyCI(root, "Results", out array))
            {
                // Try direct array property 'Data'
                if (!TryGetPropertyCI(root, "data", out array) && !TryGetPropertyCI(root, "Data", out array))
                {
                    array = root;
                }
            }
        }
        if (array.ValueKind != JsonValueKind.Array)
        {
            return new List<ProwlarrRelease>();
        }
        var list = new List<ProwlarrRelease>();
        var totalResults = 0;
        var filteredResults = 0;
        
        foreach (var item in array.EnumerateArray())
        {
            totalResults++;
            string? title = GetStringCI(item, "title") ?? GetStringCI(item, "Title");
            string? guid = GetStringCI(item, "guid") ?? GetStringCI(item, "Guid");
            string? magnet = GetStringCI(item, "magnetUrl") ?? GetStringCI(item, "MagnetUrl");
            string? downloadUrl = GetStringCI(item, "downloadUrl") ?? GetStringCI(item, "DownloadUrl")
                ?? GetStringCI(item, "link") ?? GetStringCI(item, "Link");
            long? size = GetInt64CI(item, "size") ?? GetInt64CI(item, "Size");
            int? indexerId = null;
            if (TryGetPropertyCI(item, "indexerId", out var idx) && idx.ValueKind == JsonValueKind.Number)
            {
                if (idx.TryGetInt32(out var i32)) indexerId = i32;
            }
            
            var release = new ProwlarrRelease(title, guid, magnet, downloadUrl, size, indexerId);
            
            // Validate and score the result
            if (IsValidMusicResult(release, artistName, releaseTitle))
            {
                list.Add(release);
            }
            else
            {
                filteredResults++;
                var reason = GetRejectionReason(release, artistName, releaseTitle);
                logger.LogDebug("[Prowlarr] Filtered out result '{Title}': {Reason}", title, reason);
            }
        }
        
        logger.LogInformation("[Prowlarr] Filtered {Filtered}/{Total} results for '{Artist} - {Album}'", 
            filteredResults, totalResults, artistName, releaseTitle);
        
        // Sort by relevance score (highest first)
        list.Sort((a, b) => CalculateRelevanceScore(b, artistName, releaseTitle).CompareTo(CalculateRelevanceScore(a, artistName, releaseTitle)));
        
        return list;
    }

    private static void LogRootShape(JsonElement root, ILogger logger)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            logger.LogDebug("[Prowlarr] Root is array; first item keys: {Keys}",
                root.GetArrayLength() > 0 ? string.Join(",", root[0].EnumerateObject().Select(p => p.Name)) : "(empty)");
            return;
        }
        if (root.ValueKind == JsonValueKind.Object)
        {
            var keys = root.EnumerateObject().Select(p => p.Name).ToList();
            logger.LogDebug("[Prowlarr] Root is object; keys: {Keys}", string.Join(",", keys));
            foreach (var k in keys)
            {
                if (TryGetPropertyCI(root, k, out var v) && v.ValueKind == JsonValueKind.Array && v.GetArrayLength() > 0)
                {
                    var innerKeys = v[0].EnumerateObject().Select(p => p.Name);
                    logger.LogDebug("[Prowlarr] First element keys of '{Key}': {InnerKeys}", k, string.Join(",", innerKeys));
                }
            }
        }
        else
        {
            logger.LogDebug("[Prowlarr] Root is {Kind}", root.ValueKind);
        }
    }

    private static bool TryGetPropertyCI(JsonElement obj, string name, out JsonElement value)
    {
        if (obj.ValueKind != JsonValueKind.Object)
        {
            value = default;
            return false;
        }
        foreach (var prop in obj.EnumerateObject())
        {
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = prop.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    private static string? GetStringCI(JsonElement obj, string name)
    {
        if (TryGetPropertyCI(obj, name, out var v) && v.ValueKind == JsonValueKind.String)
        {
            return v.GetString();
        }
        return null;
    }
    
    private static bool IsValidMusicResult(ProwlarrRelease release, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return false;
            
        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();
        
        // Reject obvious non-music content
        if (ContainsNonMusicTerms(title))
            return false;
            
        // Must contain artist name (or close variation)
        if (!ContainsArtistName(title, artist))
            return false;
            
        // Must contain album title (or close variation)
        if (!ContainsAlbumTitle(title, album))
            return false;
            
        // Reject torrent files for SABnzbd
        if (IsTorrentFile(release.DownloadUrl))
            return false;
            
        return true;
    }
    
    private static bool ContainsNonMusicTerms(string title)
    {
        var nonMusicTerms = new[]
        {
            "1080p", "720p", "4k", "hdtv", "web-dl", "bluray", "dvdrip", "h264", "h265", "x264", "x265",
            "season", "episode", "s01", "s02", "e01", "e02", "complete", "series",
            "movie", "film", "documentary", "show", "tv", "television",
            "subtitle", "dub", "dubbed", "multi", "dual", "audio"
        };
        
        return nonMusicTerms.Any(term => title.Contains(term));
    }
    
    private static bool ContainsArtistName(string title, string artistName)
    {
        // Split artist name into words and check if most are present
        var artistWords = artistName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // At least 50% of artist words must be present
        var matchingWords = artistWords.Count(word => titleWords.Any(titleWord => 
            titleWord.Contains(word) || word.Contains(titleWord)));
            
        return matchingWords >= Math.Max(1, artistWords.Length / 2);
    }
    
    private static bool ContainsAlbumTitle(string title, string albumTitle)
    {
        // Split album title into words and check if most are present
        var albumWords = albumTitle.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleWords = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // At least 50% of album words must be present
        var matchingWords = albumWords.Count(word => titleWords.Any(titleWord => 
            titleWord.Contains(word) || word.Contains(titleWord)));
            
        return matchingWords >= Math.Max(1, albumWords.Length / 2);
    }
    
    private static bool IsTorrentFile(string? downloadUrl)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl))
            return false;
            
        return downloadUrl.EndsWith(".torrent", StringComparison.OrdinalIgnoreCase) ||
               downloadUrl.Contains("torrent", StringComparison.OrdinalIgnoreCase);
    }
    
    private static int CalculateRelevanceScore(ProwlarrRelease release, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return 0;
            
        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();
        
        int score = 0;
        
        // Base score for being valid
        score += 10;
        
        // Bonus for exact artist name match
        if (title.Contains(artist))
            score += 20;
            
        // Bonus for exact album title match
        if (title.Contains(album))
            score += 20;
            
        // Bonus for quality indicators
        var qualityTerms = new[] { "flac", "lossless", "320", "cd", "vinyl", "24bit", "16bit" };
        foreach (var term in qualityTerms)
        {
            if (title.Contains(term))
                score += 5;
        }
        
        // Bonus for common music file extensions
        var musicExtensions = new[] { ".mp3", ".flac", ".m4a", ".wav", ".ogg", ".aac" };
        foreach (var ext in musicExtensions)
        {
            if (title.Contains(ext))
                score += 3;
        }
        
        // Penalty for torrent files
        if (IsTorrentFile(release.DownloadUrl))
            score -= 50;
            
        return score;
    }
    
    private static string GetRejectionReason(ProwlarrRelease release, string artistName, string releaseTitle)
    {
        if (string.IsNullOrWhiteSpace(release.Title))
            return "Missing title";
            
        var title = release.Title.ToLowerInvariant();
        var artist = artistName.ToLowerInvariant();
        var album = releaseTitle.ToLowerInvariant();
        
        // Check each validation step
        if (ContainsNonMusicTerms(title))
            return "Contains non-music terms (TV/movie/video)";
            
        if (!ContainsArtistName(title, artist))
            return "Artist name mismatch";
            
        if (!ContainsAlbumTitle(title, album))
            return "Album title mismatch";
            
        if (IsTorrentFile(release.DownloadUrl))
            return "Torrent file (not compatible with SABnzbd)";
            
        return "Unknown validation failure";
    }

    private static long? GetInt64CI(JsonElement obj, string name)
    {
        if (TryGetPropertyCI(obj, name, out var v) && v.ValueKind == JsonValueKind.Number)
        {
            if (v.TryGetInt64(out var i64)) return i64;
        }
        return null;
    }
}

public record ProwlarrRelease(string? Title, string? Guid, string? MagnetUrl, string? DownloadUrl, long? Size, int? IndexerId);


