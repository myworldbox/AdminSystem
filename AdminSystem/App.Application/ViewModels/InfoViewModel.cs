using AdminSystem.Application.Validators;
using AdminSystem.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AdminSystem.Application.ViewModels
{
    public record InfoViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50, ErrorMessage = "最多50字")]
        public string 客戶名稱 { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "必須為8碼數字")]
        [RegularExpression(@"\d{8}", ErrorMessage = "必須為8位數字")]
        public string 統一編號 { get; set; }
        [Required(ErrorMessage = "必填")]
        [PhoneFormat]
        public string 電話 { get; set; }
        [StringLength(50)]
        [RegularExpression(@"^\d{6,15}$", ErrorMessage = "傳真必須為6-15位數字")]
        public string? 傳真 { get; set; }
        [StringLength(100)]
        public string? 地址 { get; set; }
        [EmailAddress(ErrorMessage = "無效格式")]
        public string? Email { get; set; }
        public Enums.Category? 客戶分類 { get; set; }
        public bool 是否已刪除 { get; set; }

        public InfoDropdown dropdown { get; set; }
    }

    public record InfoDropdown
    {
        public SelectList? 客戶分類List { get; set; }
        public SelectList? 客戶IdList { get; set; }
    }
}