using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Accord.Bot.Helpers;
using Accord.Bot.Models;
using Accord.Domain.Model;
using Accord.Services.RunOptions;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Results;

namespace Accord.Bot.CommandGroups
{
    public class RunOptionCommandGroup: AccordCommandGroup
    {
        private readonly IMediator _mediator;
        private readonly CommandResponder _commandResponder;

        public RunOptionCommandGroup(IMediator mediator,
            CommandResponder commandResponder)
        {
            _mediator = mediator;
            _commandResponder = commandResponder;
        }

        [RequireUserGuildPermission(DiscordPermission.Administrator), Command("configure"), Description("Configure an option for the bot")]
        public async Task<Result<IUserMessage>> Configure(string type, string value)
        {
            if (!Enum.TryParse<RunOptionType>(type, out var actualRunOptionType) || !Enum.IsDefined(actualRunOptionType))
                return new ErrorMessage("Configuration is not found");
            
            var response = await _mediator.Send(new UpdateRunOptionRequest(actualRunOptionType, value));

            return response.Success 
                ? new InfoMessage($"{actualRunOptionType} configuration updated to {value}") 
                : new ErrorMessage(response.ErrorMessage);
        }
    }
}
