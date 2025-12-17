namespace Application.Abstractions.Reporting;

public interface IPdfGenerator
{
    Task<byte[]> GeneratePdfAsync<TModel>(string templateName, TModel model, CancellationToken cancellationToken = default);
}
