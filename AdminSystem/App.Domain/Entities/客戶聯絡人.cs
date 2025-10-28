using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Domain.Entities;

[Table("CUSTOMER_CONTACTS")]
[Index(nameof(客戶Id), Name = "IX_CONTACTS_CUSTOMERID")]
public partial class 客戶聯絡人
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("CUSTOMER_ID")]
    public int 客戶Id { get; set; }

    [Column("TITLE")]
    [StringLength(50)]
    public string 職稱 { get; set; } = null!;

    [Column("NAME")]
    [StringLength(50)]
    [Required]
    public string 姓名 { get; set; } = null!;

    [Column("EMAIL")]
    [StringLength(250)]
    [Required]
    public string Email { get; set; } = null!;

    [Column("MOBILE")]
    [StringLength(50)]
    public string? 手機 { get; set; }

    [Column("PHONE")]
    [StringLength(50)]
    public string? 電話 { get; set; }

    [Column("IS_DELETED")]
    public bool 是否已刪除 { get; set; }

    [ForeignKey(nameof(客戶Id))]
    [InverseProperty(nameof(客戶資料.客戶聯絡人s))]
    public virtual 客戶資料 客戶 { get; set; } = null!;
}