using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;

[Authorize]
public class ListarLancamentosModel : PageModel
{
    private readonly AppDbContext _context;
    public ListarLancamentosModel(AppDbContext context)
    {
        _context = context;
    }

    // Propriedade que irá armazenar a lista de lançamentos para exibição
    public IList<Lancamento> Lancamentos { get;set; } = new List<Lancamento>();

    // O método OnGet é chamado ao acessar a página
    public async Task OnGetAsync()
    {
        // 1. Obter o ID do usuário logado
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return;
        int userId = int.Parse(userIdString);

        // 2. Buscar todos os lançamentos para este usuário, ordenados pela data mais recente
        Lancamentos = await _context.Lancamentos
            .Where(l => l.UsuarioId == userId)
            .OrderByDescending(l => l.DataVencimento)
            .ToListAsync();
    }

    // O método OnPostDelete é chamado ao submeter o formulário de exclusão
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        // 1. Obter o ID do usuário logado para segurança
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
        int userId = int.Parse(userIdString);

        // 2. Encontrar o lançamento E garantir que pertence ao usuário logado
        var lancamento = await _context.Lancamentos
            .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == userId);

        if (lancamento == null)
        {
            // Se o lançamento não existir ou não pertencer ao usuário, retorna para a mesma página
            return NotFound();
        }

        // 3. Remover e Salvar
        _context.Lancamentos.Remove(lancamento);
        await _context.SaveChangesAsync();

        // 4. Redirecionar para recarregar a lista
        return RedirectToPage();
    }
}