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
    // Usaremos a própria entidade Lancamento para simplificar
    public Lancamento Lancamento { get; set; } = default!;

    // OnGetAsync: Carrega os dados para o formulário
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // 1. Obter o ID do usuário logado
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
        int userId = int.Parse(userIdString);

        // 2. Buscar o lançamento E garantir que pertence ao usuário
        var lancamento = await _context.Lancamentos
            .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == userId);

        if (lancamento == null)
        {
            return NotFound();
        }
        
        Lancamento = lancamento;
        return Page();
    }

    // OnPostAsync: Salva as alterações
    public async Task<IActionResult> OnPostAsync()
    {
        // Se a validação do modelo falhar, retorna à página
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // 1. Obter o ID do usuário logado para segurança
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int userId = int.Parse(userIdString!);
        
        // 2. Garantir que o ID do usuário no objeto não seja alterado
        Lancamento.UsuarioId = userId; 

        // 3. Atualizar no banco de dados
        _context.Attach(Lancamento).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Lógica de tratamento de erro se o registro não for encontrado
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