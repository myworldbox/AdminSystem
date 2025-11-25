using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminSystem.Models.Tables_new
{
    public partial class TBL_STAFF_DTO
    {
        // ──────────────────────────────────────────────────────────────────────
        // Properties — MAXIMUM use of DataAnnotations
        // ──────────────────────────────────────────────────────────────────────
        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Staff No.")]
        [Unique(nameof(STF_NO))]
        public string STF_NO { get; set; } = null!;

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Surname")]
        public string STF_SURNAME { get; set; } = null!;

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Staff Name in English")]
        [NoChinese]
        public string STF_NAME { get; set; } = null!;

        [DisplayName("Given Name")]
        [NoChinese]
        public string? STF_GIVENNAME { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Date of Birth")]
        [DateNotInFuture(ErrorMessage = Msg.FutureDob)]
        [MinimumAge(15, ErrorMessage = Msg.Under15)]
        public DateTime STF_DOB { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Staff Sex")]
        public string STF_SEX { get; set; } = null!;

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Marital Status")]
        public string STF_MARITAL_STAT { get; set; } = null!;

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("First Line of Address")]
        [NoChinese]
        public string STF_ADDR1 { get; set; } = null!;

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Address Area")]
        public string STF_ADDR_AREA { get; set; } = null!;

        [DisplayName("HKID No.")]
        [HkIdFullValidation]
        public string? STF_HKID { get; set; }

        [DisplayName("Passport No.")]
        public string? STF_PP_NO { get; set; }

        [DisplayName("Passport Issue Country")]
        [NoChinese]
        [RequiredIfOtherHasValue(nameof(STF_PP_NO), ErrorMessage = "Passport Issue Country is required when Passport No. is provided.")]
        public string? STF_PP_ISCNTY { get; set; }

        [DisplayName("Nationality")]
        [NoChinese]
        public string? STF_NAT { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Bank Code")]
        [BankCodeExists]
        public string? STF_AC_BNK_CODE { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Account Code")]
        [AccountCodeLength]
        public string? STF_AC_CODE { get; set; }

        [DisplayName("Spouse Name")]
        [NoChinese]
        [RequiredWhenMaritalStatusIsM(ErrorMessage = Msg.SpouseRequired)]
        public string? STF_SPS_NAME { get; set; }

        [DisplayName("Permit Number")]
        [RequiredIfOtherHasValue(nameof(STF_PERMIT_XDATE), ErrorMessage = "Permit Number and Expiry Date must be both filled or both empty.")]
        public string? STF_PERMITNO { get; set; }

        [DisplayName("Permit Expiry Date")]
        [RequiredIfOtherHasValue(nameof(STF_PERMITNO), ErrorMessage = "Permit Number and Expiry Date must be both filled or both empty.")]
        public DateTime? STF_PERMIT_XDATE { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Mobile Country Code")]
        public string? STF_PHONE1AREACODE { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Mobile Number")]
        [HkMobileValid]
        public string? STF_PHONE1 { get; set; }

        [NoChinese] public string? STF_ADDR2 { get; set; }
        [NoChinese] public string? STF_ADDR3 { get; set; }
        [NoChinese] public string? STF_ADDR4 { get; set; }

        public string OperationMode { get; set; } = "insert";

        public string CombinedName => $"{STF_SURNAME} {STF_GIVENNAME}".Trim();
    }
}