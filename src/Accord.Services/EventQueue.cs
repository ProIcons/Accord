﻿using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Accord.Services
{
    public interface IEventQueue
    {
        ValueTask Queue(IEvent queuedEvent);
        ValueTask<IEvent> Dequeue(CancellationToken cancellationToken);
    }

    public class EventQueue : IEventQueue
    {
        private const int QUEUE_CAPACITY = 1000;
        private readonly Channel<IEvent> _queue;

        public EventQueue()
        {
            var options = new BoundedChannelOptions(QUEUE_CAPACITY)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            };

            _queue = Channel.CreateBounded<IEvent>(options);
        }

        public async ValueTask Queue(IEvent queuedEvent)
        {
            await _queue.Writer.WriteAsync(queuedEvent);
        }

        public async ValueTask<IEvent> Dequeue(
            CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }

    public interface IEvent
    {
        ulong DiscordUserId { get; }
        DateTimeOffset QueuedDateTime { get; }
    }

    public sealed record MessageSentEvent(ulong DiscordUserId, ulong DiscordChannelId, DateTimeOffset QueuedDateTime) : IEvent;

    public sealed record VoiceConnectedEvent(ulong DiscordUserId, ulong DiscordChannelId, string DiscordSessionId, DateTimeOffset QueuedDateTime) : IEvent;

    public sealed record VoiceDisconnectedEvent(ulong DiscordUserId, string DiscordSessionId, DateTimeOffset QueuedDateTime) : IEvent;
}