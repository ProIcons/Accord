using System;
using System.Linq;
using Accord.Bot.CommandGroups;
using Accord.Bot.CommandGroups.UserReports;
using Accord.Bot.Helpers;
using Accord.Bot.Helpers.Permissions;
using Accord.Bot.Parsers;
using Accord.Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Responders;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using CommandResponder = Accord.Bot.Helpers.CommandResponder;

namespace Accord.Bot.Infrastructure
{
    public static class BotServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordBot(this IServiceCollection services, IConfiguration configuration)
        {
            var discordConfigurationSection = configuration.GetSection("DiscordConfiguration");

            var token = discordConfigurationSection["BotToken"];

            services
                .Configure<DiscordConfiguration>(discordConfigurationSection);

            services
                .Configure<CommandResponderOptions>(o => o.Prefix = "!");
            
            services
                .AddLogging()
                .AddTransient<BotClient>()
                .AddSingleton<BotState>()
                .AddSingleton<DiscordCache>()
                .AddScoped<UserFeedbackService>()
                .AddScoped<DiscordAvatarHelper>()
                .AddScoped<DiscordPermissionHelper>()
                .AddScoped<DiscordScopedCache>()
                .AddScoped<DiscordChannelParser>()
                .AddScoped<CommandResponder>()
                .AddDiscordGateway(_ => token)
                .Configure<DiscordGatewayClientOptions>(o =>
                {
                    o.Intents |= GatewayIntents.GuildPresences;
                    o.Intents |= GatewayIntents.GuildVoiceStates;
                    o.Intents |= GatewayIntents.GuildMembers;
                    o.Intents |= GatewayIntents.GuildMessages;
                })
                .AddHostedService<RemindersHostedService>()
                .AddDiscordCommands(true)
                .AddExecutionEvent<CommandExecutionEventRespondHandler>()
                .AddParser<TimeSpan, TimeSpanParser>()
                .AddCommandGroup<XpCommandGroup>()
                .AddCommandGroup<ChannelFlagCommandGroup>()
                .AddCommandGroup<UserChannelHidingCommandGroup>()
                .AddCommandGroup<PermissionCommandGroup>()
                .AddCommandGroup<RunOptionCommandGroup>()
                .AddCommandGroup<ReminderCommandGroup>()
                .AddCommandGroup<NamePatternCommandGroup>()
                .AddCommandGroup<ProfileCommandGroup>()
                .AddCommandGroup<UserReportCommandGroup>()
                .AddCommandGroup<ReportCommandGroup>()
                .AddCommandGroup<LgtmCommandGroup>();

            var responderTypes = typeof(BotClient).Assembly
                .GetExportedTypes()
                .Where(t => t.IsResponder());

            foreach (var responderType in responderTypes)
            {
                services.AddResponder(responderType);
            }

            return services;
        }
    }
}