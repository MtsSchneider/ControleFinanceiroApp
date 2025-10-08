using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
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

        public IList<VendaDevedorViewModel> VendasPendentes { get; set; } = new List<VendaDevedorViewModel>();
        public decimal TotalDevedor { get; set; } = 0;
        public int ParcelasAtrasadas { get; set; } = 0;

        public async Task OnGetAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return;
            _userId = int.Parse(userIdString);

            var vendasBase = await _context.Vendas
                .Where(v => v.UsuarioId == _userId && v.StatusVenda != "Pago")
                .OrderByDescending(v => v.DataVenda)
                .ToListAsync();

            var listaDevedores = new List<VendaDevedorViewModel>();

            foreach (var venda in vendasBase)
            {
                // Encontra a próxima parcela aberta
                var proximaParcela = await _context.Parcelas
                    .Where(p => p.VendaId == venda.Id && p.Status == "Aberta")
                    .OrderBy(p => p.DataVencimento)
                    .FirstOrDefaultAsync();

                // Mapeia para a ViewModel
                listaDevedores.Add(new VendaDevedorViewModel
                {
                    // Mapeie todos os campos da Venda base, mais o novo campo:
                    Id = venda.Id,
                    NomeComprador = venda.NomeComprador,
                    ValorTotal = venda.ValorTotal,
                    SaldoDevedor = venda.SaldoDevedor,
                    StatusVenda = venda.StatusVenda,
                    DataVenda = venda.DataVenda,
                    NumeroParcelas = venda.NumeroParcelas,

                    // NOVO CAMPO:
                    ProximoVencimento = proximaParcela?.DataVencimento
                });
            }

            VendasPendentes = listaDevedores;

        }

        public async Task<IActionResult> OnPostDeleteVendaAsync(int id)
        {
    
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            var venda = await _context.Vendas
                .Include(v => v.Parcelas) 
                .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == _userId);

            if (venda == null) return RedirectToPage();

            _context.Vendas.Remove(venda);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public class VendaDevedorViewModel : Venda
        {
            public DateTime? ProximoVencimento { get; set; }
        }

    }
}