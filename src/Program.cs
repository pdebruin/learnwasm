using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using learnwasm.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add HttpClient for MCP service
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<McpService>();

await builder.Build().RunAsync();
