using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Domain.Entities;

[Table("客戶銀行資訊")]
public partial class 客戶銀行資訊
{
    [Key]
    public int Id { get; set; }

    [Column("客戶Id")]
    public int 客戶id { get; set; }

    [StringLength(50)]
    public string 銀行名稱 { get; set; } = null!;

    public int 銀行代碼 { get; set; }

    public int? 分行代碼 { get; set; }

    [StringLength(50)]
    public string 帳戶名稱 { get; set; } = null!;

    [StringLength(20)]
    public string 帳戶號碼 { get; set; } = null!;

    public bool 是否已刪除 { get; set; }

    [ForeignKey("客戶id")]
    [InverseProperty("客戶銀行資訊s")]
    public virtual 客戶資料 客戶 { get; set; } = null!;
}
