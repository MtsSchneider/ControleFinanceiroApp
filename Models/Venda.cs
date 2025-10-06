using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ControleFinanceiroApp.Models
{
    // Registra uma venda a prazo
    public class Venda
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; } // Vincula ao usuário

        [Required]
        [StringLength(100)]
        [Display(Name = "Nome do Comprador")]
        public string? NomeComprador { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataVenda { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal ValorTotal { get; set; }

        [Required]
        public int NumeroParcelas { get; set; }

        public decimal SaldoDevedor { get; set; } // O quanto falta pagar
        public string StatusVenda { get; set; } = "Pendente";
        
        // Propriedade de navegação para as parcelas
        public ICollection<Parcela>? Parcelas { get; set; }
    }
}