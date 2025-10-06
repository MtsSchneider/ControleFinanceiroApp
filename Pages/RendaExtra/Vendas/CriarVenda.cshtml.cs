using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ControleFinanceiroApp.Pages.RendaExtra.Vendas
{
    [Authorize]
    public class CriarVendaModel : PageModel
    {
        private readonly AppDbContext _context;
        private int _userId = 0; 

        public CriarVendaModel(AppDbContext context)
        {
            _context = context;
        }

        // Propriedade que recebe os dados do formulário
        [BindProperty]
        public VendaInputModel Input { get; set; } = new VendaInputModel();

        // Estrutura do formulário
        public class VendaInputModel
        {
            [Required(ErrorMessage = "O nome do comprador é obrigatório.")]
            [StringLength(100)]
            [Display(Name = "Nome do Comprador")]
            public string? NomeComprador { get; set; }

            [Required(ErrorMessage = "O valor total é obrigatório.")]
            [Range(0.01, 100000.00, ErrorMessage = "O valor deve ser positivo.")]
            [DataType(DataType.Currency)]
            [Display(Name = "Valor Total da Venda")]
            public decimal ValorTotal { get; set; }

            [Required(ErrorMessage = "O número de parcelas é obrigatório.")]
            [Range(1, 48, ErrorMessage = "A venda deve ter no mínimo 1 parcela.")]
            [Display(Name = "Número de Parcelas")]
            public int NumeroParcelas { get; set; }
            
            [Required(ErrorMessage = "A data da primeira parcela é obrigatória.")]
            [DataType(DataType.Date)]
            [Display(Name = "Data da 1ª Parcela")]
            public DateTime DataPrimeiraParcela { get; set; }
        }

        public void OnGet()
        {
            // Define a data atual como padrão para a primeira parcela
            Input.DataPrimeiraParcela = DateTime.Today.AddDays(30); // Sugere 30 dias para a primeira
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Obtém o ID do usuário de forma segura
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Cria a Venda Principal
            var novaVenda = new Venda
            {
                UsuarioId = _userId,
                NomeComprador = Input.NomeComprador,
                DataVenda = DateTime.Today,
                ValorTotal = Input.ValorTotal,
                NumeroParcelas = Input.NumeroParcelas,
                SaldoDevedor = Input.ValorTotal, // Saldo inicial é o valor total
                StatusVenda = "Pendente" 
            };
            
            // 2. Calcular e Gerar as Parcelas
            var parcelas = new List<Parcela>();
            // Divide o valor total pelo número de parcelas, arredondando para 2 casas decimais
            decimal valorParcela = Math.Round(Input.ValorTotal / Input.NumeroParcelas, 2);
            DateTime dataVencimento = Input.DataPrimeiraParcela;
            
            for (int i = 1; i <= Input.NumeroParcelas; i++)
            {
                parcelas.Add(new Parcela
                {
                    NumeroParcela = i,
                    ValorParcela = valorParcela,
                    // Adiciona i-1 meses à data da primeira parcela
                    DataVencimento = dataVencimento.AddMonths(i - 1), 
                    Status = "Aberta"
                });
            }
            
            // 3. Vínculo e Salvamento
            novaVenda.Parcelas = parcelas;
            _context.Vendas.Add(novaVenda);
            await _context.SaveChangesAsync();

            // Redireciona para a lista de devedores
            return RedirectToPage("./DevedoresIndex"); 
        }
    }
}