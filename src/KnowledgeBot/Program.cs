using KnowledgeBot;
using KnowledgeBot.Bots;
using KnowledgeBot.Dialogs;
using KnowledgeBot.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
});

string connectionString = builder.Configuration["AppConfig"];

// Load configuration from Azure App Configuration
builder.Configuration.AddAzureAppConfiguration(options => 
{
    options.Connect(builder.Configuration["AppConfig"])
           .Select("KnowledgeBase")
           .ConfigureRefresh(refreshOptions =>
                   refreshOptions.Register("KnowledgeBase", refreshAll: true));         
});

builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

builder.Services.AddSingleton<ILanguageService,LanguageService>();
builder.Services.AddSingleton<ICosmosDbRepository, CosmosDbRepository>();

builder.Services.RegisterSemanticKernel(builder.Configuration);

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

builder.Services.AddSingleton<IChatService, ChatService>();

builder.Services.RegisterState();

// Register all dialog
builder.Services.AddSingleton<GreetingDialog>();
builder.Services.AddSingleton<KnowledgeDialog>();
builder.Services.AddSingleton<ExtendedSearchDialog>();

builder.Services.AddSingleton<MainDialog>();

builder.Services.AddTransient<IBot, KnowledgeBot<MainDialog>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseWebSockets()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.Run();
