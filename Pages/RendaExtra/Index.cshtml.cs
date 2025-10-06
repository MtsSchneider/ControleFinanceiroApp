using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ControleFinanceiroApp.Data;
using ControleFinanceiroApp.Models;
using System.Security.Claims;

[Authorize]
public class RendaExtraModel : PageModel
{
    private readonly AppDbContext _context;
    private int _userId;
    
    // CLASSE AUXILIAR (VIEW MODEL) PARA O RESUMO DE DEVEDORES
    public class DevedorResumoViewModel
    {
        public int Id { get; set; } // VendaId
        public string? NomeComprador { get; set; }
        public decimal SaldoDevedor { get; set; }
        public string? StatusVenda { get; set; }
        public DateTime? ProximoVencimento { get; set; } // NOVO CAMPO
    }

    public string? TipoRendaExtra { get; set; }

    // PROPRIEDADES DE VENDAS
    public IList<Produto> Produtos { get; set; } = new List<Produto>();
    public IList<DevedorResumoViewModel> DevedoresResumo { get; set; } = new List<DevedorResumoViewModel>(); // Tipo alterado
    public decimal TotalDevido { get; set; } = 0;
    public Parcela? ProximaParcelaVencimento { get; set; }
    
    // DADOS PARA ESTOQUE
    public int TotalProdutosEmEstoque { get; set; } = 0;
    public decimal ValorTotalEstoque { get; set; } = 0;

    // PROPRIEDADES DE UBER
    public IList<GanhoDiarioUber> GanhosUber { get; set; } = new List<GanhoDiarioUber>();
    public decimal TotalGanhosUberMes { get; set; } = 0;

    public RendaExtraModel(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userIdString))
        {
            return RedirectToPage("/Account/Login");
        }
        
        _userId = int.Parse(userIdString);
        
        TipoRendaExtra = User.FindFirstValue("RendaExtra"); 
        
        if (TipoRendaExtra == "Vendas")
        {
            // 1. CARREGA PRODUTOS (RESUMO DE ESTOQUE)
            Produtos = await _context.Produtos
                .Where(p => p.UsuarioId == _userId)
                .OrderBy(p => p.Nome)
                .Take(5)
                .ToListAsync();
            
            // LÓGICA DE CÁLCULO DO ESTOQUE
            TotalProdutosEmEstoque = await _context.Produtos
                .Where(p => p.UsuarioId == _userId)
                .SumAsync(p => p.QuantidadeEstoque);
            
            ValorTotalEstoque = await _context.Produtos
                .Where(p => p.UsuarioId == _userId)
                .SumAsync(p => p.QuantidadeEstoque * p.PrecoVenda);
            
            // 2. CARREGA DEVEDORES (RESUMO)
            TotalDevido = await _context.Vendas
                .Where(v => v.UsuarioId == _userId && v.StatusVenda != "Pago")
                .SumAsync(v => v.SaldoDevedor);

            var vendasAbertas = await _context.Vendas
                .Where(v => v.UsuarioId == _userId && v.StatusVenda != "Pago")
                .Take(5)
                .ToListAsync();

            var devedores = new List<DevedorResumoViewModel>();

            // ITERA SOBRE AS VENDAS PARA ENCONTRAR O PRÓXIMO VENCIMENTO DE CADA UMA
            foreach (var venda in vendasAbertas)
            {
                // Busca a parcela mais próxima e aberta (Aberto)
                var proximaParcela = await _context.Parcelas
                    .Where(p => p.VendaId == venda.Id && p.Status == "Aberta")
                    .OrderBy(p => p.DataVencimento)
                    .FirstOrDefaultAsync();

                devedores.Add(new DevedorResumoViewModel
                {
                    Id = venda.Id,
                    NomeComprador = venda.NomeComprador,
                    SaldoDevedor = venda.SaldoDevedor,
                    StatusVenda = venda.StatusVenda,
                    ProximoVencimento = proximaParcela?.DataVencimento // Adiciona o vencimento aqui
                });
            }

            DevedoresResumo = devedores.AsEnumerable().OrderByDescending(v => v.SaldoDevedor).ToList();

            // 3. PRÓXIMO VENCIMENTO GERAL (permanece igual)
            ProximaParcelaVencimento = await _context.Parcelas
                .Include(p => p.Venda)
                .Where(p => p.Venda!.UsuarioId == _userId && p.Status == "Aberta")
                .OrderBy(p => p.DataVencimento)
                .FirstOrDefaultAsync();
        }
        else if (TipoRendaExtra == "Uber")
        {
            // Lógica de Uber (mantida)
            var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            
            GanhosUber = await _context.GanhosUber
                .Where(g => g.UsuarioId == _userId && g.Data >= inicioMes)
                .OrderByDescending(g => g.Data)
                .ToListAsync();

            TotalGanhosUberMes = GanhosUber.Sum(g => g.ValorGanho);
        }
        
        return Page();
    }
}