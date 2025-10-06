using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroApp.Pages.RendaExtra.Produtos // <--- NOVO NAMESPACE
{
    [Authorize]
    public class CriarProdutoModel : PageModel
    {
        private readonly AppDbContext _context;
        private int _userId = 0;

        public CriarProdutoModel(AppDbContext context)
        {
            _context = context;
        }

        // ... o resto do código InputModel e OnPostAsync continua aqui ...
        [BindProperty]
        public ProdutoInputModel Input { get; set; } = new ProdutoInputModel();

        public class ProdutoInputModel
        {
            [Required(ErrorMessage = "O nome do produto é obrigatório.")]
            [StringLength(100)]
            [Display(Name = "Nome do Produto")]
            public string? Nome { get; set; }
			
			
			[Required(ErrorMessage = "O código é obrigatório.")]
			[Display(Name = "Código do Produto")]
			public string? CodigoProduto { get; set; }

            [Required(ErrorMessage = "A quantidade é obrigatória.")]
            [Range(0, 100000, ErrorMessage = "A quantidade deve ser um número inteiro.")]
            [Display(Name = "Quantidade em Estoque")]
            public int QuantidadeEstoque { get; set; }

            [Required(ErrorMessage = "O preço de venda é obrigatório.")]
            [Range(0.01, 100000.00, ErrorMessage = "O preço deve ser maior que zero.")]
            [DataType(DataType.Currency)]
            [Display(Name = "Preço de Venda")]
            public decimal PrecoVenda { get; set; }
        }

        public void OnGet()
        {
            // Nada a fazer no carregamento
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                // Se por algum motivo perder o login, redireciona para a página de login
                return RedirectToPage("/Account/Login"); 
            }
            _userId = int.Parse(userIdString);
			
			if (!ModelState.IsValid)
            {
                return Page();
            }

            var novoProduto = new Produto
            {
                UsuarioId = _userId,
                Nome = Input.Nome,
                QuantidadeEstoque = Input.QuantidadeEstoque,
                PrecoVenda = Input.PrecoVenda,
				CodigoProduto = Input.CodigoProduto,
            };

            _context.Produtos.Add(novoProduto);
            await _context.SaveChangesAsync();

            // Redireciona para a lista de produtos (IndexModel)
            return RedirectToPage("./Index"); 
        }
    }
}