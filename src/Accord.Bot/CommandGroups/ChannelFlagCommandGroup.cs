using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Accord.Bot.Helpers;
using Accord.Bot.Models;
using Accord.Domain.Model;
using Accord.Services.ChannelFlags;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Accord.Bot.CommandGroups
{
    [Group("channel-flag")]
    public class ChannelFlagCommandGroup: AccordCommandGroup
    {
        private readonly ICommandContext _commandContext;
        private readonly IMediator _mediator;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly CommandResponder _commandResponder;

        public ChannelFlagCommandGroup(ICommandContext commandContext,
            IMediator mediator, 
            IDiscordRestGuildAPI guildApi,
            CommandResponder commandResponder)
        {
            _commandContext = commandContext;
            _mediator = mediator;
            _guildApi = guildApi;
            _commandResponder = commandResponder;
        }

        [Command("add"), Description("Add flag to the current channel")]
        public async Task<Result<IUserMessage>> AddFlag(string type, IChannel? channel = null)
        {
            var isParsedEnumValue = Enum.TryParse<ChannelFlagType>(type, out var actualChannelFlag);

            if (!isParsedEnumValue || !Enum.IsDefined(actualChannelFlag))
                return new ErrorMessage("Type of flag is not found");
            
            var user = await _commandContext.ToPermissionUser(_guildApi);

            var channelId = channel?.ID.Value ?? _commandContext.ChannelID.Value;

            var response = await _mediator.Send(new AddChannelFlagRequest(user, actualChannelFlag, channelId));

            return response.Success
                ? new InfoMessage($"{actualChannelFlag} flag added")
                : new ErrorMessage(response.ErrorMessage);
        }

        [Command("remove"), Description("Add flag to the current channel")]
        public async Task<Result<IUserMessage>> RemoveFlag(string type, IChannel? channel = null)
        {
            var isParsedEnumValue = Enum.TryParse<ChannelFlagType>(type, out var actualChannelFlag);

            if (!isParsedEnumValue || !Enum.IsDefined(actualChannelFlag))
                return new ErrorMessage("Type of flag is not found");
            
            var user = await _commandContext.ToPermissionUser(_guildApi);

            var channelId = channel?.ID.Value ?? _commandContext.ChannelID.Value;

            var response = await _mediator.Send(new DeleteChannelFlagRequest(user, actualChannelFlag, channelId));

            return response.Success
                ? new InfoMessage($"{actualChannelFlag} flag removed")
                : new ErrorMessage(response.ErrorMessage);
        }
    }
}
