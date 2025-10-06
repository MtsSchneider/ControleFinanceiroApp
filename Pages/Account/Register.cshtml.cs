using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using ControleFinanceiroApp.Models;
using ControleFinanceiroApp.Data;
using Microsoft.AspNetCore.Identity;

namespace ControleFinanceiroApp.Pages.Account
{
	public class RegisterModel : PageModel
	{
		private readonly AppDbContext _context;
		private readonly IPasswordHasher<Usuario> _passwordHasher = new PasswordHasher<Usuario>();
		public RegisterModel(AppDbContext context)
		{
			_context = context;
		}
		[BindProperty]
		public InputModel Input {get; set;} = new InputModel();
		
		public class InputModel
		{
			[Required(ErrorMessage = "O nome é obrigatório. ")]
			[StringLength(100)]
			public string? Nome {get; set;}
			
			[Required(ErrorMessage = "O E-mail é obrigatório.")]
			[EmailAddress]
			public string? Email {get;set;}
			
			[Required(ErrorMessage = "A senha é obrigatório.")]
			[DataType(DataType.Password)]
			[MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
			public string? Senha {get; set;}
			
			[DataType(DataType.Password)]
			[Display(Name = "Confirmar Senha")]
			[Compare("Senha", ErrorMessage = "As senhas não conferem.")]
			public string? ConfirmarSenha {get; set;}
			
			[Required(ErrorMessage = "Selecione um tipo de renda extra.")]
			[Display(Name = "Tipo de Renda Extra")]
			public string? TipoRendaExtra {get; set;}
		}
		
		public void OnGet()
		{
			
		}
		
		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}
			
			var emailExists = _context.Usuarios.Any(u => u.Email == Input.Email);
			if (emailExists)
			{
				ModelState.AddModelError("Input.Email", "Este e-mail já está em uso.");
				return Page();
			}
			
			var novoUsuario = new Usuario
			{
				Nome = Input.Nome,
				Email = Input.Email,
				TipoRendaExtra = Input.TipoRendaExtra
			};
			
			novoUsuario.SenhaHash = _passwordHasher.HashPassword(novoUsuario, Input.Senha!);
			
			_context.Usuarios.Add(novoUsuario);
			await _context.SaveChangesAsync();
			
			return RedirectToPage("/Index");
		}
	}
}