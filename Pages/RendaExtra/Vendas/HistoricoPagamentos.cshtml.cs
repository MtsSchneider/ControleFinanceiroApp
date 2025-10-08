using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models; // <<<< ESTE USING É CRÍTICO
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ControleFinanceiroApp.Pages.RendaExtra.Vendas
{
    [Authorize]
    public class HistoricoPagamentosModel : PageModel
    {
        private readonly AppDbContext _context;

        public HistoricoPagamentosModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<HistoricoPagamentoVenda> Historico { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            int userId = int.Parse(userIdString);

            Historico = await _context.HistoricoPagamentosVenda
                .Where(h => h.UsuarioId == userId)
                .OrderByDescending(h => h.DataPagamento)
                .ToListAsync();

            return Page();
        }
    }
}