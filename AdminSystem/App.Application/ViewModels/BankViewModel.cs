using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSystem.Application.ViewModels
{
    public record BankViewModel
    {
        public int Id { get; set; }
        [ForeignKey("客戶資料")]
        public int 客戶Id { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 銀行名稱 { get; set; }
        [Required(ErrorMessage = "必填")]
        [Range(1, 999, ErrorMessage = "銀行代碼必須介於 1 到 999 之間")]
        [DisplayFormat(ConvertEmptyStringToNull = true)]
        public int? 銀行代碼 { get; set; }
        [Range(1, 999, ErrorMessage = "分行代碼必須介於 1 到 999 之間")]
        public int? 分行代碼 { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 帳戶名稱 { get; set; }
        [Required(ErrorMessage = "必填")]
        [RegularExpression(@"^\d{6,20}$", ErrorMessage = "帳戶號碼必須為 6-20 位數字")]
        public string 帳戶號碼 { get; set; }
        public bool 是否已刪除 { get; set; }
    }
}