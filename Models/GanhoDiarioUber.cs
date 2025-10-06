using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroApp.Models
{
    // Armazena o ganho diário do motorista
    public class GanhoDiarioUber
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; } // Vínculo com o usuário

        [Required]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        // Valor Bruto Ganho no dia
        public decimal ValorGanho { get; set; }
    }
}