using AdminSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSystem.Models
{
    public partial class 客戶聯絡人
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
        public string Email { get; set; }
        [PhoneFormat]
        public string? 手機 { get; set; }
        [StringLength(50)]
        public string? 電話 { get; set; }
        public bool 是否已刪除 { get; set; }

        public virtual 客戶資料 客戶資料 { get; set; }
    }
}