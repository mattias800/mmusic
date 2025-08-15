using System.Collections.Concurrent;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadSlotManager(
    ILogger<DownloadSlotManager> logger,
    ServerSettingsAccessor serverSettingsAccessor,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    private readonly ConcurrentDictionary<int, DownloadSlot> _slots = new();
    private readonly ConcurrentQueue<DownloadQueueItem> _workQueue = new();
    private readonly SemaphoreSlim _queueSemaphore = new(1, 1);
    private readonly SemaphoreSlim _slotSemaphore = new(1, 1);
    
    private int _nextSlotId = 0;
    private bool _isInitialized = false;

    public IReadOnlyDictionary<int, DownloadSlot> Slots => _slots.AsReadOnly();
    public int QueueLength => _workQueue.Count;
    public int ActiveSlotCount => _slots.Values.Count(s => s.IsActive);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[DownloadSlotManager] Starting with configurable slots");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await InitializeSlotsIfNeededAsync(stoppingToken);
                await ManageSlotsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[DownloadSlotManager] Error in main loop");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task InitializeSlotsIfNeededAsync(CancellationToken cancellationToken)
    {
        if (_isInitialized) return;

        try
        {
            var settings = await serverSettingsAccessor.GetAsync();
            var slotCount = settings.DownloadSlotCount; // Default to 3 slots
            
            logger.LogInformation("[DownloadSlotManager] Initializing {SlotCount} download slots", slotCount);
            
            for (int i = 0; i < slotCount; i++)
            {
                var slotId = Interlocked.Increment(ref _nextSlotId);
                var slot = new DownloadSlot(slotId, logger, scopeFactory);
                _slots[slotId] = slot;
                
                // Start the slot
                _ = Task.Run(() => slot.StartAsync(cancellationToken), cancellationToken);
            }
            
            _isInitialized = true;
            logger.LogInformation("[DownloadSlotManager] Initialized {SlotCount} slots", _slots.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[DownloadSlotManager] Failed to initialize slots");
        }
    }

    private async Task ManageSlotsAsync(CancellationToken cancellationToken)
    {
        // Check if any slots need work
        var idleSlots = _slots.Values.Where(s => !s.IsActive && !s.IsWorking).ToList();
        
        foreach (var slot in idleSlots)
        {
            if (cancellationToken.IsCancellationRequested) break;
            
            // Try to assign work to this slot
            if (await TryAssignWorkToSlotAsync(slot, cancellationToken))
            {
                logger.LogDebug("[DownloadSlotManager] Assigned work to slot {SlotId}", slot.Id);
            }
        }
    }

    private async Task<bool> TryAssignWorkToSlotAsync(DownloadSlot slot, CancellationToken cancellationToken)
    {
        await _queueSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_workQueue.TryDequeue(out var workItem))
            {
                // Check if any other slot is already working on this release
                var isAlreadyBeingProcessed = _slots.Values
                    .Where(s => s.Id != slot.Id)
                    .Any(s => s.IsWorking && s.CurrentWork?.ArtistId == workItem.ArtistId && s.CurrentWork?.ReleaseFolderName == workItem.ReleaseFolderName);
                
                if (isAlreadyBeingProcessed)
                {
                    // Put it back in the queue for later
                    _workQueue.Enqueue(workItem);
                    logger.LogDebug("[DownloadSlotManager] Release {ArtistId}/{Release} already being processed, re-queuing", 
                        workItem.ArtistId, workItem.ReleaseFolderName);
                    return false;
                }
                
                // Assign work to the slot
                await slot.AssignWorkAsync(workItem, cancellationToken);
                return true;
            }
            return false;
        }
        finally
        {
            _queueSemaphore.Release();
        }
    }

    public async Task EnqueueWorkAsync(DownloadQueueItem item, CancellationToken cancellationToken)
    {
        // Check if any slot is already working on this release
        var isAlreadyBeingProcessed = _slots.Values
            .Any(s => s.IsWorking && s.CurrentWork?.ArtistId == item.ArtistId && s.CurrentWork?.ReleaseFolderName == item.ReleaseFolderName);
        
        if (isAlreadyBeingProcessed)
        {
            logger.LogDebug("[DownloadSlotManager] Release {ArtistId}/{Release} already being processed, skipping", 
                item.ArtistId, item.ReleaseFolderName);
            return;
        }
        
        // Check if it's already in the queue
        var isAlreadyQueued = _workQueue.Any(q => q.ArtistId == item.ArtistId && q.ReleaseFolderName == item.ReleaseFolderName);
        if (isAlreadyQueued)
        {
            logger.LogDebug("[DownloadSlotManager] Release {ArtistId}/{Release} already in queue, skipping", 
                item.ArtistId, item.ReleaseFolderName);
            return;
        }
        
        await _queueSemaphore.WaitAsync(cancellationToken);
        try
        {
            _workQueue.Enqueue(item);
            logger.LogInformation("[DownloadSlotManager] Enqueued work for {ArtistId}/{Release}, queue length: {QueueLength}", 
                item.ArtistId, item.ReleaseFolderName, _workQueue.Count);
        }
        finally
        {
            _queueSemaphore.Release();
        }
    }

    public async Task UpdateSlotConfigurationAsync(int newSlotCount, CancellationToken cancellationToken)
    {
        await _slotSemaphore.WaitAsync(cancellationToken);
        try
        {
            var currentCount = _slots.Count;
            
            if (newSlotCount > currentCount)
            {
                // Add new slots
                var slotsToAdd = newSlotCount - currentCount;
                logger.LogInformation("[DownloadSlotManager] Adding {Count} new slots", slotsToAdd);
                
                for (int i = 0; i < slotsToAdd; i++)
                {
                    var slotId = Interlocked.Increment(ref _nextSlotId);
                    var slot = new DownloadSlot(slotId, logger, scopeFactory);
                    _slots[slotId] = slot;
                    
                    // Start the new slot
                    _ = Task.Run(() => slot.StartAsync(cancellationToken), cancellationToken);
                }
            }
            else if (newSlotCount < currentCount)
            {
                // Remove excess slots (gracefully)
                var slotsToRemove = currentCount - newSlotCount;
                logger.LogInformation("[DownloadSlotManager] Removing {Count} slots", slotsToRemove);
                
                var slotsToStop = _slots.Values
                    .Where(s => !s.IsWorking)
                    .Take(slotsToRemove)
                    .ToList();
                
                foreach (var slot in slotsToStop)
                {
                    await slot.StopAsync(cancellationToken);
                    _slots.TryRemove(slot.Id, out _);
                }
            }
            
            logger.LogInformation("[DownloadSlotManager] Updated to {SlotCount} slots", _slots.Count);
        }
        finally
        {
            _slotSemaphore.Release();
        }
    }

    public async Task StopAllSlotsAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("[DownloadSlotManager] Stopping all slots");
        
        var stopTasks = _slots.Values.Select(slot => slot.StopAsync(cancellationToken));
        await Task.WhenAll(stopTasks);
        
        _slots.Clear();
        _isInitialized = false;
    }

    public List<DownloadQueueItem> GetQueueSnapshot()
    {
        return _workQueue.ToList();
    }

    public List<DownloadSlotInfo> GetSlotsInfo()
    {
        return _slots.Values.Select(slot => new DownloadSlotInfo(
            slot.Id,
            slot.IsActive,
            slot.IsWorking,
            slot.CurrentWork,
            slot.CurrentProgress,
            slot.StartedAt,
            slot.LastActivityAt,
            slot.Status
        )).ToList();
    }
}
