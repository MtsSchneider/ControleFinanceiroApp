using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations; // Certifique-se de que esta linha está aqui!

namespace ControleFinanceiroApp.Pages.RendaExtra.Produtos
{
    [Authorize]
    public class EditarModel : PageModel
    {
        private readonly AppDbContext _context;
        // CORREÇÃO 1: Remova o 'readonly' e inicialize com 0
        private int _userId = 0; 

        // Construtor: AGORA SÓ INJETA O CONTEXTO!
        public EditarModel(AppDbContext context)
        {
            _context = context;
            // REMOVA AQUI qualquer código que tenta ler o User
        }

        [BindProperty]
        public Produto Produto { get; set; } = default!;

        // OnGetAsync: Carrega os dados (seguro para ler o User aqui)
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // OBTEM O ID DO USUÁRIO AQUI
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString); // Seta o _userId

            if (id == null)
            {
                return NotFound();
            }

            // Busca o produto pelo ID E garante que ele pertence ao usuário logado
            var produto = await _context.Produtos
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == _userId);

            if (produto == null)
            {
                return NotFound();
            }
            
            Produto = produto;
            return Page();
        }

        // OnPostAsync: Salva as alterações (seguro para ler o User aqui)
        public async Task<IActionResult> OnPostAsync()
        {
            // OBTEM O ID DO USUÁRIO AQUI
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString); // Seta o _userId
            
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // Garante que o ID do usuário no objeto não seja alterado
            Produto.UsuarioId = _userId; 

            _context.Attach(Produto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Produtos.AnyAsync(e => e.Id == Produto.Id))
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
}