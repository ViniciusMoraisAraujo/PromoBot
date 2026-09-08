using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromoBot.Domain.Entities;

namespace PromoBot.Infrastructure.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("Promotions");
        
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("Id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Description)
            .HasColumnName("Description")
            .IsRequired();
        
        builder.Property(p => p.Url)
            .HasColumnName("Url")
            .IsRequired(false);
        
        builder.Property(p => p.MessageId)
            .HasColumnName("IdMessage")
            .IsRequired();
        
        builder.Property(p => p.ChatId)
            .HasColumnName("ChatId")
            .IsRequired();
        
        builder.Property(p => p.PromotionDate)
            .HasColumnName("PromotionDate")
            .IsRequired();
        
        builder.Property(p => p.Price)
            .HasColumnName("Price")
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);
        
        builder.Property(p => p.IsNotify)
            .HasColumnName("IsNotify")
            .IsRequired();
        
        builder.HasIndex(p => new { IdMessage = p.MessageId, p.ChatId })
            .IsUnique();
    }
}