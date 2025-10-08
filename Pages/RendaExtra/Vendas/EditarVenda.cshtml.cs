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

        // PROPRIEDADES DE INPUT
        [BindProperty]
        [Required(ErrorMessage = "O nome do comprador é obrigatório.")]
        [Display(Name = "Nome do Comprador")]
        public string? NomeComprador { get; set; }

        [BindProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Valor Total")]
        public decimal? ValorTotal { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "O número de parcelas é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Deve ter pelo menos 1 parcela.")]
        [Display(Name = "Número de Parcelas")]
        public int NumeroParcelas { get; set; }

        [BindProperty]
        [Display(Name = "Valor da 1ª Parcela")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser positivo ou zero.")]
        public decimal? ValorPrimeiraParcela { get; set; }

        // CAMPOS OCULTOS
        [BindProperty]
        public int VendaId { get; set; }

        [BindProperty]
        public DateTime DataVendaOriginal { get; set; }

        public Venda VendaOriginal { get; set; } = default!;

        public bool PodeEditarValores { get; set; }

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
            NomeComprador = venda.NomeComprador;
            ValorTotal = venda.ValorTotal;
            NumeroParcelas = venda.NumeroParcelas;
            ValorPrimeiraParcela = venda.Parcelas?.FirstOrDefault(p => p.NumeroParcela == 1)?.ValorParcela ?? 0m;

            VendaId = venda.Id;
            DataVendaOriginal = venda.DataVenda;

            PodeEditarValores = !(venda.Parcelas ?? new List<Parcela>()).Any(p => p.Status == "Paga");

            return Page();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToPage("/Account/Login");
            _userId = int.Parse(userIdString);

            // 1. Re-busca a venda completa
            var vendaToUpdate = await _context.Vendas
                .Include(v => v.Parcelas)
                .FirstOrDefaultAsync(v => v.Id == VendaId && v.UsuarioId == _userId);

            if (vendaToUpdate == null) return NotFound();

            // Seta o flag/original para renderizar corretamente a página em caso de erro
            PodeEditarValores = !(vendaToUpdate.Parcelas ?? new List<Parcela>()).Any(p => p.Status == "Paga");
            VendaOriginal = vendaToUpdate;

            // --- Lógica de Fallback de Valores Nulos ---
            decimal novoValorTotal = ValorTotal ?? vendaToUpdate.ValorTotal;
            int novoNumeroParcelas = NumeroParcelas;
            decimal valorDaPrimeira = ValorPrimeiraParcela ?? 0m;

            ValorTotal = novoValorTotal;
            NumeroParcelas = novoNumeroParcelas;
            // ------------------------------------------

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (valorDaPrimeira > novoValorTotal)
                {
                    ModelState.AddModelError(nameof(ValorPrimeiraParcela), "O valor da primeira parcela não pode ser maior que o Valor Total.");
                    return Page();
                }

                // 2. Aplica as alterações no Nome
                vendaToUpdate.NomeComprador = NomeComprador;

                // 3. LOGICA DE FORÇA DE RECÁLCULO
                bool firstInstallmentChanged = valorDaPrimeira != (vendaToUpdate.Parcelas?.FirstOrDefault(p => p.NumeroParcela == 1)?.ValorParcela ?? 0m);

                // Recalcula se (VALOR/NUMERO MUDOU) OU SE (SÓ A 1ª PARCELA MUDOU)
                if (PodeEditarValores &&
                    (vendaToUpdate.ValorTotal != novoValorTotal ||
                     vendaToUpdate.NumeroParcelas != novoNumeroParcelas ||
                     firstInstallmentChanged)) // NOVO GATILHO
                {
                    // A. Remove e atualiza os valores
                    _context.Parcelas.RemoveRange(vendaToUpdate.Parcelas!);
                    vendaToUpdate.ValorTotal = novoValorTotal;
                    vendaToUpdate.NumeroParcelas = novoNumeroParcelas;
                    vendaToUpdate.SaldoDevedor = novoValorTotal;

                    // B. LÓGICA DE RECRIAÇÃO
                    if (valorDaPrimeira == 0m)
                    {
                        valorDaPrimeira = novoValorTotal / novoNumeroParcelas;
                    }

                    decimal valorRestante = novoValorTotal - valorDaPrimeira;
                    int parcelasRestantes = novoNumeroParcelas - 1;
                    decimal valorParcelasIguais = 0m;

                    if (parcelasRestantes > 0)
                    {
                        valorParcelasIguais = valorRestante / parcelasRestantes;
                    }
                    else if (novoNumeroParcelas == 1)
                    {
                        valorDaPrimeira = novoValorTotal;
                    }

                    DateTime dataInicial = DataVendaOriginal;

                    for (int i = 1; i <= novoNumeroParcelas; i++)
                    {
                        DateTime dataVencimento = dataInicial.AddMonths(i);
                        decimal valorAtual = (i == 1) ? valorDaPrimeira : valorParcelasIguais;

                        vendaToUpdate.Parcelas.Add(new Parcela
                        {
                            NumeroParcela = i,
                            ValorParcela = Math.Round(valorAtual, 2),
                            DataVencimento = dataVencimento,
                            Status = "Aberta"
                        });
                    }
                }

                // 4. Salva todas as alterações
                await _context.SaveChangesAsync();
                return RedirectToPage("./DevedoresIndex");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Erro Crítico: " + ex.Message;
                return Page();
            }
        }
    }
}