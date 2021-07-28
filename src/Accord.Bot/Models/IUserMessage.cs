using System.Collections.Generic;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Accord.Bot.Models
{
    public interface IUserMessage
    {
        FileData? FileData { get; }

        MessageReference? MessageReference { get; }

        List<IMessageComponent>? MessageComponents { get; }
        
        AllowedMentions? AllowedMentions { get; }
    }
}