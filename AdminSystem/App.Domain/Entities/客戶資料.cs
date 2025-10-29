using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Domain.Entities;

public partial class 客戶資料
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    [Required]
    public string 客戶名稱 { get; set; } = null!;

    [StringLength(8)]
    [Unicode(false)]
    [Required]
    public string 統一編號 { get; set; } = null!;

    [StringLength(50)]
    [Required]
    public string 電話 { get; set; } = null!;

    [StringLength(50)]
    public string? 傳真 { get; set; }

    [StringLength(100)]
    public string? 地址 { get; set; }

    [StringLength(250)]
    public string? Email { get; set; }

    public bool 是否已刪除 { get; set; }

    [StringLength(50)]
    public string? 客戶分類 { get; set; }

    public virtual ICollection<客戶聯絡人> 客戶聯絡人s { get; set; } = new List<客戶聯絡人>();

    [InverseProperty("客戶")]
    public virtual ICollection<客戶銀行資訊> 客戶銀行資訊s { get; set; } = new List<客戶銀行資訊>();
}