using AdminSystem.Application.Validators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSystem.Application.ViewModels
{
    public record ContactViewModel
    {
        public int Id { get; set; }
        [ForeignKey("客戶資料")]
        public int 客戶Id { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 職稱 { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 姓名 { get; set; }
        [Required(ErrorMessage = "必填")]
        [EmailAddress(ErrorMessage = "無效格式")]
        [UniqueEmail]
        public string Email { get; set; }
        [PhoneFormat]
        public string? 手機 { get; set; }
        [PhoneFormat]
        public string? 電話 { get; set; }
        public bool 是否已刪除 { get; set; }
    }
}