namespace BlazorServer;
using BlazorServer.Data;

public static class Hosting
{
    // Start Blazor Server on a background thread
    // This is mostly copied directly from the Blazor Server .NET 6 minimal template
    public static async Task<WebApplication> StartOnThreadpool() =>
        await Task.Run(async () =>
        {
            // I have tried setting the ContentRootPath and ApplicationName without any luck - maybe I'm barking up the wrong tree?
            //string crp = @"C:\Users\reill\source\WebView\WebView\bin\Debug\net6.0-windows\";
            //string crp = @"C:\Users\reill\source\WebView\BlazorServer";
            //var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { ContentRootPath = crp, ApplicationName = "WebView" });

            var builder = WebApplication.CreateBuilder();

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();

            var app = builder.Build();
            app.Urls.Add("http://localhost:5003");

            // Configure the HTTP request pipeline.
            
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();
            
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            await app.StartAsync();
            return app;
        });
}
