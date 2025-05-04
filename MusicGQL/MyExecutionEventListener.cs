using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Resolvers;

namespace MusicGQL;

public class MyExecutionEventListener(ILogger<MyExecutionEventListener> logger)
    : ExecutionDiagnosticEventListener
{
    public override void RequestError(IRequestContext context, Exception exception)
    {
        logger.LogError(exception, "A request error occurred!");
    }

    public override void ResolverError(IMiddlewareContext context, IError error)
    {
        logger.LogError(error.Exception, "Resolver error: {Message}", error.Message);
    }
}
