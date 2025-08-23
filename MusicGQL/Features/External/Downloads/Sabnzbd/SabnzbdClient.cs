using Microsoft.Extensions.Options;
using MusicGQL.Features.External.Downloads.Sabnzbd.Configuration;

namespace MusicGQL.Features.External.Downloads.Sabnzbd;

public class SabnzbdClient(HttpClient httpClient, IOptions<SabnzbdOptions> options, ILogger<SabnzbdClient> logger, MusicGQL.Features.Downloads.Services.DownloadLogPathProvider logPathProvider)
{
    private MusicGQL.Features.Downloads.Services.DownloadLogger? serviceLogger;

    public async Task<MusicGQL.Features.Downloads.Services.DownloadLogger> GetLogger()
    {
        if (serviceLogger == null)
        {
            var path = await logPathProvider.GetServiceLogFilePathAsync("sabnzbd", CancellationToken.None);
            serviceLogger = new MusicGQL.Features.Downloads.Services.DownloadLogger(path);
        }
        return serviceLogger;
    }

    public async Task<bool> AddNzbByUrlAsync(string nzbUrl, string? nzbName, CancellationToken cancellationToken, string? pathOverride = null)
    {
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
        var apiKey = options.Value.ApiKey;
        var serviceLogger = await GetLogger();
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("[SABnzbd] Missing BaseUrl or ApiKey; cannot send NZB");
            serviceLogger.Warn("Missing BaseUrl or ApiKey");
            return false;
        }
        var category = options.Value.Category ?? "music";
        try
        {
            // Log the exact NZB source URL handed off to SAB (sanitized)
            logger.LogInformation("[SABnzbd] addurl source NZB URL: {Url}", SanitizeUrlForLogs(nzbUrl));
            serviceLogger.Info($"addurl source NZB URL: {SanitizeUrlForLogs(nzbUrl)}");

            var url = $"{baseUrl}/api?mode=addurl&name={Uri.EscapeDataString(nzbUrl)}&apikey={Uri.EscapeDataString(apiKey)}&output=json&cat={Uri.EscapeDataString(category)}";
            if (!string.IsNullOrWhiteSpace(nzbName))
            {
                url += "&nzbname=" + Uri.EscapeDataString(nzbName);
            }
            if (!string.IsNullOrWhiteSpace(pathOverride))
            {
                // Try both keys; SAB will ignore unknown
                url += "&path=" + Uri.EscapeDataString(pathOverride);
                url += "&dir=" + Uri.EscapeDataString(pathOverride);
            }
            logger.LogDebug("[SABnzbd] GET {Url}", url);
            serviceLogger.Info($"GET {url}");
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            var resp = await httpClient.SendAsync(req, cancellationToken);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var trimmed = body?.Length > 500 ? body[..500] + "…" : body;
                logger.LogWarning("[SABnzbd] Add URL failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, trimmed);
                serviceLogger.Warn($"Add URL failed HTTP {(int)resp.StatusCode}. Body: {trimmed}");
                return false;
            }
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(body);
                var statusProp = json.RootElement.TryGetProperty("status", out var s) ? s : default;
                var okJson = statusProp.ValueKind == System.Text.Json.JsonValueKind.True || (statusProp.ValueKind == System.Text.Json.JsonValueKind.String && string.Equals(statusProp.GetString(), "true", StringComparison.OrdinalIgnoreCase));
                if (!okJson)
                {
                    logger.LogWarning("[SABnzbd] Add URL returned status=false. Body: {Body}", body);
                    serviceLogger.Warn($"Add URL returned status=false. Body: {body}");
                }
                return okJson;
            }
            catch
            {
                // If not JSON, assume success when HTTP 200
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SABnzbd addurl failed");
            serviceLogger.Error($"Add URL exception: {ex.Message}");
            return false;
        }
    }

public async Task<bool> AddNzbByContentAsync(byte[] nzbBytes, string fileName, CancellationToken cancellationToken, string? pathOverride = null, string? nzbName = null)
{
    var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
    var apiKey = options.Value.ApiKey;
    var serviceLogger = await GetLogger();
    if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
    {
        logger.LogWarning("[SABnzbd] Missing BaseUrl or ApiKey; cannot send NZB content");
        serviceLogger.Warn("Missing BaseUrl or ApiKey");
        return false;
    }
    var category = options.Value.Category ?? "music";
    try
    {
        var url = $"{baseUrl}/api?mode=addfile&apikey={Uri.EscapeDataString(apiKey)}&output=json&cat={Uri.EscapeDataString(category)}";
        if (!string.IsNullOrWhiteSpace(nzbName))
        {
            url += "&nzbname=" + Uri.EscapeDataString(nzbName);
        }
        if (!string.IsNullOrWhiteSpace(pathOverride))
        {
            url += "&path=" + Uri.EscapeDataString(pathOverride);
            url += "&dir=" + Uri.EscapeDataString(pathOverride);
        }
        using var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(nzbBytes);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-nzb");
        form.Add(content, "nzbfile", fileName);
        logger.LogDebug("[SABnzbd] POST {Url} (addfile) filename={File}", url, fileName);
        serviceLogger.Info($"POST {url} (addfile) filename={fileName}");
        var resp = await httpClient.PostAsync(url, form, cancellationToken);
        var ok = resp.IsSuccessStatusCode;
        if (!ok)
        {
            string? body = null;
            try { body = await resp.Content.ReadAsStringAsync(cancellationToken); } catch { }
            if (!string.IsNullOrWhiteSpace(body) && body.Length > 500) body = body[..500] + "…";
            logger.LogWarning("[SABnzbd] Add file failed HTTP {Status}. Body: {Body}", (int)resp.StatusCode, body);
            serviceLogger.Warn($"Add file failed HTTP {(int)resp.StatusCode}. Body: {body}");
        }
        return ok;
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "[SABnzbd] addfile failed");
        serviceLogger.Error($"Add file exception: {ex.Message}");
        return false;
    }
}

public async Task<bool> IsJobCompleteAsync(string nzbName, CancellationToken cancellationToken)
{
    var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
    var apiKey = options.Value.ApiKey;
    var serviceLogger = await GetLogger();
    if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
    {
        logger.LogWarning("[SABnzbd] Missing BaseUrl or ApiKey; cannot check job status");
        serviceLogger.Warn("Missing BaseUrl or ApiKey; cannot check job status");
        return false;
    }

    try
    {
        // First check the queue
        var queueUrl = $"{baseUrl}/api?mode=queue&apikey={Uri.EscapeDataString(apiKey)}&output=json";
        logger.LogInformation("[SABnzbd] Checking queue for job '{JobName}' at {Url}", nzbName, queueUrl);
        serviceLogger.Info($"Checking queue for job '{nzbName}' at {queueUrl}");
        
        var queueResp = await httpClient.GetAsync(queueUrl, cancellationToken);
        if (queueResp.IsSuccessStatusCode)
        {
            var queueBody = await queueResp.Content.ReadAsStringAsync(cancellationToken);
            var preview = queueBody.Length > 500 ? queueBody[..500] + "..." : queueBody;
            logger.LogInformation("[SABnzbd] Queue API response (first 500 chars): {Body}", preview);
            serviceLogger.Info($"Queue API response (first 500 chars): {preview}");
            
            var queueJson = System.Text.Json.JsonDocument.Parse(queueBody);
            
            // Look for our job in the queue
            if (queueJson.RootElement.TryGetProperty("queue", out var queue) && 
                queue.TryGetProperty("slots", out var slots))
            {
                var slotCount = slots.GetArrayLength();
                logger.LogInformation("[SABnzbd] Found {SlotCount} slots in queue", slotCount);
                serviceLogger.Info($"Found {slotCount} slots in queue");
                
                foreach (var slot in slots.EnumerateArray())
                {
                    if (slot.TryGetProperty("name", out var name))
                    {
                        var jobName = name.GetString();
                        logger.LogInformation("[SABnzbd] Queue slot name: '{JobName}'", jobName);
                        serviceLogger.Info($"Queue slot name: '{jobName}'");
                        
                        if (jobName?.Contains(nzbName, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            logger.LogInformation("[SABnzbd] Found matching job in queue: '{JobName}'", jobName);
                            serviceLogger.Info($"Found matching job in queue: '{jobName}'");
                            
                            // Found our job, check its status
                            if (slot.TryGetProperty("status", out var status))
                            {
                                var statusStr = status.GetString()?.ToLowerInvariant();
                                var isComplete = statusStr == "completed" || statusStr == "finished";
                                logger.LogInformation("[SABnzbd] Job '{JobName}' status: '{Status}', complete: {Complete}", jobName, statusStr, isComplete);
                                serviceLogger.Info($"Job '{jobName}' status: '{statusStr}', complete: {isComplete}");
                                return isComplete;
                            }
                            else
                            {
                                logger.LogWarning("[SABnzbd] Job '{JobName}' found but no status property", jobName);
                                serviceLogger.Warn($"Job '{jobName}' found but no status property");
                            }
                        }
                    }
                }
            }
        }
        else
        {
            logger.LogWarning("[SABnzbd] Queue check failed HTTP {Status}", (int)queueResp.StatusCode);
            serviceLogger.Warn($"Queue check failed HTTP {(int)queueResp.StatusCode}");
        }

        // Now check the history separately
        var historyUrl = $"{baseUrl}/api?mode=history&apikey={Uri.EscapeDataString(apiKey)}&output=json";
        logger.LogInformation("[SABnzbd] Checking history for job '{JobName}' at {Url}", nzbName, historyUrl);
        serviceLogger.Info($"Checking history for job '{nzbName}' at {historyUrl}");
        
        var historyResp = await httpClient.GetAsync(historyUrl, cancellationToken);
        if (historyResp.IsSuccessStatusCode)
        {
            var historyBody = await historyResp.Content.ReadAsStringAsync(cancellationToken);
            var historyPreview = historyBody.Length > 500 ? historyBody[..500] + "..." : historyBody;
            logger.LogInformation("[SABnzbd] History API response (first 500 chars): {Body}", historyPreview);
            serviceLogger.Info($"History API response (first 500 chars): {historyPreview}");
            
            var historyJson = System.Text.Json.JsonDocument.Parse(historyBody);
            
            if (historyJson.RootElement.TryGetProperty("history", out var history) && 
                history.TryGetProperty("slots", out var historySlots))
            {
                var historySlotCount = historySlots.GetArrayLength();
                logger.LogInformation("[SABnzbd] Found {SlotCount} slots in history", historySlotCount);
                serviceLogger.Info($"Found {historySlotCount} slots in history");
                
                foreach (var slot in historySlots.EnumerateArray())
                {
                    if (slot.TryGetProperty("name", out var name))
                    {
                        var jobName = name.GetString();
                        logger.LogInformation("[SABnzbd] History slot name: '{JobName}'", jobName);
                        serviceLogger.Info($"History slot name: '{jobName}'");
                        
                        if (jobName?.Contains(nzbName, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            logger.LogInformation("[SABnzbd] Job '{JobName}' found in history (completed)", jobName);
                            serviceLogger.Info($"Job '{jobName}' found in history (completed)");
                            return true;
                        }
                    }
                }
            }
            else
            {
                logger.LogWarning("[SABnzbd] History response missing expected properties");
                serviceLogger.Warn("History response missing expected properties");
            }
        }
        else
        {
            logger.LogWarning("[SABnzbd] History check failed HTTP {Status}", (int)historyResp.StatusCode);
            serviceLogger.Warn($"History check failed HTTP {(int)historyResp.StatusCode}");
        }

        logger.LogInformation("[SABnzbd] Job '{JobName}' not found in queue or history", nzbName);
        serviceLogger.Info($"Job '{nzbName}' not found in queue or history");
        return false;
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "[SABnzbd] Job status check failed");
        serviceLogger.Warn($"Job status check failed: {ex.Message}");
        return false;
    }
}

public async Task<string?> GetJobStatusAsync(string nzbName, CancellationToken cancellationToken)
{
    var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
    var apiKey = options.Value.ApiKey;
    var serviceLogger = await GetLogger();
    if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
    {
        serviceLogger.Warn("Missing BaseUrl or ApiKey; cannot get job status");
        return null;
    }

    try
    {
        // Get the queue to find our job
        var url = $"{baseUrl}/api?mode=queue&apikey={Uri.EscapeDataString(apiKey)}&output=json";
        serviceLogger.Info($"GetJobStatus: GET {url}");
        
        var resp = await httpClient.GetAsync(url, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            serviceLogger.Warn($"GetJobStatus queue HTTP {(int)resp.StatusCode}");
            return null;
        }

        var body = await resp.Content.ReadAsStringAsync(cancellationToken);
        var json = System.Text.Json.JsonDocument.Parse(body);
        
        // Look for our job in the queue
        if (json.RootElement.TryGetProperty("queue", out var queue) && 
            queue.TryGetProperty("slots", out var slots))
        {
            foreach (var slot in slots.EnumerateArray())
            {
                if (slot.TryGetProperty("name", out var name) && 
                    name.GetString()?.Contains(nzbName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (slot.TryGetProperty("status", out var status))
                    {
                        var s = $"queue:{status.GetString()}";
                        serviceLogger.Info($"GetJobStatus: {s}");
                        return s;
                    }
                }
            }
        }

        // Job not found in queue, might be completed and moved to history
        if (json.RootElement.TryGetProperty("history", out var history) && 
            history.TryGetProperty("slots", out var historySlots))
        {
            foreach (var slot in historySlots.EnumerateArray())
            {
                if (slot.TryGetProperty("name", out var name) && 
                    name.GetString()?.Contains(nzbName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    serviceLogger.Info("GetJobStatus: history:completed");
                    return "history:completed";
                }
            }
        }

        serviceLogger.Info("GetJobStatus: not_found");
        return "not_found";
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "[SABnzbd] Job status check failed");
        serviceLogger.Warn($"GetJobStatus failed: {ex.Message}");
        return null;
    }
}

private static string SanitizeUrlForLogs(string url)
{
    try
    {
        return System.Text.RegularExpressions.Regex.Replace(
            url,
            "(?i)([?&](?:apikey|api_key|apiKey)=)[^&#]+",
            "$1***"
        );
    }
    catch
    {
        return url;
    }
}

public async Task<(bool ok, string message)> TestConnectivityAsync(CancellationToken cancellationToken)
{
    var baseUrl = options.Value.BaseUrl?.TrimEnd('/');
    var apiKey = options.Value.ApiKey;
    if (string.IsNullOrWhiteSpace(baseUrl)) return (false, "BaseUrl not configured");
    if (string.IsNullOrWhiteSpace(apiKey)) return (false, "API key not configured");
    var serviceLogger = await GetLogger();

    var url = $"{baseUrl}/api?mode=version&output=json&apikey={Uri.EscapeDataString(apiKey)}";
    try
    {
        serviceLogger.Info($"Test connectivity: GET {url}");
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        var resp = await httpClient.SendAsync(req, cancellationToken);
        var msg = $"HTTP {(int)resp.StatusCode}";
        serviceLogger.Info($"Result: {msg}");
        return (resp.IsSuccessStatusCode, msg);
    }
    catch (Exception ex)
    {
        serviceLogger.Error($"Exception: {ex.Message}");
        return (false, ex.Message);
    }
}
}


