using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Domain.Entities;

public partial class 客戶聯絡人
{
    [Key]
    public int Id { get; set; }

    public int 客戶Id { get; set; }

    [StringLength(50)]
    public string 職稱 { get; set; } = null!;

    [StringLength(50)]
    [Required]
    public string 姓名 { get; set; } = null!;

    [StringLength(250)]
    [Required]
    public string Email { get; set; } = null!;

    [StringLength(50)]
    public string? 手機 { get; set; }

    [StringLength(50)]
    public string? 電話 { get; set; }

    public bool 是否已刪除 { get; set; }

    [ForeignKey(nameof(客戶Id))]
    [InverseProperty(nameof(客戶資料.客戶聯絡人s))]
    public virtual 客戶資料 客戶 { get; set; } = null!;
}