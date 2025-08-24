namespace MusicGQL.Features.External;

public record ExternalServicesSearchRoot
{
    public IEnumerable<ExternalService> All() =>
        ExternalServiceCatalog.All.Select(s => new ExternalService(s));

    public ExternalService? ById([ID] string id)
    {
        var model = ExternalServiceCatalog.GetById(id);
        return model == null ? null : new(model);
    }
}
