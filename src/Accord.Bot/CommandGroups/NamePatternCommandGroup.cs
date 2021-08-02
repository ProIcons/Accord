using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Accord.Bot.Helpers;
using Accord.Bot.Models;
using Accord.Domain.Model;
using Accord.Services.NamePatterns;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Accord.Bot.CommandGroups
{
    [Group("name-pattern")]
    public class NamePatternCommandGroup : AccordCommandGroup
    {
        private readonly ICommandContext _commandContext;
        private readonly IMediator _mediator;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly CommandResponder _commandResponder;

        public NamePatternCommandGroup(ICommandContext commandContext,
            IMediator mediator,
            IDiscordRestGuildAPI guildApi,
            CommandResponder commandResponder)
        {
            _commandContext = commandContext;
            _mediator = mediator;
            _guildApi = guildApi;
            _commandResponder = commandResponder;
        }

        [Command("list"), Description("List all name patterns")]
        public async Task<Result<IUserMessage>> List()
        {
            var user = await _commandContext.ToPermissionUser(_guildApi);

            var response = await _mediator.Send(new GetNamePatternsRequest());

            var blocked = response.Any(x => x.Type == PatternType.Blocked)
                ? string.Join(Environment.NewLine, response.Where(x => x.Type == PatternType.Blocked).Select(x => $"- `{x.Pattern}` [{x.OnDiscovery}]"))
                : "There are no blocked patterns";

            var allowed = response.Any(x => x.Type == PatternType.Allowed)
                ? string.Join(Environment.NewLine, response.Where(x => x.Type == PatternType.Allowed).Select(x => $"- `{x.Pattern}`"))
                : "There are no allowed patterns";

            var embed = new Embed(Title: "Name patterns",
                Description: "Allowed patterns supersede those that are blocked.",
                Fields: new EmbedField[]
                {
                    new("Blocked", blocked),
                    new("Allowed", allowed),
                });

            return new EmbedMessage(embed);
        }

        [Command("allow"), Description("Add name pattern to allow")]
        public async Task<Result<IUserMessage>> AllowPattern(string pattern)
        {
            var user = await _commandContext.ToPermissionUser(_guildApi);

            var response = await _mediator.Send(new AddNamePatternRequest(user, pattern, PatternType.Allowed, OnNamePatternDiscovery.DoNothing));
            
            return response.Success
                ? new InfoMessage($"{pattern} Allowed")
                : new ErrorMessage(response.ErrorMessage);
        }

        [Command("block"), Description("Add name pattern to block")]
        public async Task<Result<IUserMessage>> BlockPattern(string pattern, string onDiscovery)
        {
            var isParsedOnDiscovery = Enum.TryParse<OnNamePatternDiscovery>(onDiscovery, out var actualOnDiscovery);

            if (!isParsedOnDiscovery || !Enum.IsDefined(actualOnDiscovery))
            {
                return new ErrorMessage("Pattern discovery is not found");
            }
            var user = await _commandContext.ToPermissionUser(_guildApi);

            var response = await _mediator.Send(new AddNamePatternRequest(user, pattern, PatternType.Blocked, actualOnDiscovery));
            
            return response.Success
                ? new InfoMessage($"{pattern} Blocked, will {actualOnDiscovery}")
                : new ErrorMessage(response.ErrorMessage);

        }

        [Command("remove"), Description("Remove name pattern")]
        public async Task<Result<IUserMessage>> RemovePattern(string pattern)
        {
            var user = await _commandContext.ToPermissionUser(_guildApi);

            var response = await _mediator.Send(new DeleteNamePatternRequest(user, pattern));

            return response.Success
                ? new InfoMessage($"{pattern} removed")
                : new ErrorMessage(response.ErrorMessage);
        }
    }
}