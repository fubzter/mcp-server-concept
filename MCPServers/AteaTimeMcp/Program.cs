using AteaTimeMcp;
using AteaTimeMcp.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}

builder.Services.AddHttpClient<AteaTimeMcpService>();

var isTransportStateless = bool.Parse(builder.Configuration["IsTransportStateless"] ?? "false");
builder.Services.AddMcpServer()
    .WithHttpTransport(options => options.Stateless = isTransportStateless)
    .WithTools<AteaTimeMcpTool>();

var app = builder.Build();

var serverUrl = builder.Configuration["ServerUrl"] ?? "http://localhost:4551";

app.MapMcp();

app.Run(serverUrl);
