using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

[Authorize]
public class CriarModel : PageModel
{
    private readonly AppDbContext _context;

    public CriarModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public LancamentoInputModel Input { get; set; } = new LancamentoInputModel();
    
    public class LancamentoInputModel
    {
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(200)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, 10000000.00, ErrorMessage = "O valor deve ser positivo.")]
        [DataType(DataType.Currency)]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "Selecione o tipo (Receita/Despesa).")]
        public string? Tipo { get; set; }

        [Required(ErrorMessage = "A data de vencimento/recebimento é obrigatória.")]
        [DataType(DataType.Date)]
        [Display(Name = "Data Vencimento/Recebimento")]
        public DateTime DataVencimento { get; set; }
        
        [Required(ErrorMessage = "Selecione o status.")]
        public string Status { get; set; } = "Pendente";
    }

    public void OnGet()
    {
        Input.DataVencimento = DateTime.Today;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToPage("/Account/Login");
        }
        int userId = int.Parse(userIdString);

        var novoLancamento = new Lancamento
        {
            UsuarioId = userId,
            Descricao = Input.Descricao,
            Valor = Input.Valor,
            Tipo = Input.Tipo,
            DataVencimento = Input.DataVencimento,
            Status = Input.Status
        };

        _context.Lancamentos.Add(novoLancamento);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Index");
    }
}