using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroApp.Models
{

    public class Parcela
    {
        public int Id { get; set; }

        public int VendaId { get; set; }
        public Venda? Venda { get; set; } 

        [Required]
        public int NumeroParcela { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal ValorParcela { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataVencimento { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataPagamento { get; set; }

        public string Status { get; set; } = "Aberta";
    }
}