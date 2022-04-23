using System.ComponentModel.DataAnnotations;

namespace WebApplicationTechSale.Models
{
    public class MakeTransactionViewModel
    {
        public string CtAccountId { get; set; }
        public string DtAccountId { get; set; }
        [Display(Name = "Сумма")]
        [Required(ErrorMessage = "Укажите сумму")]
        [Range(100, 1000000, ErrorMessage = "Минимальная сумма 100 руб.")]
        public decimal Amount { get; set; }
    }
}
