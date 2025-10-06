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
    public class DevedoresIndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private int _userId = 0;

        public DevedoresIndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IList<Venda> VendasPendentes { get; set; } = new List<Venda>();
        public decimal TotalDevedor { get; set; } = 0;
        public int ParcelasAtrasadas { get; set; } = 0;

        public async Task OnGetAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return;
            _userId = int.Parse(userIdString);

            VendasPendentes = await _context.Vendas
                .Where(v => v.UsuarioId == _userId && v.StatusVenda != "Pago")
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();

            TotalDevedor = VendasPendentes.Sum(v => v.SaldoDevedor);

            ParcelasAtrasadas = await _context.Parcelas
                .Include(p => p.Venda)
                .Where(p => p.Venda!.UsuarioId == _userId && 
                            p.Status == "Aberta" && 
                            p.DataVencimento < DateTime.Today)
                .CountAsync();
        }
    }
}