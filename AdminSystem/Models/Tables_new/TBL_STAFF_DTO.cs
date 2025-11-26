using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminSystem.Models.Tables_new
{
    public partial class TBL_STAFF_DTO
    {
        // ──────────────────────────────────────────────────────────────────────
        // Core Identity
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

        [DisplayName("Chinese Name")]
        public string? STF_CNAME { get; set; }

        // ──────────────────────────────────────────────────────────────────────
        // Personal Info
        // ──────────────────────────────────────────────────────────────────────
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

        // ──────────────────────────────────────────────────────────────────────
        // Address
        // ──────────────────────────────────────────────────────────────────────
        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("First Line of Address")]
        [NoChinese]
        public string STF_ADDR1 { get; set; } = null!;

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Address Area")]
        public string STF_ADDR_AREA { get; set; } = null!;

        [NoChinese] public string? STF_ADDR2 { get; set; }
        [NoChinese] public string? STF_ADDR3 { get; set; }
        [NoChinese] public string? STF_ADDR4 { get; set; }

        // ──────────────────────────────────────────────────────────────────────
        // Identity Documents
        // ──────────────────────────────────────────────────────────────────────
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

        // ──────────────────────────────────────────────────────────────────────
        // Bank Account
        // ──────────────────────────────────────────────────────────────────────
        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Bank Code")]
        [BankCodeExists]
        public string? STF_AC_BNK_CODE { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Account Code")]
        [AccountCodeLength]
        public string? STF_AC_CODE { get; set; }

        // ──────────────────────────────────────────────────────────────────────
        // Contact
        // ──────────────────────────────────────────────────────────────────────
        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Mobile Country Code")]
        public string? STF_PHONE1AREACODE { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        [DisplayName("Mobile Number")]
        [HkMobileValid]
        public string? STF_PHONE1 { get; set; }

        [DisplayName("Phone 2")]
        public string? STF_PHONE2 { get; set; }

        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? STF_EMAIL { get; set; }

        // ──────────────────────────────────────────────────────────────────────
        // Spouse
        // ──────────────────────────────────────────────────────────────────────
        [DisplayName("Spouse Name")]
        [NoChinese]
        [RequiredWhenMaritalStatusIsM(ErrorMessage = Msg.SpouseRequired)]
        public string? STF_SPS_NAME { get; set; }

        public string? STF_SPS_HKID { get; set; }
        public string? STF_SPS_PP_NO { get; set; }
        public string? STF_SPS_PP_ISCNTY { get; set; }
        public string? STF_SPS_HEALTH { get; set; }

        // ──────────────────────────────────────────────────────────────────────
        // Parents
        // ──────────────────────────────────────────────────────────────────────
        [NoChinese] public string? STF_DAD_NAME { get; set; }
        public string? STF_DAD_HKID { get; set; }
        public string? STF_DAD_PP_NO { get; set; }
        public string? STF_DAD_PP_ISCNTY { get; set; }
        public string? STF_DAD_HEALTH { get; set; }

        [NoChinese] public string? STF_MOM_NAME { get; set; }
        public string? STF_MOM_HKID { get; set; }
        public string? STF_MOM_PP_NO { get; set; }
        public string? STF_MOM_PP_ISCNTY { get; set; }
        public string? STF_MOM_HEALTH { get; set; }

        // ──────────────────────────────────────────────────────────────────────
        // Work Permit / Visa
        // ──────────────────────────────────────────────────────────────────────
        public DateTime? STF_EMPVISA_XDATE { get; set; }

        [DisplayName("Permit Number")]
        [RequiredIfOtherHasValue(nameof(STF_PERMIT_XDATE), ErrorMessage = "Permit Number and Expiry Date must be both filled or both empty.")]
        public string? STF_PERMITNO { get; set; }

        [DisplayName("Permit Expiry Date")]
        [RequiredIfOtherHasValue(nameof(STF_PERMITNO), ErrorMessage = "Permit Number and Expiry Date must be both filled or both empty.")]
        public DateTime? STF_PERMIT_XDATE { get; set; }

        // ──────────────────────────────────────────────────────────────────────
        // Audit Fields
        // ──────────────────────────────────────────────────────────────────────
        [Required(ErrorMessage = Msg.Required)]
        public string STF_ACTN { get; set; } = null!;

        public DateTime STF_ACTNDATE { get; set; }

        [Required(ErrorMessage = Msg.Required)]
        public string STF_ACTNUSER { get; set; } = null!;

        public string? STF_ACTNUSER_SSO { get; set; }

        public DateTime TIMESTAMP { get; set; }

        // ──────────────────────────────────────────────────────────────────────
        // UI Helpers
        // ──────────────────────────────────────────────────────────────────────
        public string OperationMode { get; set; } = "insert";

        public string CombinedName => $"{STF_SURNAME} {STF_GIVENNAME}".Trim();
    }
}