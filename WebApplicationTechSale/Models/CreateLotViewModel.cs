using DataAccessLogic.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;
using WebApplicationTechSale.HelperServices;

namespace WebApplicationTechSale.Models
{
    public class CreateLotViewModel
    {
        [Display(Name = "Название лота")]
        [Required(ErrorMessage = "Укажите название лота")]
        [MaxLength(100, ErrorMessage = "Не более 100 символов")]
        public string Name { get; set; }

        [Display(Name = "Фотография")]
        [Required(ErrorMessage = "Загрузите фотографию товара")]
        [DataType(DataType.Upload)]
        [ExtensionValidation(new string[] 
        { ".jpg", ".jpeg", ".pjpg", ".pjpeg", ".png" }, ErrorMessage = "Неверный формат файла")]
        public IFormFile Photo { get; set; }

        [Display(Name = "Описание лота")]
        [Required(ErrorMessage = "Добавьте описание лота")]
        [MaxLength(500, ErrorMessage = "Не более 500 символов")]
        public string Description { get; set; }

        [Display(Name = "Тип аукциона")]
        [Required(ErrorMessage = "Укажите тип аукциона")]
        public TypeOfAuction TypeOfAuction { get; set; }

        [Display(Name = "Дата начала срока подачи заявок")]
        [DataType(DataType.Date)]
        public DateTime? AppStartDate { get; set; }

        [Display(Name = "Дата окончания срока подачи заявок")]
        [DataType(DataType.Date)]
        public DateTime? AppEndDate { get; set; }

        [Display(Name = "Начальная цена")]
        [Required(ErrorMessage = "Укажите начальную цену")]
        [Range(0, 1000000, ErrorMessage = "Цена не должна быть меньше нуля")]
        public int? StartPrice { get; set; }

        [Display(Name = "Шаг ставки")]
        [Range(0, 1000000, ErrorMessage = "Шаг ставки не должен быть меньше нуля")]
        [Required(ErrorMessage = "Укажите шаг ставки")]
        public int? BidStep { get; set; }

        [Display(Name = "Конечная цена")]
        [Range(100, 1000000, ErrorMessage = "Укажите цену, за которую готовы отдать товар без торгов")]
        [Required(ErrorMessage = "Цена не должна быть меньше стартовой")]
        public int? FinalPrice { get; set; }

        [Display(Name = "Размер обеспечения ставки (в процентах)")]
        [Range(1, 50, ErrorMessage = "Размер обеспечения ставки должен находиться в диапазоне от 1 до 50")]
        public int? PercentBid { get; set; }

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
