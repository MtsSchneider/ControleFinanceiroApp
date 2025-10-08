using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;

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

            // 1. Busca a venda
            var venda = await _context.Vendas
                .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == _userId);

            if (venda == null) return NotFound();

            // 2. Carrega as parcelas
            Parcelas = await _context.Parcelas
                .Where(p => p.VendaId == venda.Id)
                .OrderBy(p => p.NumeroParcela)
                .ToListAsync();

            Venda = venda;
            return Page();
        }

        // =========================================================================
        // OnPostPagarAsync: PAGA UMA PARCELA E CRIA HISTÓRICO
        // =========================================================================
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostPagarAsync(int parcelaId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            // 1. Busca a parcela e a venda relacionada
            var parcelaToUpdate = await _context.Parcelas
                .Include(p => p.Venda)
                .FirstOrDefaultAsync(p => p.Id == parcelaId && p.Venda!.UsuarioId == _userId);

            if (parcelaToUpdate == null) return NotFound();
            var vendaToUpdate = parcelaToUpdate.Venda;

            if (parcelaToUpdate.Status != "Aberta") return Page();

            // 2. Marca a parcela como paga
            parcelaToUpdate.Status = "Paga";
            parcelaToUpdate.DataPagamento = DateTime.Today;

            // 3. Atualiza o saldo devedor
            vendaToUpdate.SaldoDevedor -= parcelaToUpdate.ValorParcela;

            // 4. Verifica e atualiza o status da venda (se o saldo for zero)
            var parcelasAbertas = await _context.Parcelas
                .CountAsync(p => p.VendaId == vendaToUpdate.Id && p.Status == "Aberta");

            if (parcelasAbertas == 0)
            {
                vendaToUpdate.StatusVenda = "Pago";
                vendaToUpdate.SaldoDevedor = 0m; // Garante que o saldo seja zero
            }

            // 6. Salva as alterações
            await _context.SaveChangesAsync();

            // Redireciona para recarregar a página
            return RedirectToPage(new { id = vendaToUpdate.Id });
        }


        // =========================================================================
        // OnPostPagarCompletoAsync: PAGA TODAS AS PARCELAS E CRIA HISTÓRICO
        // =========================================================================
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostPagarCompletoAsync(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            var vendaToUpdate = await _context.Vendas
                .Include(v => v.Parcelas)
                .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == _userId);

            if (vendaToUpdate == null) return NotFound();

            if (vendaToUpdate.SaldoDevedor <= 0) return RedirectToPage(new { id = vendaToUpdate.Id });

            // O valor pago é o SaldoDevedor antes de ser zerado
            decimal valorTotalPago = vendaToUpdate.SaldoDevedor;

            // 1. Marca todas as parcelas em aberto como "Paga"
            if (vendaToUpdate.Parcelas != null)
            {
                foreach (var parcela in vendaToUpdate.Parcelas.Where(p => p.Status != "Paga"))
                {
                    parcela.Status = "Paga";
                    parcela.DataPagamento = DateTime.Today;
                }
            }

            // 2. Zera o saldo devedor e atualiza o status da venda
            vendaToUpdate.SaldoDevedor = 0m;
            vendaToUpdate.StatusVenda = "Pago";

            // 4. Salva as alterações
            await _context.SaveChangesAsync();

            // Redireciona para recarregar a página
            return RedirectToPage(new { id = vendaToUpdate.Id });
        }
    }
}