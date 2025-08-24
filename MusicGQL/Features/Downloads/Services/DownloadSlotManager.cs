using System.Collections.Concurrent;
using MusicGQL.Features.ServerSettings;

namespace MusicGQL.Features.Downloads.Services;

public class DownloadSlotManager(
    ILogger<DownloadSlotManager> logger,
    ServerSettingsAccessor serverSettingsAccessor,
    IServiceScopeFactory scopeFactory,
    CurrentDownloadStateService currentDownloadStateService,
    MusicGQL.Features.ServerLibrary.Cache.ServerLibraryCache cache,
    DownloadLogPathProvider logPathProvider
) : BackgroundService, IDownloadSlotManager
{
    private readonly ConcurrentDictionary<int, DownloadSlot> _slots = new();
    private readonly ConcurrentQueue<DownloadQueueItem> _workQueue = new();
    private readonly SemaphoreSlim _queueSemaphore = new(1, 1);
    private readonly SemaphoreSlim _slotSemaphore = new(1, 1);
    private CurrentDownloadStateService _currentDownloadStateService;
    private readonly MusicGQL.Features.ServerLibrary.Cache.ServerLibraryCache _cache = cache;
    private readonly DownloadLogPathProvider _logPathProvider = logPathProvider;

    private int _nextSlotId = 0;
    private bool _isInitialized = false;

    public IReadOnlyDictionary<int, DownloadSlot> Slots => _slots.AsReadOnly();
    public int QueueLength => _workQueue.Count;
    public int ActiveSlotCount => _slots.Values.Count(s => s.IsActive);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[DownloadSlotManager] Starting with configurable slots");
        logger.LogInformation(
            "[DownloadSlotManager] Instance created. Initial _slots.Count: {Count}",
            _slots.Count
        );

        // Initialize the current download state service reference
        _currentDownloadStateService = currentDownloadStateService;

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
        if (_isInitialized)
        {
            logger.LogDebug(
                "[DownloadSlotManager] Already initialized with {SlotCount} slots",
                _slots.Count
            );
            return;
        }

        try
        {
            var settings = await serverSettingsAccessor.GetAsync();
            var slotCount = settings.DownloadSlotCount; // Default to 3 slots

            logger.LogInformation(
                "[DownloadSlotManager] Initializing {SlotCount} download slots",
                slotCount
            );

            for (int i = 0; i < slotCount; i++)
            {
                var slotId = Interlocked.Increment(ref _nextSlotId);
                var slot = new DownloadSlot(slotId, logger, scopeFactory);
                _slots[slotId] = slot;

                // Start the slot
                _ = Task.Run(() => slot.StartAsync(cancellationToken), cancellationToken);
            }

            _isInitialized = true;
            logger.LogInformation(
                "[DownloadSlotManager] Initialized {SlotCount} slots",
                _slots.Count
            );

            // Publish initial slot status updates
            foreach (var slot in _slots.Values)
            {
                await PublishSlotStatusUpdateAsync(
                    slot.Id,
                    slot.IsActive,
                    slot.CurrentWork,
                    cancellationToken
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[DownloadSlotManager] Failed to initialize slots");
        }
    }

    public async Task NotifySlotWorkCompletedAsync(int slotId, CancellationToken cancellationToken)
    {
        if (_slots.TryGetValue(slotId, out var slot))
        {
            await PublishSlotStatusUpdateAsync(
                slotId,
                slot.IsActive,
                slot.CurrentWork,
                cancellationToken
            );
        }
    }

    private async Task ManageSlotsAsync(CancellationToken cancellationToken)
    {
        // Check if any slots need work - look for slots that are active but not currently working
        var idleSlots = _slots.Values.Where(s => s.IsActive && !s.IsWorking).ToList();

        if (idleSlots.Any() && _workQueue.Count > 0)
        {
            logger.LogInformation(
                "[DownloadSlotManager] Found {IdleSlots} idle slots and {QueueLength} items in queue - attempting to assign work",
                idleSlots.Count,
                _workQueue.Count
            );
        }
        else if (_workQueue.Count > 0)
        {
            logger.LogDebug(
                "[DownloadSlotManager] Queue has {QueueLength} items but no idle slots available",
                _workQueue.Count
            );
        }
        else if (idleSlots.Any())
        {
            logger.LogDebug(
                "[DownloadSlotManager] Found {IdleSlots} idle slots but queue is empty",
                idleSlots.Count
            );
        }

        foreach (var slot in idleSlots)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // Try to assign work to this slot
            if (await TryAssignWorkToSlotAsync(slot, cancellationToken))
            {
                logger.LogDebug("[DownloadSlotManager] Assigned work to slot {SlotId}", slot.Id);
            }
        }
    }

    private async Task<bool> TryAssignWorkToSlotAsync(
        DownloadSlot slot,
        CancellationToken cancellationToken
    )
    {
        await _queueSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (_workQueue.TryDequeue(out var workItem))
            {
                logger.LogDebug(
                    "[DownloadSlotManager] Attempting to assign work {ArtistId}/{Release} to slot {SlotId}",
                    workItem.ArtistId,
                    workItem.ReleaseFolderName,
                    slot.Id
                );

                // Check if any other slot is already working on this release
                var isAlreadyBeingProcessed = _slots
                    .Values.Where(s => s.Id != slot.Id)
                    .Any(s =>
                        s.IsWorking
                        && s.CurrentWork?.ArtistId == workItem.ArtistId
                        && s.CurrentWork?.ReleaseFolderName == workItem.ReleaseFolderName
                    );

                if (isAlreadyBeingProcessed)
                {
                    // Put it back in the queue for later
                    _workQueue.Enqueue(workItem);
                    logger.LogDebug(
                        "[DownloadSlotManager] Release {ArtistId}/{Release} already being processed, re-queuing",
                        workItem.ArtistId,
                        workItem.ReleaseFolderName
                    );
                    await LogToReleaseAsync(
                        workItem,
                        "[Queue] Already being processed; re-queued for later",
                        cancellationToken
                    );
                    return false;
                }

                // Assign work to the slot
                await slot.AssignWorkAsync(workItem, cancellationToken);
                await LogToReleaseAsync(
                    workItem,
                    $"[Queue] Assigned to slot {slot.Id}",
                    cancellationToken
                );

                // Publish slot status update
                await PublishSlotStatusUpdateAsync(
                    slot.Id,
                    slot.IsActive,
                    slot.CurrentWork,
                    cancellationToken
                );

                logger.LogInformation(
                    "[DownloadSlotManager] Successfully assigned work {ArtistId}/{Release} to slot {SlotId}",
                    workItem.ArtistId,
                    workItem.ReleaseFolderName,
                    slot.Id
                );
                return true;
            }
            else
            {
                logger.LogDebug(
                    "[DownloadSlotManager] No work available in queue for slot {SlotId}",
                    slot.Id
                );
            }
            return false;
        }
        finally
        {
            _queueSemaphore.Release();
        }
    }

    private async Task PublishSlotStatusUpdateAsync(
        int slotId,
        bool isActive,
        DownloadQueueItem? currentWork,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _currentDownloadStateService.PublishSlotStatusUpdateAsync(
                slotId,
                isActive,
                currentWork,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish slot status update for slot {SlotId}", slotId);
        }
    }

    public bool TryDequeue(out DownloadQueueItem? item)
    {
        return _workQueue.TryDequeue(out item);
    }

    public async Task<bool> EnqueueWorkAsync(
        DownloadQueueItem item,
        CancellationToken cancellationToken
    )
    {
        // Check if any slot is already working on this release
        var isAlreadyBeingProcessed = _slots.Values.Any(s =>
            s.IsWorking
            && s.CurrentWork?.ArtistId == item.ArtistId
            && s.CurrentWork?.ReleaseFolderName == item.ReleaseFolderName
        );

        if (isAlreadyBeingProcessed)
        {
            logger.LogDebug(
                "[DownloadSlotManager] Release {ArtistId}/{Release} already being processed, skipping",
                item.ArtistId,
                item.ReleaseFolderName
            );
            await LogToReleaseAsync(
                item,
                "[Queue] Already being processed; skipping enqueue",
                cancellationToken
            );
            return false;
        }

        // Check if it's already in the queue
        var isAlreadyQueued = _workQueue.Any(q =>
            q.ArtistId == item.ArtistId && q.ReleaseFolderName == item.ReleaseFolderName
        );
        if (isAlreadyQueued)
        {
            logger.LogDebug(
                "[DownloadSlotManager] Release {ArtistId}/{Release} already in queue, skipping",
                item.ArtistId,
                item.ReleaseFolderName
            );
            await LogToReleaseAsync(
                item,
                "[Queue] Already in work queue; skipping enqueue",
                cancellationToken
            );
            return false;
        }

        await _queueSemaphore.WaitAsync(cancellationToken);
        try
        {
            _workQueue.Enqueue(item);
            logger.LogInformation(
                "[DownloadSlotManager] Enqueued work for {ArtistId}/{Release}, queue length: {QueueLength}",
                item.ArtistId,
                item.ReleaseFolderName,
                _workQueue.Count
            );
            await LogToReleaseAsync(
                item,
                $"[Queue] Added to work queue. Queue length: {_workQueue.Count}",
                cancellationToken
            );
            return true;
        }
        finally
        {
            _queueSemaphore.Release();
        }
    }

    public async Task UpdateSlotConfigurationAsync(
        int newSlotCount,
        CancellationToken cancellationToken
    )
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
                logger.LogInformation(
                    "[DownloadSlotManager] Removing {Count} slots",
                    slotsToRemove
                );

                var slotsToStop = _slots
                    .Values.Where(s => !s.IsWorking)
                    .Take(slotsToRemove)
                    .ToList();

                foreach (var slot in slotsToStop)
                {
                    await slot.StopAsync(cancellationToken);
                    _slots.TryRemove(slot.Id, out _);
                }
            }

            logger.LogInformation(
                "[DownloadSlotManager] Updated to {SlotCount} slots",
                _slots.Count
            );
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
        var slots = _slots
            .Values.Select(slot => new DownloadSlotInfo(
                slot.Id,
                slot.IsActive,
                slot.IsWorking,
                slot.CurrentWork,
                slot.CurrentProgress,
                slot.StartedAt,
                slot.LastActivityAt,
                slot.Status
            ))
            .ToList();

        logger.LogInformation(
            "[DownloadSlotManager] GetSlotsInfo called, returning {Count} slots. IsInitialized: {IsInitialized}",
            slots.Count,
            _isInitialized
        );

        return slots;
    }

    private async Task LogToReleaseAsync(
        DownloadQueueItem item,
        string message,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var rel = await _cache.GetReleaseByArtistAndFolderAsync(
                item.ArtistId,
                item.ReleaseFolderName
            );
            if (rel != null)
            {
                var path = await _logPathProvider.GetReleaseLogFilePathAsync(
                    rel.ArtistName,
                    rel.Title,
                    cancellationToken
                );
                if (!string.IsNullOrWhiteSpace(path))
                {
                    using var relLogger = new DownloadLogger(path!);
                    relLogger.Info(message);
                }
            }
        }
        catch { }
    }
}
