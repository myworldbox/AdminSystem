using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Domain.Entities;

[Keyless]
public partial class VwCustomerSummary
{
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(50)]
    public string 客戶名稱 { get; set; } = null!;

    public int? 聯絡人數量 { get; set; }

    public int? 銀行帳戶數量 { get; set; }
}
