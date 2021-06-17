﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accord.Domain.Model
{
    public class RunOption
    {
        public RunOptionType Type { get; set; }
        public string Value { get; set; } = null!;
    }

    public class RunOptionEntityTypeConfiguration : IEntityTypeConfiguration<RunOption>
    {
        public void Configure(EntityTypeBuilder<RunOption> builder)
        {
            builder
                .HasKey(x => x.Type);

            builder.HasData(new RunOption()
            {
                Type = RunOptionType.RaidModeEnabled,
                Value = "False"
            }, new RunOption()
            {
                Type = RunOptionType.AutoRaidModeEnabled,
                Value = "False"
            }, new RunOption()
            {
                Type = RunOptionType.JoinsToTriggerRaidModePerMinute,
                Value = "10"
            });
        }
    }
    
    public enum RunOptionType
    {
        RaidModeEnabled = 0,
        AutoRaidModeEnabled = 1,
        JoinsToTriggerRaidModePerMinute = 2,
    }
}