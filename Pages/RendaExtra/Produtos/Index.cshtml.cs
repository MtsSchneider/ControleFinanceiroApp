using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ControleFinanceiroApp.Pages.RendaExtra.Produtos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
		
        private int _userId = 0;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Produto> Produtos { get; set; } = new List<Produto>();

        public async Task OnGetAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return;
            _userId = int.Parse(userIdString);
			
            Produtos = await _context.Produtos
                .Where(p => p.UsuarioId == _userId)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

		public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString); 
            
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == _userId);

            if (produto == null)
            {
                return RedirectToPage();
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return RedirectToPage(); 
        }
	}
}