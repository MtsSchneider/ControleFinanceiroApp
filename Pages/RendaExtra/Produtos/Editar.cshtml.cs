using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations; 

namespace ControleFinanceiroApp.Pages.RendaExtra.Produtos
{
    [Authorize]
    public class EditarModel : PageModel
    {
        private readonly AppDbContext _context;
        private int _userId = 0; 

        public EditarModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Produto Produto { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            if (id == null)
            {
                return NotFound();
            }

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == _userId);

            if (produto == null)
            {
                return NotFound();
            }
            
            Produto = produto;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);
            
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
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