using Application.Abstractions.Reporting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Infrastructure.Reporting;

internal sealed class RazorPdfGenerator(
    IRazorViewEngine razorViewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider) : IPdfGenerator
{
    public async Task<byte[]> GeneratePdfAsync<TModel>(
        string templateName,
        TModel model,
        CancellationToken cancellationToken = default)
    {
        string html = await RenderViewToStringAsync(templateName, model);
        return await ConvertHtmlToPdfAsync(html);
    }

    private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        DefaultHttpContext httpContext = new() { RequestServices = serviceProvider };
        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());

        using StringWriter sw = new();
        Microsoft.AspNetCore.Mvc.ViewEngines.ViewEngineResult viewResult = razorViewEngine.FindView(actionContext, viewName, false);

        if (!viewResult.Success)
        {
            throw new InvalidOperationException($"Could not find view: {viewName}");
        }

        var viewDictionary = new ViewDataDictionary<TModel>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(httpContext, tempDataProvider),
            sw,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }

    private static async Task<byte[]> ConvertHtmlToPdfAsync(string html)
    {
        // Download browser if not already installed
        BrowserFetcher browserFetcher = new();
        await browserFetcher.DownloadAsync();

        await using IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox", "--disable-setuid-sandbox"]
        });

        await using IPage page = await browser.NewPageAsync();
        await page.SetContentAsync(html);

        PdfOptions pdfOptions = new()
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new()
            {
                Top = "20px",
                Bottom = "20px",
                Left = "20px",
                Right = "20px"
            }
        };

        return await page.PdfDataAsync(pdfOptions);
    }
}
