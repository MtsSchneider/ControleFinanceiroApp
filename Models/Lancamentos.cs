using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroApp.Models
{
    // Lançamento representa uma entrada ou saída de dinheiro
    public class Lancamento
    {
        public int Id { get; set; }

        // Chave estrangeira para vincular o lançamento a um usuário
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; } // Propriedade de navegação

        [Required]
        [StringLength(200)]
        public string? Descricao { get; set; }

        [Required]
        [DataType(DataType.Currency)] // Indica que é um valor monetário
        public decimal Valor { get; set; }

        // Tipo: "Receita" ou "Despesa"
        [Required]
        public string? Tipo { get; set; } 

        // Data em que o dinheiro entrou/saiu ou é esperado
        [Required]
        [DataType(DataType.Date)]
        public DateTime DataVencimento { get; set; }
        
        // Status: "Pendente" ou "Pago"
        [Required]
        public string Status { get; set; } = "Pendente"; 
    }
}