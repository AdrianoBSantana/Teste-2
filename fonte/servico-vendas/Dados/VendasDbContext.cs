using Microsoft.EntityFrameworkCore;
using ServicoVendas.Modelos;

namespace ServicoVendas.Dados;

/// <summary>
/// DbContext do microserviço de Vendas.
/// </summary>
public class VendasDbContext : DbContext
{
    public VendasDbContext(DbContextOptions<VendasDbContext> options) : base(options)
    {
    }

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pedido>(entidade =>
        {
            entidade.ToTable("Pedidos");
            entidade.HasKey(p => p.Id);
            entidade.Property(p => p.Status).HasMaxLength(50).IsRequired();
            entidade.Property(p => p.ValorTotal).HasColumnType("decimal(18,2)").IsRequired();
            // SQLite não suporta GETUTCDATE(); usar CURRENT_TIMESTAMP para default UTC aproximado
            entidade.Property(p => p.DataCriacao).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entidade.HasMany(p => p.Itens)
                    .WithOne(i => i.Pedido!)
                    .HasForeignKey(i => i.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemPedido>(entidade =>
        {
            entidade.ToTable("ItensPedido");
            entidade.HasKey(i => i.Id);
            entidade.Property(i => i.Quantidade).IsRequired();
            entidade.Property(i => i.PrecoUnitario).HasColumnType("decimal(18,2)").IsRequired();
        });
    }
}
