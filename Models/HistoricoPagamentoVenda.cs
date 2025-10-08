using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControleFinanceiroApp.Models // NAMESPACE CORRETO
{
    public class HistoricoPagamentoVenda
    {
        public int Id { get; set; }

        public int VendaId { get; set; }
        public int UsuarioId { get; set; }

        [Display(Name = "Comprador")]
        public string? NomeComprador { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Valor Pago")]
        public decimal ValorPago { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data Pagamento")]
        public DateTime DataPagamento { get; set; }

        [Display(Name = "Tipo")]
        public string? TipoPagamento { get; set; }

        [ForeignKey("VendaId")]
        public Venda? Venda { get; set; } // Referência à classe Venda
    }
}