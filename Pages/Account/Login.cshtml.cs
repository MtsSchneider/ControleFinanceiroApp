using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies; // Adicionado para garantir a referência

namespace ControleFinanceiroApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher = new PasswordHasher<Usuario>();

        public LoginModel(AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        // Modelo de dados que recebe as informações do formulário
        public class InputModel
        {
            [Required(ErrorMessage = "O E-mail é obrigatório.")]
            [EmailAddress]
            public string? Email { get; set; }

            [Required(ErrorMessage = "A Senha é obrigatória.")]
            [DataType(DataType.Password)]
            public string? Senha { get; set; }

            [Display(Name = "Lembrar-me")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Encontrar o usuário
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == Input.Email);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciais inválidas.");
                return Page();
            }

            // 2. Verificar a senha (Compara a senha digitada com o Hash no BD)
            var result = _passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash!, Input.Senha!);

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Credenciais inválidas.");
                return Page();
            }
            
            // 3. Criar os CLAMS (Identidade e Permissões)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email!),
                new Claim(ClaimTypes.Name, usuario.Nome!),
                new Claim("RendaExtra", usuario.TipoRendaExtra!) 
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(24)
            };

            // 4. Efetuar o LOGIN!
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // 5. Redirecionar
            return LocalRedirect(ReturnUrl);
        }
    }
}