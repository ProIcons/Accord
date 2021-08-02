using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Accord.Bot.Helpers;
using Accord.Bot.Models;
using Accord.Domain.Model;
using Accord.Services.Permissions;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Results;

namespace Accord.Bot.CommandGroups
{
    [Group("permission")]
    public class PermissionCommandGroup : AccordCommandGroup
    {
        private readonly IMediator _mediator;
        private readonly CommandResponder _commandResponder;

        public PermissionCommandGroup(IMediator mediator,
            CommandResponder commandResponder)
        {
            _mediator = mediator;
            _commandResponder = commandResponder;
        }

        [RequireUserGuildPermission(DiscordPermission.Administrator), Command("adduser"), Description("Add permission to a user")]
        public async Task<Result<IUserMessage>> AddPermissionToMember(IGuildMember member, string type)
        {
            if (!Enum.TryParse<PermissionType>(type, out var actualPermission) || !Enum.IsDefined(actualPermission))
                return new ErrorMessage("Permission is not found");


            if (member.User.HasValue)
            {
                await _mediator.Send(new AddPermissionForUserRequest(member.User.Value.ID.Value, actualPermission));
                return new InfoMessage($"{actualPermission} permission added to {member.User.Value.ID.ToUserMention()}");
            }

            return EmptyMessage.Instance;
        }

        [RequireUserGuildPermission(DiscordPermission.Administrator), Command("addrole"), Description("Add permission to a role")]
        public async Task<Result<IUserMessage>> AddPermissionToRole(IRole role, string type)
        {
            if (!Enum.TryParse<PermissionType>(type, out var actualPermission) || !Enum.IsDefined(actualPermission))
                return new ErrorMessage("Permission is not found");

            await _mediator.Send(new AddPermissionForRoleRequest(role.ID.Value, actualPermission));
            return new InfoMessage($"{actualPermission} permission added to `{role.Name}`");
        }
    }
}