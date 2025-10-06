using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiroApp.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; } 

        [Required]
        [StringLength(100)]
        public string? Nome { get; set; }
		
		[Required(ErrorMessage = "O código é obrigatório.")]
        [StringLength(50)]
        public string? CodigoProduto { get; set; }

        [Required]
        public int QuantidadeEstoque { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal PrecoVenda { get; set; }
    }
}