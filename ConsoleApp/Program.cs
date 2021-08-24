using System.Diagnostics;

Console.WriteLine("Starting Blazor...");
await BlazorServer.Hosting.StartOnThreadpool();
Console.WriteLine("Blazor started.");

// open in default web browser
new Process
{
    StartInfo = new ProcessStartInfo("http://localhost:5003")
    {
        UseShellExecute = true
    }
}.Start();

Console.WriteLine("Press any key to exit.");
Console.ReadKey();  