using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ControleFinanceiroApp.Pages.RendaExtra.Vendas
{
    [Authorize]
    public class EditarVendaModel : PageModel
    {
        private readonly AppDbContext _context;
        private int _userId;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [BindProperty]
        public Venda VendaOriginal { get; set; } = default!;

        // Flag para habilitar ou desabilitar campos no formulário
        public bool PodeEditarValores { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O nome do comprador é obrigatório.")]
            [Display(Name = "Nome do Comprador")]
            public string? NomeComprador { get; set; }

            [Required(ErrorMessage = "O valor total é obrigatório.")]
            [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo.")]
            [DataType(DataType.Currency)]
            [Display(Name = "Valor Total")]
            public decimal ValorTotal { get; set; }

            [Required(ErrorMessage = "O número de parcelas é obrigatório.")]
            [Range(1, int.MaxValue, ErrorMessage = "Deve ter pelo menos 1 parcela.")]
            [Display(Name = "Número de Parcelas")]
            public int NumeroParcelas { get; set; }
        }

        public EditarVendaModel(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            // 1. Busca a venda e as parcelas
            var venda = await _context.Vendas
                .Include(v => v.Parcelas)
                .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == _userId);

            if (venda == null) return NotFound();

            VendaOriginal = venda;
            Input.NomeComprador = venda.NomeComprador;
            Input.ValorTotal = venda.ValorTotal;
            Input.NumeroParcelas = venda.NumeroParcelas;

            // 2. Verifica se pode editar valores/parcelas
            PodeEditarValores = !venda.Parcelas.Any(p => p.Status == "Paga");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            // 1. Re-busca a venda completa
            var vendaToUpdate = await _context.Vendas
                .Include(v => v.Parcelas)
                .FirstOrDefaultAsync(v => v.Id == VendaOriginal.Id && v.UsuarioId == _userId);

            if (vendaToUpdate == null) return NotFound();

            // Seta o flag de edição para renderizar corretamente a página em caso de erro
            PodeEditarValores = !vendaToUpdate.Parcelas.Any(p => p.Status == "Paga");
            VendaOriginal = vendaToUpdate;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 2. Aplica as alterações no Nome
            vendaToUpdate.NomeComprador = Input.NomeComprador;

            // 3. Verifica e aplica alterações em Valor/Parcelas (SÓ SE NADA FOI PAGO)
            if (PodeEditarValores)
            {
                // Se o Valor ou as Parcelas mudaram
                if (vendaToUpdate.ValorTotal != Input.ValorTotal || vendaToUpdate.NumeroParcelas != Input.NumeroParcelas)
                {
                    // A. Remove todas as parcelas existentes (já que nenhuma foi paga)
                    _context.Parcelas.RemoveRange(vendaToUpdate.Parcelas);

                    // B. Atualiza os novos valores
                    vendaToUpdate.ValorTotal = Input.ValorTotal;
                    vendaToUpdate.NumeroParcelas = Input.NumeroParcelas;
                    vendaToUpdate.SaldoDevedor = Input.ValorTotal; // Reseta o saldo

                    // C. Recria as parcelas com base nos novos valores
                    var valorParcela = vendaToUpdate.ValorTotal / vendaToUpdate.NumeroParcelas;
                    var dataParcela = vendaToUpdate.DataVenda;

                    for (int i = 1; i <= vendaToUpdate.NumeroParcelas; i++)
                    {
                        dataParcela = dataParcela.AddMonths(1);

                        vendaToUpdate.Parcelas.Add(new Parcela
                        {
                            VendaId = vendaToUpdate.Id,
                            NumeroParcela = i,
                            Valor = valorParcela,
                            DataVencimento = dataParcela,
                            Status = "Aberta"
                        });
                    }
                }
            }

            // 4. Salva todas as alterações (venda e parcelas)
            await _context.SaveChangesAsync();

            return RedirectToPage("./DevedoresIndex");
        }
    }
}