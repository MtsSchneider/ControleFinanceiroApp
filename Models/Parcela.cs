using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroApp.Models
{
    // Rastreia cada parcela de uma Venda
    public class Parcela
    {
        public int Id { get; set; }

        // Chave Estrangeira: vincula a parcela à Venda
        public int VendaId { get; set; }
        public Venda? Venda { get; set; } // Propriedade de navegação

        [Required]
        public int NumeroParcela { get; set; } // 1, 2, 3, etc.

        [Required]
        [DataType(DataType.Currency)]
        public decimal ValorParcela { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataVencimento { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataPagamento { get; set; } // Null se não foi paga

        public string Status { get; set; } = "Aberta"; // Aberta, Paga, Atrasada
    }
}