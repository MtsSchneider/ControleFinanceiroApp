using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ControleFinanceiroApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize] 
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    public decimal SaldoAtual { get; set; } = 0;
    public decimal SaldoPrevisto { get; set; } = 0;
    public List<ControleFinanceiroApp.Models.Lancamento> DebitosProximos { get; set; } = new List<ControleFinanceiroApp.Models.Lancamento>();

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return; 
        }
        int userId = int.Parse(userIdString);

        var lancamentosPagos = await _context.Lancamentos
            .Where(l => l.UsuarioId == userId && l.Status == "Pago")
            .ToListAsync();

        foreach (var lancamento in lancamentosPagos)
        {
            if (lancamento.Tipo == "Receita")
            {
                SaldoAtual += lancamento.Valor;
            }
            else 
            {
                SaldoAtual -= lancamento.Valor;
            }
        }
        

        var todosLancamentos = await _context.Lancamentos
            .Where(l => l.UsuarioId == userId)
            .ToListAsync();

        foreach (var lancamento in todosLancamentos)
        {
            if (lancamento.Tipo == "Receita")
            {
                SaldoPrevisto += lancamento.Valor;
            }
            else
            {
                SaldoPrevisto -= lancamento.Valor;
            }
        }

        var hoje = DateTime.Today;
        var trintaDiasApos = hoje.AddDays(30);

        DebitosProximos = await _context.Lancamentos
            .Where(l => l.UsuarioId == userId && 
                        l.Tipo == "Despesa" && 
                        l.Status == "Pendente" &&
                        l.DataVencimento >= hoje &&
                        l.DataVencimento <= trintaDiasApos)
            .OrderBy(l => l.DataVencimento)
            .ToListAsync();
    }
}