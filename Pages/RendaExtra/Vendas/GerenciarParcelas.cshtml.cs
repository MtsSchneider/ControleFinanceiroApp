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

        // OnGetAsync: Carrega a venda e suas parcelas
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // 1. Obtém o ID do usuário de forma segura
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            if (id == null) return NotFound();

            // 2. Busca a venda, garantindo que pertence ao usuário
            var venda = await _context.Vendas
                .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == _userId);

            if (venda == null) return NotFound();

            // 3. Carrega as parcelas relacionadas a esta venda, ordenadas por número
            Parcelas = await _context.Parcelas
                .Where(p => p.VendaId == venda.Id)
                .OrderBy(p => p.NumeroParcela)
                .ToListAsync();

            Venda = venda;
            return Page();
        }

        // OnPostPagar: Registra o pagamento de uma parcela
        public async Task<IActionResult> OnPostPagarAsync(int parcelaId)
        {
            // 1. Obtém o ID do usuário
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            // 2. Busca a parcela E garante que pertence ao usuário via Venda (Include(p => p.Venda))
            var parcela = await _context.Parcelas
                .Include(p => p.Venda)
                .FirstOrDefaultAsync(p => p.Id == parcelaId && p.Venda!.UsuarioId == _userId);

            if (parcela == null) return NotFound();
            
            // 3. Verifica se já está paga (evita double-click)
            if (parcela.Status == "Paga")
            {
                 return RedirectToPage(new { id = parcela.Venda!.Id }); // Recarrega a página
            }

            // 4. Marca a parcela como paga e registra a data
            parcela.Status = "Paga";
            parcela.DataPagamento = DateTime.Today;

            // 5. Atualiza o saldo devedor na Venda
            var venda = parcela.Venda;
            venda!.SaldoDevedor -= parcela.ValorParcela;

            // 6. Verifica se todas as parcelas foram pagas (para atualizar o StatusVenda)
            // Usa CountAsync para checar rapidamente quantas parcelas 'Aberta' restam
            var parcelasAbertas = await _context.Parcelas
                .CountAsync(p => p.VendaId == venda.Id && p.Status == "Aberta");

            if (parcelasAbertas == 0)
            {
                venda.StatusVenda = "Pago";
            }

            // 7. Salva todas as alterações
            await _context.SaveChangesAsync();

            // Retorna para a mesma página de gerenciamento
            return RedirectToPage(new { id = venda.Id });
        }
    }
}