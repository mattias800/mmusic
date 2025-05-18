using MusicGQL.Db;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;

namespace MusicGQL.EventProcessor;

public abstract class EventProcessor<TAggregate>
{
    public abstract Task PrepareProcessing(EventDbContext dbContext);

    public abstract TAggregate? GetAggregate();

    public void ProcessEvent(Event ev, EventDbContext dbContext)
    {
        var a = GetAggregate();
        if (a is not null)
        {
            ProcessEvent(ev, dbContext, a);
        }
    }

    protected abstract void ProcessEvent(Event ev, EventDbContext dbContext, TAggregate aggregate);
}
