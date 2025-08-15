using Microsoft.Extensions.DependencyInjection;
using MusicGQL.Features.ServerLibrary.Cache;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadSlot
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public int Id { get; }
    public bool IsActive { get; private set; }
    public bool IsWorking { get; private set; }
    public DownloadQueueItem? CurrentWork { get; private set; }
    public DownloadProgress? CurrentProgress { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public string? Status { get; private set; }

    public DownloadSlot(int id, ILogger logger, IServiceScopeFactory scopeFactory)
    {
        Id = id;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (IsActive) return;
        
        IsActive = true;
        _logger.LogInformation("[DownloadSlot {SlotId}] Started", Id);
        
        try
        {
            while (IsActive && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (CurrentWork != null)
                    {
                        await ProcessWorkAsync(cancellationToken);
                    }
                    else
                    {
                        // Wait for work to be assigned
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DownloadSlot {SlotId}] Error processing work", Id);
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }
        finally
        {
            IsActive = false;
            _logger.LogInformation("[DownloadSlot {SlotId}] Stopped", Id);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        IsActive = false;
        _cancellationTokenSource.Cancel();
        
        if (CurrentWork != null)
        {
            _logger.LogInformation("[DownloadSlot {SlotId}] Stopping while processing {ArtistId}/{Release}", 
                Id, CurrentWork.ArtistId, CurrentWork.ReleaseFolderName);
        }
        
        // Wait a bit for graceful shutdown
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
        catch (OperationCanceledException) { }
    }

    public async Task AssignWorkAsync(DownloadQueueItem workItem, CancellationToken cancellationToken)
    {
        if (IsWorking)
        {
            _logger.LogWarning("[DownloadSlot {SlotId}] Cannot assign work, already working", Id);
            return;
        }
        
        CurrentWork = workItem;
        IsWorking = true;
        StartedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        Status = "Starting";
        
        _logger.LogInformation("[DownloadSlot {SlotId}] Assigned work: {ArtistId}/{Release}", 
            Id, workItem.ArtistId, workItem.ReleaseFolderName);
        
        // Start processing immediately
        _ = Task.Run(() => ProcessWorkAsync(cancellationToken), cancellationToken);
    }

    private async Task ProcessWorkAsync(CancellationToken cancellationToken)
    {
        if (CurrentWork == null) return;
        
        try
        {
            Status = "Processing";
            LastActivityAt = DateTime.UtcNow;
            
            _logger.LogInformation("[DownloadSlot {SlotId}] Starting download for {ArtistId}/{Release}", 
                Id, CurrentWork.ArtistId, CurrentWork.ReleaseFolderName);
            
            using var scope = _scopeFactory.CreateScope();
            var downloadService = scope.ServiceProvider.GetRequiredService<StartDownloadReleaseService>();
            var progressService = scope.ServiceProvider.GetRequiredService<CurrentDownloadStateService>();
            
            // Initialize progress for this slot
            CurrentProgress = new DownloadProgress
            {
                ArtistId = CurrentWork.ArtistId,
                ReleaseFolderName = CurrentWork.ReleaseFolderName,
                Status = DownloadStatus.Searching,
                TotalTracks = 0,
                CompletedTracks = 0,
                CurrentProvider = null,
                CurrentProviderIndex = 0,
                TotalProviders = 0
            };
            
            // Update progress service with slot-specific progress
            await progressService.UpdateSlotProgressAsync(Id, CurrentProgress, cancellationToken);
            
            // Start the download
            var result = await downloadService.StartAsync(
                CurrentWork.ArtistId, 
                CurrentWork.ReleaseFolderName, 
                cancellationToken);
            
            Status = "Completed";
            LastActivityAt = DateTime.UtcNow;
            
            _logger.LogInformation("[DownloadSlot {SlotId}] Completed download for {ArtistId}/{Release}", 
                Id, CurrentWork.ArtistId, CurrentWork.ReleaseFolderName);
        }
        catch (OperationCanceledException)
        {
            Status = "Cancelled";
            _logger.LogInformation("[DownloadSlot {SlotId}] Download cancelled for {ArtistId}/{Release}", 
                Id, CurrentWork?.ArtistId, CurrentWork?.ReleaseFolderName);
        }
        catch (Exception ex)
        {
            Status = "Error";
            _logger.LogError(ex, "[DownloadSlot {SlotId}] Error downloading {ArtistId}/{Release}", 
                Id, CurrentWork?.ArtistId, CurrentWork?.ReleaseFolderName);
        }
        finally
        {
            // Clear current work
            CurrentWork = null;
            CurrentProgress = null;
            IsWorking = false;
            StartedAt = null;
            Status = "Idle";
            
            // Clear progress service
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var progressService = scope.ServiceProvider.GetRequiredService<CurrentDownloadStateService>();
                await progressService.ClearSlotProgressAsync(Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[DownloadSlot {SlotId}] Error clearing progress", Id);
            }
        }
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        LastActivityAt = DateTime.UtcNow;
    }

    public void UpdateProgress(DownloadProgress progress)
    {
        CurrentProgress = progress;
        LastActivityAt = DateTime.UtcNow;
    }
}
