using AgentService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<ILogReader, LogReader>();
builder.Configuration.AddEnvironmentVariables();
builder.Logging.AddConsole();

var host = builder.Build();
host.Run();
