using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroApp.Models
{
    public class Lancamento
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required]
        [StringLength(200)]
        public string? Descricao { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Valor { get; set; }

       
        [Required]
        public string? Tipo { get; set; } 

        [Required]
        [DataType(DataType.Date)]
        public DateTime DataVencimento { get; set; }
        
        [Required]
        public string Status { get; set; } = "Pendente"; 
    }
}