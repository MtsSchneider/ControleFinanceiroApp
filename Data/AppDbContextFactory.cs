using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ControleFinanceiroApp.Data;

namespace ControleFinanceiroApp.Data
{
	public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
	{
		public AppDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
			
			optionsBuilder.UseSqlite("Data Source=app.db");
			
			return new AppDbContext(optionsBuilder.Options);
		}
	}
}