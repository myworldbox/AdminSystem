using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminSystem.Application.ViewModels
{
    public record InfoViewModel
    {
        public InfoViewModel()
        {
            客戶聯絡人 = new HashSet<ContactViewModel>();
            客戶銀行資訊 = new HashSet<BankViewModel>();
        }

        public int Id { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50, ErrorMessage = "最多50字")]
        public string 客戶名稱 { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "必須為8碼數字")]
        [RegularExpression(@"\d{8}", ErrorMessage = "必須為8位數字")]
        public string 統一編號 { get; set; }
        [StringLength(50)]
        public string 電話 { get; set; }
        [StringLength(50)]
        public string? 傳真 { get; set; }
        [StringLength(100)]
        public string? 地址 { get; set; }
        [EmailAddress(ErrorMessage = "無效格式")]
        public string? Email { get; set; }
        public string? 客戶分類 { get; set; }
        public bool 是否已刪除 { get; set; }

        public virtual ICollection<ContactViewModel> 客戶聯絡人 { get; set; }
        public virtual ICollection<BankViewModel> 客戶銀行資訊 { get; set; }
    }
}