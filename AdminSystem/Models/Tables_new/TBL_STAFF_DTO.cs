using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using AdminSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminSystem.Models.Tables_new
{
    public partial class TBL_STAFF_DTO : IValidatableObject
    {
        // Centralized regex & constants
        internal static class Patterns
        {
            // Covers most common Chinese characters + full-width punctuation
            public static readonly Regex ChineseChars = new(@"[\u4E00-\u9FFF\u3400-\u4DBF\uF900-\uFAFF]", RegexOptions.Compiled);
            public static readonly Regex HkIdBasic = new(@"^[A-Z]{1,2}\d{6}\([0-9A]\)$|^[A-Z]{1,2}\d{6}[0-9A]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            public static readonly Regex NonDigits = new(@"\D", RegexOptions.Compiled);
        }

        internal static class Msg
        {
            public const string Required = "{0} is a compulsory field";
            public const string NoChinese = "Chinese characters are not allowed in {0}.";
            public const string HkMobileInvalid = "HK mobile number should not start with '2' or '3'. Please input a valid mobile number.";
            public const string HkIdInvalid = "[HKID No.] [{0}] is invalid. Please verify.";
            public const string AccountCodeTooLong = "Maximum length of [Account Code] is 12 excluding \"-\"";
            public const string NameTooLong = "The combined length of surname, space, and given name should not be more than 75 characters.";
            public const string MobileRequired = "Mobile and Country Code are mandatory fields.";
            public const string IdOrPassportRequired = "Please input [HK ID No.] or/and [Passport No.]";
            public const string BothOrNone = "{0} and {1} must be both filled or both empty.";
            public const string SpouseRequired = "[Spouse Name] is compulsory if [marital status] is set to \"Married\". Please enter [spouse name] or set [marital status] to \"Married without spouse ID\".";
            public const string Duplicate = "This [{0}] ({1}) is already used in our staff records";
            public const string IdConflict = "{0} Someone already used this no. [{1}] as his/her HKID or Passport No.";
            public const string FutureDob = "[Date of Birth] cannot be a future date";
            public const string Under15 = "This staff is under 15 years old";
            public const string InvalidBank = "[Bank Account Code] ({0}) is an invalid bank code";
            public const string PayrollLockedName = "[Staff Name] cannot be changed Just after Payroll Process";
            public const string PayrollLockedBank = "[Bank Account Code] and [Account Code] cannot be changed Just after Payroll Process";
            public const string PermanentLocked = "There is one or more contracts for this staff in the Human Resources Information System (Permanent).\r\nPlease contact Human Resources Branch to modify the information of this staff if necessary";
            public const string OtherCenterWarning = "Warning: This staff has contract(s) in other centres. Data changes of this staff will also affect them.";
        }

        // Properties
        [Required(ErrorMessage = Msg.Required)][DisplayName("Staff No.")] public string STF_NO { get; set; } = null!;
        [Required(ErrorMessage = Msg.Required)][DisplayName("Surname")] public string STF_SURNAME { get; set; } = null!;
        [Required(ErrorMessage = Msg.Required)][DisplayName("Staff Name in English")][NoChinese] public string STF_NAME { get; set; } = null!;
        [DisplayName("Given Name")][NoChinese] public string? STF_GIVENNAME { get; set; }
        [Required(ErrorMessage = Msg.Required)][DisplayName("Date of Birth")] public DateTime STF_DOB { get; set; }
        [Required(ErrorMessage = Msg.Required)][DisplayName("Staff Sex")] public string STF_SEX { get; set; } = null!;
        [Required(ErrorMessage = Msg.Required)][DisplayName("Marital Status")] public string STF_MARITAL_STAT { get; set; } = null!;
        [Required(ErrorMessage = Msg.Required)][DisplayName("First Line of Address")][NoChinese] public string STF_ADDR1 { get; set; } = null!;
        [Required(ErrorMessage = Msg.Required)][DisplayName("Address Area")] public string STF_ADDR_AREA { get; set; } = null!;
        [DisplayName("HKID No.")][HkIdFullValidation] public string? STF_HKID { get; set; }
        [DisplayName("Passport No.")] public string? STF_PP_NO { get; set; }
        [DisplayName("Passport Issue Country")][NoChinese] public string? STF_PP_ISCNTY { get; set; }
        [DisplayName("Nationality")][NoChinese] public string? STF_NAT { get; set; }
        [Required(ErrorMessage = Msg.Required)][DisplayName("Bank Code")] public string? STF_AC_BNK_CODE { get; set; }
        [Required(ErrorMessage = Msg.Required)][DisplayName("Account Code")][AccountCodeLength] public string? STF_AC_CODE { get; set; }
        [DisplayName("Spouse Name")][NoChinese] public string? STF_SPS_NAME { get; set; }
        [DisplayName("Permit Number")] public string? STF_PERMITNO { get; set; }
        [DisplayName("Permit Expiry Date")] public DateTime? STF_PERMIT_XDATE { get; set; }
        [Required(ErrorMessage = Msg.Required)][DisplayName("Mobile Country Code")] public string? STF_PHONE1AREACODE { get; set; }
        [Required(ErrorMessage = Msg.Required)][DisplayName("Mobile Number")][HkMobileValid] public string? STF_PHONE1 { get; set; }
        [NoChinese] public string? STF_ADDR2 { get; set; }
        [NoChinese] public string? STF_ADDR3 { get; set; }
        [NoChinese] public string? STF_ADDR4 { get; set; }

        public string OperationMode { get; set; } = "insert";

        public string CombinedName => $"{STF_SURNAME} {STF_GIVENNAME}".Trim();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            var db = validationContext.GetService(typeof(DBnew)) as DBnew;
            var userService = validationContext.GetService(typeof(ICurrentUserService)) as ICurrentUserService; // Assume you have this

            // 1. Permanent contract lock
            if (!string.IsNullOrEmpty(STF_NO) && !CanModifyStaff(STF_NO, userService, db))
            {
                results.Add(new ValidationResult(Msg.PermanentLocked));
                return results; // Block everything else
            }

            if (OperationMode != "display")
            {
                if (string.IsNullOrWhiteSpace(STF_PHONE1AREACODE) || string.IsNullOrWhiteSpace(STF_PHONE1))
                    results.Add(new ValidationResult(Msg.MobileRequired, new[] { nameof(STF_PHONE1AREACODE), nameof(STF_PHONE1) }));
            }

            if (CombinedName.Length > 75)
                results.Add(new ValidationResult(Msg.NameTooLong));

            // Identity rules
            if (string.IsNullOrWhiteSpace(STF_HKID) && string.IsNullOrWhiteSpace(STF_PP_NO))
                results.Add(new ValidationResult(Msg.IdOrPassportRequired, new[] { nameof(STF_HKID), nameof(STF_PP_NO) }));

            BothOrNeither(STF_PP_NO, STF_PP_ISCNTY, "Passport No.", "Issue Country", results);
            BothOrNeither(STF_PERMITNO, STF_PERMIT_XDATE.HasValue ? "Y" : null, "Permit Number", "Permit Expiry Date", results);

            if (STF_DOB.Date > DateTime.Today)
                results.Add(new ValidationResult(Msg.FutureDob, new[] { nameof(STF_DOB) }));
            else if (DateTime.Today.Year - STF_DOB.Year - (DateTime.Today.DayOfYear < STF_DOB.DayOfYear ? 1 : 0) < 15)
                results.Add(new ValidationResult(Msg.Under15, new[] { nameof(STF_DOB) }));

            if (STF_MARITAL_STAT == "M" && string.IsNullOrWhiteSpace(STF_SPS_NAME))
                results.Add(new ValidationResult(Msg.SpouseRequired, new[] { nameof(STF_SPS_NAME) }));

            if (db != null && !string.IsNullOrEmpty(STF_NO))
            {
                // Duplicates
                CheckDuplicate(db, s => s.STF_HKID, STF_HKID, "HKID No.", results);
                CheckDuplicate(db, s => s.STF_PP_NO, STF_PP_NO, "Passport No.", results);

                // Cross ID conflict
                if (!string.IsNullOrWhiteSpace(STF_HKID) && db.TBL_STAFF.Any(s => s.STF_PP_NO == STF_HKID.Trim() && s.STF_NO != STF_NO))
                    results.Add(new ValidationResult(string.Format(Msg.IdConflict, "[HKID No.]", STF_HKID), new[] { nameof(STF_HKID) }));

                if (!string.IsNullOrWhiteSpace(STF_PP_NO) && db.TBL_STAFF.Any(s => s.STF_HKID == STF_PP_NO.Trim() && s.STF_NO != STF_NO))
                    results.Add(new ValidationResult(string.Format(Msg.IdConflict, "[Passport No.]", STF_PP_NO), new[] { nameof(STF_PP_NO) }));

                // Payroll lock for name & bank
                var original = db.TBL_STAFF.AsNoTracking().FirstOrDefault(s => s.STF_NO == STF_NO);
                if (original != null && !AllowChangeBankAccount(STF_NO, userService, db))
                {
                    if (!string.Equals(original.STF_NAME?.Trim(), STF_NAME?.Trim(), StringComparison.OrdinalIgnoreCase))
                        results.Add(new ValidationResult(Msg.PayrollLockedName));

                    if (!string.Equals(original.STF_AC_BNK_CODE?.Trim(), STF_AC_BNK_CODE?.Trim()) ||
                        !string.Equals(original.STF_AC_CODE?.Trim(), STF_AC_CODE?.Trim()))
                        results.Add(new ValidationResult(Msg.PayrollLockedBank));
                }

                // Cross-center warning
                var userDept = userService?.GetCurrentUserDepartment();
                if (!string.IsNullOrEmpty(userDept) &&
                    db.TBL_PTCNTR.Any(c => c.PCT_STFNO == STF_NO && c.PCT_DEL_FLG == "N" && c.PCT_CNTR_CTR != userDept))
                {
                    results.Add(new ValidationResult(Msg.OtherCenterWarning));
                }
            }

            // Invalid bank code
            if (db != null && !string.IsNullOrWhiteSpace(STF_AC_BNK_CODE) &&
                !db.TBL_BANK.Any(b => b.BNK_CODE == STF_AC_BNK_CODE))
                results.Add(new ValidationResult(string.Format(Msg.InvalidBank, STF_AC_BNK_CODE), new[] { nameof(STF_AC_BNK_CODE) }));

            return results;
        }

        private void CheckDuplicate(DBnew db, Func<TBL_STAFF, string?> selector, string? value, string fieldName, List<ValidationResult> results)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                db.TBL_STAFF.Any(s => selector(s) == value.Trim() && s.STF_NO != STF_NO))
            {
                results.Add(new ValidationResult(string.Format(Msg.Duplicate, fieldName, value), new[] { fieldName == "HKID No." ? nameof(STF_HKID) : nameof(STF_PP_NO) }));
            }
        }

        private void BothOrNeither(string? a, string? b, string nameA, string nameB, List<ValidationResult> results)
        {
            if ((a != null && b == null) || (a == null && b != null))
                results.Add(new ValidationResult(string.Format(Msg.BothOrNone, nameA, nameB), new[] { nameA.Contains("Passport") ? nameof(STF_PP_NO) : nameof(STF_PERMITNO) }));
        }

        // === Translated Oracle Functions ===

        private bool CanModifyStaff(string staffNo, ICurrentUserService? userService, DBnew? db)
        {
            if (userService == null) return true;
            if (userService.IsHrbUser()) return true;
            if (userService.IsSsUser()) return true;
            return !db?.HR_PTCNTR.Any(c => c.PCT_STFNO == staffNo && c.PCT_TYPE == "PERM" && c.PCT_DEL_FLG == "N") ?? true;
        }

        private bool AllowChangeBankAccount(string staffNo, ICurrentUserService? userService, DBnew? db)
        {
            if (userService?.IsFsdOperator() == true) return true;

            var ctrl = db?.TBL_PYRLPROC_CTRL.FirstOrDefault();
            if (ctrl == null) return false;

            if (ctrl.PPC_LCK_STF_BNKAC?.ToUpper() == "N") return true;

            bool hasPayrollThisMonth = db?.TBL_PYRL_TX.Any(t => t.PRX_STFNO == staffNo && t.PRX_MRK_DELE == "N") ?? false;
            return !hasPayrollThisMonth;
        }

        // Full HKID validation with check digit
        public static bool IsValidHkid(string? hkid)
        {
            if (string.IsNullOrWhiteSpace(hkid)) return true;
            var clean = hkid.Replace("(", "").Replace(")", "").Replace(" ", "").ToUpper();
            if (clean.Length < 8 || clean.Length > 9) return false;

            var digits = clean.Take(clean.Length - 1).ToArray();
            var checkChar = clean[^1];

            int sum = 0;
            if (clean.Length == 9) // Two letters: AB123456A
            {
                sum += (10 + (digits[0] - 'A')) * 9;
                sum += (10 + (digits[1] - 'A')) * 8;
                for (int i = 2; i < 8; i++) sum += (digits[i] - '0') * (8 - i + 1);
            }
            else // One letter: A123456A
            {
                sum += 324; // 36 * 9
                sum += (10 + (digits[0] - 'A')) * 8;
                for (int i = 1; i < 7; i++) sum += (digits[i] - '0') * (8 - i);
            }

            int remainder = sum % 11;
            int checkDigit = (11 - remainder) % 11;
            char expected = checkDigit == 10 ? 'A' : checkDigit == 0 ? '0' : (char)('0' + checkDigit);

            return expected == checkChar;
        }
    }

    // Custom Attributes
    public class NoChineseAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s) && TBL_STAFF_DTO.Patterns.ChineseChars.IsMatch(s))
                return new ValidationResult(string.Format(TBL_STAFF_DTO.Msg.NoChinese, ctx.DisplayName));
            return ValidationResult.Success;
        }
    }

    public class HkMobileValidAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                var digits = new string(s.Where(char.IsDigit).ToArray());
                if (digits.Length >= 8 && (digits[0] == '2' || digits[0] == '3'))
                    return new ValidationResult(TBL_STAFF_DTO.Msg.HkMobileInvalid);
            }
            return ValidationResult.Success;
        }
    }

    public class HkIdFullValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s))
            {
                if (!TBL_STAFF_DTO.Patterns.HkIdBasic.IsMatch(s) || !TBL_STAFF_DTO.IsValidHkid(s))
                    return new ValidationResult(string.Format(TBL_STAFF_DTO.Msg.HkIdInvalid, s.Trim()));
            }
            return ValidationResult.Success;
        }
    }

    public class AccountCodeLengthAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is string s && s != null && s.Replace("-", "").Length > 12)
                return new ValidationResult(TBL_STAFF_DTO.Msg.AccountCodeTooLong);
            return ValidationResult.Success;
        }
    }

    // Assume you have this interface injected
    public interface ICurrentUserService
    {
        bool IsHrbUser();
        bool IsSsUser();
        bool IsFsdOperator();
        string? GetCurrentUserDepartment();
    }
}