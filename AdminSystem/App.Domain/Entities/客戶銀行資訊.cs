using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Domain.Entities;

[Table("CUSTOMER_BANK_INFOS")]
public partial class 客戶銀行資訊
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CUSTOMER_ID")]
    public int 客戶Id { get; set; }

    [Column("BANK_NAME")]
    [StringLength(50)]
    [Required]
    public string 銀行名稱 { get; set; } = null!;

    [Column("BANK_CODE")]
    public int 銀行代碼 { get; set; }

    [Column("BRANCH_CODE")]
    public int? 分行代碼 { get; set; }

    [Column("ACCOUNT_NAME")]
    [StringLength(50)]
    [Required]
    public string 帳戶名稱 { get; set; } = null!;

    [Column("ACCOUNT_NUMBER")]
    [StringLength(20)]
    [Required]
    public string 帳戶號碼 { get; set; } = null!;

    [Column("IS_DELETED")]
    public bool 是否已刪除 { get; set; }

    [ForeignKey(nameof(客戶Id))]
    [InverseProperty(nameof(客戶資料.客戶銀行資訊s))]
    public virtual 客戶資料 客戶 { get; set; } = null!;
}