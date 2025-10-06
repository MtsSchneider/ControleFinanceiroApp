using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;
using System.Linq;

namespace ControleFinanceiroApp.Pages.RendaExtra.Vendas
{
    [Authorize]
    public class GerenciarParcelasModel : PageModel
    {
        private readonly AppDbContext _context;
        private int _userId = 0;

        public GerenciarParcelasModel(AppDbContext context)
        {
            _context = context;
        }

        public Venda Venda { get; set; } = default!;
        public IList<Parcela> Parcelas { get; set; } = new List<Parcela>();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            if (id == null) return NotFound();

            var venda = await _context.Vendas
                .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == _userId);

            if (venda == null) return NotFound();

            Parcelas = await _context.Parcelas
                .Where(p => p.VendaId == venda.Id)
                .OrderBy(p => p.NumeroParcela)
                .ToListAsync();

            Venda = venda;
            return Page();
        }

        public async Task<IActionResult> OnPostPagarAsync(int parcelaId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);
            var parcela = await _context.Parcelas
                .Include(p => p.Venda)
                .FirstOrDefaultAsync(p => p.Id == parcelaId && p.Venda!.UsuarioId == _userId);

            if (parcela == null) return NotFound();
            
            if (parcela.Status == "Paga")
            {
                 return RedirectToPage(new { id = parcela.Venda!.Id });
            }

            parcela.Status = "Paga";
            parcela.DataPagamento = DateTime.Today;

            var venda = parcela.Venda;
            venda!.SaldoDevedor -= parcela.ValorParcela;

            var parcelasAbertas = await _context.Parcelas
                .CountAsync(p => p.VendaId == venda.Id && p.Status == "Aberta");

            if (parcelasAbertas == 0)
            {
                venda.StatusVenda = "Pago";
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = venda.Id });
        }
    }
}