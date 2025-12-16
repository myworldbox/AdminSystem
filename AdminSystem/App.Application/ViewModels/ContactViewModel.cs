using AdminSystem.App.Application.Common.Validators;
using AdminSystem.Domain.Entities;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminSystem.Application.ViewModels
{
    public class ContactViewModel
    {
        public int Id { get; set; }
        [ForeignKey("客戶資料")]
        [Ignore]
        public int 客戶Id { get; set; }
        public new string 客戶名稱 => 客戶.客戶名稱;
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 職稱 { get; set; }
        [Required(ErrorMessage = "必填")]
        [StringLength(50)]
        public string 姓名 { get; set; }
        [Required(ErrorMessage = "必填")]
        [EmailAddress(ErrorMessage = "無效格式")]
        [UniqueEmail]
        public string Email { get; set; }
        [PhoneFormat]
        public string? 手機 { get; set; }
        [PhoneFormat]
        public string? 電話 { get; set; }
        [Ignore]
        public bool 是否已刪除 { get; set; }
        [Ignore]
        public virtual 客戶資料 客戶 { get; set; }
        [Ignore]
        public ContactDropdown? dropdown { get; set; }
    }

    public record ContactDropdown
    {

        public SelectList? CategoryList { get; set; }
        public SelectList? 客戶IdList { get; set; }
    }
}