using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ControleFinanceiroApp.Models
{
    public class Venda
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }

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

        public decimal SaldoDevedor { get; set; }
        public string StatusVenda { get; set; } = "Pendente";
        
        public ICollection<Parcela>? Parcelas { get; set; }
    }
}