using BrasfootAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MiniBrasfoot.Infrastructure.Configurations;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.Property(team => team.IsCustom).HasDefaultValue(false);
        builder.Property(team => team.BadgeTemplate).HasMaxLength(32);
        builder.Property(team => team.PrimaryColor).HasMaxLength(7);
        builder.Property(team => team.SecondaryColor).HasMaxLength(7);
    }
}
