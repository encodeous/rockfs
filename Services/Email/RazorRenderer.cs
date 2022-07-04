using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RockFS.Services.Email;

public class RazorRenderer
{
    private readonly IRazorViewEngine _razor; 
    private readonly IHttpContextAccessor _ctx; 
    private readonly ITempDataProvider _tmpData;

    public RazorRenderer(IRazorViewEngine razor, IHttpContextAccessor ctx, ITempDataProvider tmpData)
    {
        _razor = razor;
        _ctx = ctx;
        _tmpData = tmpData;
    }

    public async Task<string> RenderViewAsync<TModel>(string viewName, TModel model)
    {
        ViewEngineResult viewEngineResult = _razor.GetView(viewName, viewName, false);
        if (viewEngineResult.View == null)
        {
            throw new Exception("Could not find the View file. Searched locations:\r\n" + string.Join("\r\n", viewEngineResult.SearchedLocations));
        }

        IView view = viewEngineResult.View;
        var actionContext = new ActionContext(_ctx.HttpContext, new RouteData(), new ActionDescriptor());

        using var outputStringWriter = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model },
            new TempDataDictionary(actionContext.HttpContext, _tmpData),
            outputStringWriter,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);

        return outputStringWriter.ToString();
    }
}