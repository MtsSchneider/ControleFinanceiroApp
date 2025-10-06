using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Models;

namespace ControleFinanceiroApp.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
			:base(options)
			{
			}
			
			public DbSet<Usuario> Usuarios {get; set;}
			
			public DbSet<Lancamento> Lancamentos {get; set;}
			
			public DbSet<Produto> Produtos {get;set;}
			
			public DbSet<GanhoDiarioUber> GanhosUber {get;set;}
			
			public DbSet<Venda> Vendas {get; set;}
			
			public DbSet<Parcela> Parcelas {get;set;}
	}
}