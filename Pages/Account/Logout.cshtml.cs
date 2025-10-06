using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; // Necessário para a constante AuthenticationScheme

namespace ControleFinanceiroApp.Pages.Account
{
    public class LogoutModel : PageModel
    {
        // O método OnPostAsync é chamado quando o formulário é submetido.
        public async Task<IActionResult> OnPostAsync()
        {
            // O SignOutAsync diz ao sistema para remover o cookie de autenticação.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Após sair, redireciona para a página de Login.
            return RedirectToPage("/Account/Login");
        }
        
        // O método OnGet é chamado quando o usuário navega diretamente para a página.
        public IActionResult OnGet()
        {
            // Se o usuário acessar via GET, mostramos a página de confirmação.
            return Page();
        }
    }
}