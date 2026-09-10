using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Configurations;

public class BotSubscriberConfiguration : IEntityTypeConfiguration<BotSubscriber>
{
    public void Configure(EntityTypeBuilder<BotSubscriber> builder)
    {
        builder.ToTable("Subscribers");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();
        
        builder.Property(x => x.ChatId)
            .HasColumnName("ChatId")
            .IsRequired();
        
        builder.Property(x => x.UserName)
            .HasColumnName("UserName")
            .IsRequired(false);
        
        builder.Property(x => x.SubscribedAt)
            .HasColumnName("SubscribedAt")
            .IsRequired();
        
        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();
        
        builder.HasIndex(x => x.ChatId)
            .IsUnique();
    }
}