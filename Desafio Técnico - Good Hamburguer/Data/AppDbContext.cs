using Desafio_Técnico___Good_Hamburguer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Desafio_Técnico___Good_Hamburguer.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Subtotal).HasColumnType("numeric(10,2)");
            entity.Property(x => x.DiscountPercentage).HasColumnType("numeric(5,2)");
            entity.Property(x => x.DiscountAmount).HasColumnType("numeric(10,2)");
            entity.Property(x => x.Total).HasColumnType("numeric(10,2)");
            entity.HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItemEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Price).HasColumnType("numeric(10,2)");
        });
    }
}
