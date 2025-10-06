using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ControleFinanceiroApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// Protege a página: só acessível se o usuário estiver logado
[Authorize] 
public class IndexModel : PageModel
{
    private readonly AppDbContext _context;

    // Propriedades que serão exibidas na tela (o ViewModel do Dashboard)
    public decimal SaldoAtual { get; set; } = 0;
    public decimal SaldoPrevisto { get; set; } = 0;
    public List<ControleFinanceiroApp.Models.Lancamento> DebitosProximos { get; set; } = new List<ControleFinanceiroApp.Models.Lancamento>();

    // Construtor: o DbContext é "injetado" aqui.
    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        // 1. Obter o ID do usuário logado
        // O ClaimTypes.NameIdentifier é o ID que salvamos no cookie durante o login.
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            // Isso não deve acontecer se [Authorize] estiver funcionando, mas é uma segurança.
            return; 
        }
        int userId = int.Parse(userIdString);

        // 2. Calcular o Saldo Atual (Considerando apenas o que já está 'Pago')
        var lancamentosPagos = await _context.Lancamentos
            .Where(l => l.UsuarioId == userId && l.Status == "Pago")
            .ToListAsync();

        foreach (var lancamento in lancamentosPagos)
        {
            if (lancamento.Tipo == "Receita")
            {
                SaldoAtual += lancamento.Valor;
            }
            else // Tipo == "Despesa"
            {
                SaldoAtual -= lancamento.Valor;
            }
        }
        
        // 3. Calcular o Saldo Previsto (Soma o que está 'Pago' e o que está 'Pendente')
        var todosLancamentos = await _context.Lancamentos
            .Where(l => l.UsuarioId == userId)
            .ToListAsync();

        foreach (var lancamento in todosLancamentos)
        {
            if (lancamento.Tipo == "Receita")
            {
                SaldoPrevisto += lancamento.Valor;
            }
            else // Tipo == "Despesa"
            {
                SaldoPrevisto -= lancamento.Valor;
            }
        }

        // 4. Buscar Débitos Próximos a Vencer (Débitos Pendentes nos próximos 30 dias)
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