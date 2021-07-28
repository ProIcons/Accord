using System.Collections.Generic;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Accord.Bot.Models
{
    public abstract record BaseMessage<T>(
        FileData? FileData = default,
        MessageReference? MessageReference = default,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : IUserMessage where T : BaseMessage<T>
    {
        public static implicit operator Result<T>(BaseMessage<T> baseMessage) => Result<T>.FromSuccess((T)baseMessage);

        public static implicit operator Task<Result<T>>(BaseMessage<T> baseMessage) =>
            Task.FromResult(Result<T>.FromSuccess((T)baseMessage));

        public static implicit operator Task<Result<IUserMessage>>(BaseMessage<T> baseMessage) =>
            Task.FromResult(Result<IUserMessage>.FromSuccess((T)baseMessage));
    }
}