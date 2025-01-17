﻿using System;
using Accord.Services.Helpers;

namespace Accord.Services.Raid
{
    public class RaidCalculator
    {
        private DateTime? _lastJoin;
        private DateTimeOffset? _lastJoinAccountCreated;
        private int _joinsInLastRecordedCooldown;
        private int _joinsWithSimilarDegreeCreation;

        private static readonly TimeSpan JoinCooldown = TimeSpan.FromSeconds(90);

        public bool CalculateIsRaid(UserJoin userJoin, int sequentialLimit, int accountCreationSimilarityLimit)
        {
            if (_lastJoin is null 
                || (userJoin.JoinedDateTime - _lastJoin) > JoinCooldown)
            {
                _joinsInLastRecordedCooldown = 1;
            }
            else
            {
                _joinsInLastRecordedCooldown++;
            }

            var accountCreated = DiscordSnowflakeHelper.ToDateTimeOffset(userJoin.DiscordUserId);

            if (_lastJoinAccountCreated is not null)
            {
                var max = DateTimeHelper.Max(accountCreated, _lastJoinAccountCreated.Value);
                var min = DateTimeHelper.Min(accountCreated, _lastJoinAccountCreated.Value);

                if((max - min) <= TimeSpan.FromHours(1))
                {
                    _joinsWithSimilarDegreeCreation++;
                }
                else
                {
                    _joinsWithSimilarDegreeCreation = 1;
                }
            }

            _lastJoin = userJoin.JoinedDateTime;
            _lastJoinAccountCreated = accountCreated;

            return _joinsInLastRecordedCooldown >= sequentialLimit 
                   || _joinsWithSimilarDegreeCreation >= accountCreationSimilarityLimit;
        }
    }

    public sealed record UserJoin(ulong DiscordUserId, DateTime JoinedDateTime);
}