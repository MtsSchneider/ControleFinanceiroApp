using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroApp.Models
{
    
    public class GanhoDiarioUber
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; } 

        [Required]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal ValorGanho { get; set; }
    }
}