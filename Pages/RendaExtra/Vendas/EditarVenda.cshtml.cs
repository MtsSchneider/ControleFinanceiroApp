using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Collections.Generic;

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

            var venda = await _context.Vendas
                .Include(v => v.Parcelas)
                .FirstOrDefaultAsync(v => v.Id == id && v.UsuarioId == _userId);

            if (venda == null) return NotFound();

            VendaOriginal = venda;
            Input.NomeComprador = venda.NomeComprador;
            Input.ValorTotal = venda.ValorTotal;
            Input.NumeroParcelas = venda.NumeroParcelas;

            PodeEditarValores = !(venda.Parcelas ?? new List<Parcela>())
                .Any(p => p.Status == "Paga");

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            var vendaToUpdate = await _context.Vendas
                .Include(v => v.Parcelas)
                .FirstOrDefaultAsync(v => v.Id == VendaOriginal.Id && v.UsuarioId == _userId);

            if (vendaToUpdate == null) return NotFound();

            PodeEditarValores = !(vendaToUpdate.Parcelas ?? new List<Parcela>()).Any(p => p.Status == "Paga");
            VendaOriginal = vendaToUpdate;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            vendaToUpdate.NomeComprador = Input.NomeComprador;

            if (PodeEditarValores)
            {
                if (vendaToUpdate.ValorTotal != Input.ValorTotal || vendaToUpdate.NumeroParcelas != Input.NumeroParcelas)
                {
                    _context.Parcelas.RemoveRange(vendaToUpdate.Parcelas!);

                    vendaToUpdate.ValorTotal = Input.ValorTotal;
                    vendaToUpdate.NumeroParcelas = Input.NumeroParcelas;
                    vendaToUpdate.SaldoDevedor = Input.ValorTotal;

                    decimal valorParcela = vendaToUpdate.ValorTotal / vendaToUpdate.NumeroParcelas;
                    DateTime dataInicial = vendaToUpdate.DataVenda;

                    for (int i = 1; i <= vendaToUpdate.NumeroParcelas; i++)
                    {
                        DateTime dataVencimento = dataInicial.AddMonths(i);

                        vendaToUpdate.Parcelas.Add(new Parcela
                        {
                            NumeroParcela = i,
                            ValorParcela = Math.Round(valorParcela, 2),
                            DataVencimento = dataVencimento,
                            Status = "Aberta"
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./DevedoresIndex");
        }
    }
}