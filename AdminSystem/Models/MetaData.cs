using AdminSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace AdminSystem.Models
{
    [MetadataType(typeof(CustomerMetadata))]
    public partial class 客戶資料 { }

    public class CustomerMetadata
    {
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
        public string 傳真 { get; set; }

        [StringLength(100)]
        public string 地址 { get; set; }

        [EmailAddress(ErrorMessage = "無效格式")]
        public string Email { get; set; }

        [Required(ErrorMessage = "必填")]
        public string 客戶分類 { get; set; }
    }

    [MetadataType(typeof(ContactMetadata))]
    public partial class 客戶聯絡人 { }

    public class ContactMetadata
    {
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 職稱 { get; set; }

        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 姓名 { get; set; }

        [Required(ErrorMessage = "必填")]
        [EmailAddress(ErrorMessage = "無效格式")]
        public string Email { get; set; }

        [PhoneFormat]
        public string 手機 { get; set; }

        [StringLength(50)]
        public string 電話 { get; set; }
    }

    [MetadataType(typeof(BankMetadata))]
    public partial class 客戶銀行資訊 { }

    public class BankMetadata
    {
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 銀行名稱 { get; set; }

        [Required(ErrorMessage = "必填")]
        [StringLength(3, MinimumLength = 3)]
        public string 銀行代碼 { get; set; }

        [Required(ErrorMessage = "必填")]
        [StringLength(4)]
        public string 分行代碼 { get; set; }

        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 帳戶名稱 { get; set; }

        [Required(ErrorMessage = "必填")]
        [StringLength(20)]
        public string 帳戶號碼 { get; set; }
    }
}