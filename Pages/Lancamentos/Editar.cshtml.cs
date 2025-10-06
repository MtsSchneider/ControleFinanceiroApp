using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;

[Authorize]
public class EditarModel : PageModel
{
    private readonly AppDbContext _context;
    public EditarModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Lancamento Lancamento { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
        int userId = int.Parse(userIdString);

        var lancamento = await _context.Lancamentos
            .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == userId);

        if (lancamento == null)
        {
            return NotFound();
        }
        
        Lancamento = lancamento;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int userId = int.Parse(userIdString!);
        
        Lancamento.UsuarioId = userId; 

        _context.Attach(Lancamento).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Lancamentos.AnyAsync(e => e.Id == Lancamento.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }
}