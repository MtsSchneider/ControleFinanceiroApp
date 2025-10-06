using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc; // Necessário para IActionResult

namespace ControleFinanceiroApp.Pages.RendaExtra.Produtos
{
    [Authorize]
    public class IndexModel : PageModel
    {
        // CAMPOS DA CLASSE: ONDE AS VARIÁVEIS DEVEM SER DECLARADAS
        private readonly AppDbContext _context;
		
        private int _userId = 0;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Produto> Produtos { get; set; } = new List<Produto>();

        public async Task OnGetAsync()
        {
			// 1. OBTÉM O ID DO USUÁRIO AQUI (É SEGURO AGORA!)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return;
            _userId = int.Parse(userIdString);
			
			// O _context e _userId são acessíveis aqui
            Produtos = await _context.Produtos
                .Where(p => p.UsuarioId == _userId)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }

        // Método de exclusão (necessário para o CRUD)
		public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // CORREÇÃO: LEIA O ID AQUI NOVAMENTE!
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString); // Define _userId para o valor correto
            
            // Agora, o _userId está correto e a busca funcionará
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == _userId);

            if (produto == null)
            {
                // Se não encontrar (talvez já tenha sido excluído), apenas recarrega
                return RedirectToPage();
            }

            // ... (o restante da lógica de exclusão está correta) ...
            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return RedirectToPage(); 
        }
	}
}