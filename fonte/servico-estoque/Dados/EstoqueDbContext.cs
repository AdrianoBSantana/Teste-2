using Microsoft.EntityFrameworkCore;
using ServicoEstoque.Modelos;

namespace ServicoEstoque.Dados;

/// <summary>
/// DbContext do microserviço de Estoque.
/// </summary>
public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Produto>(entidade =>
        {
            entidade.ToTable("Produtos");
            entidade.HasKey(p => p.Id);
            entidade.Property(p => p.Nome).HasMaxLength(200).IsRequired();
            entidade.Property(p => p.Descricao).HasMaxLength(1000);
            entidade.Property(p => p.Preco).HasColumnType("decimal(18,2)").IsRequired();
            entidade.Property(p => p.QuantidadeEmEstoque).IsRequired();
            // SQLite não suporta GETUTCDATE(); usar CURRENT_TIMESTAMP para default UTC aproximado
            entidade.Property(p => p.DataCriacao).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
