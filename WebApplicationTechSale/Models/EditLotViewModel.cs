using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationTechSale.HelperServices;

namespace WebApplicationTechSale.Models
{
    public class EditLotViewModel
    {
        [Required]
        public string Id { get; set; }

        [Display(Name = "Название лота")]
        [Required(ErrorMessage = "Укажите название лота")]
        [MaxLength(100, ErrorMessage = "Не более 100 символов")]
        public string Name { get; set; }

        [Required]
        public string OldName { get; set; }

        [Display(Name = "Фотография")]
        [Required(ErrorMessage = "Загрузите фотографию товара")]
        [DataType(DataType.Upload)]
        [ExtensionValidation(new string[]
        { ".jpg", ".jpeg", ".pjpg", ".pjpeg", ".png" }, ErrorMessage = "Неверный формат файла")]
        public IFormFile Photo { get; set; }

        [Required]
        public string OldPhotoSrc { get; set; }

        [Display(Name = "Описание лота")]
        [Required(ErrorMessage = "Добавьте описание лота")]
        [MaxLength(500, ErrorMessage = "Не более 500 символов")]
        public string Description { get; set; }

        [Display(Name = "Начальная цена")]
        [Required(ErrorMessage = "Укажите начальную цену")]
        [Range(0, 1000000, ErrorMessage = "Цена не должна быть меньше нуля")]
        public int? StartPrice { get; set; }

        [Display(Name = "Шаг ставки")]
        [Range(0, 1000000, ErrorMessage = "Шаг ставки не должен быть меньше нуля")]
        [Required(ErrorMessage = "Укажите шаг ставки")]
        public int? BidStep { get; set; }

        [Display(Name = "Дата начала торгов")]
        [Required(ErrorMessage = "Укажите дату начала торгов")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Дата окончания торгов")]
        [Required(ErrorMessage = "Укажите дату окончания торгов")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
    }
}
