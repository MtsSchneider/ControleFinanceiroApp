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

        // PROPRIEDADES DE INPUT (SEM O INPUTMODEL)
        [BindProperty]
        [Required(ErrorMessage = "O nome do comprador é obrigatório.")]
        [Display(Name = "Nome do Comprador")]
        public string? NomeComprador { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "O valor total é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Valor Total")]
        public decimal ValorTotal { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "O número de parcelas é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Deve ter pelo menos 1 parcela.")]
        [Display(Name = "Número de Parcelas")]
        public int NumeroParcelas { get; set; }

        [BindProperty]
        [Display(Name = "Valor da 1ª Parcela")]
        [Range(0, double.MaxValue, ErrorMessage = "O valor deve ser positivo ou zero.")]
        public decimal ValorPrimeiraParcela { get; set; } = 0;

        // CAMPOS OCULTOS PARA BINDING DIRETO DO POST (RESOLVE O PROBLEMA DE DATA)
        [BindProperty]
        public int VendaId { get; set; } // NOVO: ID da Venda

        [BindProperty]
        public DateTime DataVendaOriginal { get; set; } // NOVO: Data para recálculo

        // MANTIDO: VendaOriginal para exibição e validação
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

            // POPULAÇÃO DOS CAMPOS DE INPUT NA REQUISIÇÃO GET
            VendaOriginal = venda;
            NomeComprador = venda.NomeComprador;
            ValorTotal = venda.ValorTotal;
            NumeroParcelas = venda.NumeroParcelas;
            ValorPrimeiraParcela = venda.Parcelas?.FirstOrDefault(p => p.NumeroParcela == 1)?.ValorParcela ?? 0;

            // POPULAÇÃO DOS CAMPOS OCULTOS
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

            // 1. Re-busca a venda completa (usando VendaId)
            var vendaToUpdate = await _context.Vendas
                .Include(v => v.Parcelas)
                .FirstOrDefaultAsync(v => v.Id == VendaId && v.UsuarioId == _userId); // Usa VendaId

            if (vendaToUpdate == null) return NotFound();

            // Re-calcula flags para fallback (necessário para o Page() em caso de erro)
            PodeEditarValores = !(vendaToUpdate.Parcelas ?? new List<Parcela>()).Any(p => p.Status == "Paga");
            VendaOriginal = vendaToUpdate;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (ValorPrimeiraParcela > ValorTotal)
                {
                    ModelState.AddModelError(nameof(ValorPrimeiraParcela), "O valor da primeira parcela não pode ser maior que o Valor Total.");
                    return Page();
                }

                // 2. Aplica as alterações no Nome
                vendaToUpdate.NomeComprador = NomeComprador;

                // 3. Recálculo (A DataVendaOriginal já está vinculada no POST)
                if (PodeEditarValores)
                {
                    if (vendaToUpdate.ValorTotal != ValorTotal || vendaToUpdate.NumeroParcelas != NumeroParcelas)
                    {
                        // Remove e atualiza os valores
                        _context.Parcelas.RemoveRange(vendaToUpdate.Parcelas!);
                        vendaToUpdate.ValorTotal = ValorTotal;
                        vendaToUpdate.NumeroParcelas = NumeroParcelas;
                        vendaToUpdate.SaldoDevedor = ValorTotal;

                        // Lógica de Recriação
                        decimal valorDaPrimeira = ValorPrimeiraParcela;

                        if (valorDaPrimeira == 0 && NumeroParcelas > 0)
                        {
                            valorDaPrimeira = ValorTotal / NumeroParcelas;
                        }

                        decimal valorRestante = ValorTotal - valorDaPrimeira;
                        int parcelasRestantes = NumeroParcelas - 1;

                        decimal valorParcelasIguais = 0;

                        if (parcelasRestantes > 0)
                        {
                            valorParcelasIguais = valorRestante / parcelasRestantes;
                        }
                        else if (NumeroParcelas == 1)
                        {
                            valorDaPrimeira = ValorTotal;
                        }

                        DateTime dataInicial = DataVendaOriginal; // Usa a data vinculada do campo oculto

                        for (int i = 1; i <= NumeroParcelas; i++)
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
                }

                // 4. Salva todas as alterações
                await _context.SaveChangesAsync();

                return RedirectToPage("./DevedoresIndex");
            }
            catch (Exception ex)
            {
                // Captura e exibe o erro real (o erro que estava quebrando o ModelState)
                ViewData["ErrorMessage"] = "Erro Crítico: " + ex.Message;
                return Page();
            }
        }
    }
}