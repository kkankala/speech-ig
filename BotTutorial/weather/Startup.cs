// Generated with EmptyBot .NET Template version v4.22.0

using WeatherBot.Bots;
using WeatherBot.Dialogs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using Microsoft.Bot.Schema;

namespace WeatherBot;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
        });

        // Create the Bot Framework Authentication to be used with the Bot Adapter.
        services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

        // Create the Bot Adapter with error handling enabled.
        services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

        // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
        // services.AddTransient<IBot, WelcomeUserBot>();

        // Create the User state.
        services.AddSingleton<UserState>();

        //create the conversation state.
        services.AddSingleton<ConversationState>();

        //Initialize dialog information
        services.AddSingleton<UserProfileDialog>();
        services.AddSingleton<MainDialog>();

        // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
        // services.AddTransient<IBot, DialogBot<UserProfileDialog>>();
        // services.AddTransient<IBot, DialogBot<MainDialog>>();
        // services.AddTransient<IBot, AdaptiveCardBot>();
        // services.AddTransient<IBot, SuggestedActionsBot>();
        // services.AddTransient<IBot, WebhookBot>();
        services.AddTransient<IBot, ProactiveBot>();

        // Create a global hashset for our ConversationReferences
        services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

        // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
        services.AddSingleton<IStorage, MemoryStorage>();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
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

        // app.UseHttpsRedirection();
    }
}

