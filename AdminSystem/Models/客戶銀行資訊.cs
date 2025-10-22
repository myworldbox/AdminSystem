using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSystem.Models
{
    public partial class 客戶銀行資訊
    {
        public int Id { get; set; }
        [ForeignKey("客戶資料")]
        public int 客戶Id { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 銀行名稱 { get; set; }
        [Required(ErrorMessage = "必填")]
        public int 銀行代碼 { get; set; }
        public int? 分行代碼 { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 帳戶名稱 { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(20)]
        public string 帳戶號碼 { get; set; }
        public bool 是否已刪除 { get; set; }
        public virtual 客戶資料? 客戶資料 { get; set; }
    }
}