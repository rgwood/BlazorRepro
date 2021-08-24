# Blazor Hosting Repro

 I'm trying to host Blazor Server inside a desktop application (and then display it inside WebView2), but I'm running into a Razor routing failure at runtime:

 ```
 System.InvalidOperationException: Cannot find the fallback endpoint specified by route values: { page: /_Host, area:  }.
   at Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.DynamicPageEndpointMatcherPolicy.ApplyAsync(HttpContext httpContext, CandidateSet candidates)
   at Microsoft.AspNetCore.Routing.Matching.DfaMatcher.SelectEndpointWithPoliciesAsync(HttpContext httpContext, IEndpointSelectorPolicy[] policies, CandidateSet candidateSet)
   at Microsoft.AspNetCore.Routing.EndpointRoutingMiddleware.<Invoke>g__AwaitMatch|8_1(EndpointRoutingMiddleware middleware, HttpContext httpContext, Task matchTask)
   at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware.Invoke(HttpContext context)
```

I am using the latest .NET 6 ASP.NET core minimal hosting APIs, but in a way that is somewhat unusual. There is a decent chance that this is an issue with how I am hosting Blazor and not an ASP.NET bug.

## Hosting Environment

A minimal .NET 6 application that displays UI using WebView2. There is no WinForms, no WPF, [just a Win32 message pump](https://github.com/rgwood/MinimalWebView). This is the WebView project in the repo.

I have created a Blazor project named BlazorServer using the .NET 6p7 Blazor Server template project, converted it to a library project, and moved Program.cs largely as-is into a static function in `Hosting.cs`:

```cs
public static async Task<WebApplication> StartOnThreadpool() =>
    await Task.Run(async () =>
            await Task.Run(async () =>
        {
            var builder = WebApplication.CreateBuilder();
            ...
            await app.StartAsync();
            return app;
        });
```

Inside WebView.Program, I start up ASP.NET and navigate to the page:

```cs
_webApp = await BlazorServer.Hosting.StartOnThreadpool();
_controller.CoreWebView2.Navigate("http://localhost:5003/");
```
Boom:
![screenshot](screenshot.png)

## Fixes attempted

I've tried setting the ContentRootPath and ApplicationName without any luck:

```cs
string crp = @"C:\Users\reill\source\WebView\BlazorServer";
var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { ContentRootPath = crp, ApplicationName = "WebView" });
```

I've also tried enabling Trace logging but that doesn't provide any additional info.

## Version Info

- Windows 10 21H1
- Visual Studio Version 17.0.0 Preview 3.1
- .NET 6.0.100-rc.2.21423.17 (obtained from `dotnet --version`)