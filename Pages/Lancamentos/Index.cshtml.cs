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

    public IList<Lancamento> Lancamentos { get;set; } = new List<Lancamento>();

    public async Task OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return;
        int userId = int.Parse(userIdString);

        Lancamentos = await _context.Lancamentos
            .Where(l => l.UsuarioId == userId)
            .OrderByDescending(l => l.DataVencimento)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
        int userId = int.Parse(userIdString);

        var lancamento = await _context.Lancamentos
            .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == userId);

        if (lancamento == null)
        {
            return NotFound();
        }

        _context.Lancamentos.Remove(lancamento);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
}