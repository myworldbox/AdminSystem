using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using AdminSystem.Data;
using Microsoft.EntityFrameworkCore;
using static AdminSystem.Models.Tables_new.TBL_STAFF_DTO;

namespace AdminSystem.Models.Tables_new
{
    public partial class TBL_STAFF_DTO : IValidatableObject
    {
        // ──────────────────────────────────────────────────────────────────────
        // Centralized regex & constants — DO NOT TOUCH (as requested)
        // ──────────────────────────────────────────────────────────────────────
        internal static class Patterns
        {
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

        // ──────────────────────────────────────────────────────────────────────
        // IValidatableObject — ONLY complex / DB / cross-field logic remains
        // ──────────────────────────────────────────────────────────────────────
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            var db = validationContext.GetService(typeof(DBnew)) as DBnew;
            var userService = validationContext.GetService(typeof(ICurrentUserService)) as ICurrentUserService;

            var mode = (OperationMode ?? "").Trim().ToLowerInvariant();
            bool isDisplay = mode == "display";
            string currentStaffNo = STF_NO?.Trim() ?? "";

            // 1. Permanent Contract Lock
            if (!string.IsNullOrEmpty(currentStaffNo) && !CanModifyStaff(currentStaffNo, userService, db))
            {
                results.Add(new ValidationResult(Msg.PermanentLocked));
                return results;
            }

            // 2. Mobile required only in insert/update
            if (!isDisplay)
            {
                if (string.IsNullOrWhiteSpace(STF_PHONE1AREACODE) || string.IsNullOrWhiteSpace(STF_PHONE1))
                    results.Add(new ValidationResult(Msg.MobileRequired, new[] { nameof(STF_PHONE1AREACODE), nameof(STF_PHONE1) }));
            }

            // 3. At least one identity document
            if (string.IsNullOrWhiteSpace(STF_HKID) && string.IsNullOrWhiteSpace(STF_PP_NO))
                results.Add(new ValidationResult(Msg.IdOrPassportRequired, new[] { nameof(STF_HKID), nameof(STF_PP_NO) }));

            // 4. Full English name ≤ 75 chars
            var fullName = $"{STF_SURNAME?.Trim()} {STF_GIVENNAME?.Trim()}".Trim();
            if (fullName.Length > 75)
                results.Add(new ValidationResult(Msg.NameTooLong));

            // 5. Surname has space but no Given Name → warning
            if (!isDisplay && !string.IsNullOrWhiteSpace(STF_SURNAME) &&
                STF_SURNAME.Trim().Contains(' ') && string.IsNullOrWhiteSpace(STF_GIVENNAME))
            {
                results.Add(new ValidationResult(
                    "Warning: If the full name can be separated, please enter it as Surname and Given Name.<br/>" +
                    "<span style=\"margin-left:20px;\">For example:</span><br/>" +
                    "<span style=\"margin-left:20px;\">\"CHAN TAI MAN\" should be entered as:</span><br/>" +
                    "<span style=\"margin-left:20px;\">Surname: CHAN</span><br/>" +
                    "<span style=\"margin-left:20px;\">Given Name: TAI MAN</span>",
                    new[] { nameof(STF_SURNAME), nameof(STF_GIVENNAME) }));
            }

            // 6. DB-dependent checks
            if (db != null && !string.IsNullOrEmpty(currentStaffNo))
            {
                ValidateUniquenessAndConflicts(db, currentStaffNo, results);
                ValidatePayrollLock(db, userService, currentStaffNo, results);
                ValidateCrossCenterWarning(db, userService, currentStaffNo, results);
            }

            return results;
        }

        // ──────────────────────────────────────────────────────────────────────
        // DB-only validation methods (unchanged)
        // ──────────────────────────────────────────────────────────────────────
        private void ValidateUniquenessAndConflicts(DBnew db, string currentStaffNo, List<ValidationResult> results)
        {
            CheckDuplicateId(db, s => s.STF_HKID, STF_HKID, "HKID No.", nameof(STF_HKID), currentStaffNo, results);
            CheckDuplicateId(db, s => s.STF_PP_NO, STF_PP_NO, "Passport No.", nameof(STF_PP_NO), currentStaffNo, results);

            if (!string.IsNullOrWhiteSpace(STF_HKID) &&
                db.TBL_STAFF.Any(s => s.STF_PP_NO != null && s.STF_PP_NO.Trim() == STF_HKID.Trim() && s.STF_NO != currentStaffNo))
                results.Add(new ValidationResult(string.Format(Msg.IdConflict, "[HKID No.]", STF_HKID), new[] { nameof(STF_HKID) }));

            if (!string.IsNullOrWhiteSpace(STF_PP_NO) &&
                db.TBL_STAFF.Any(s => s.STF_HKID != null && s.STF_HKID.Trim() == STF_PP_NO.Trim() && s.STF_NO != currentStaffNo))
                results.Add(new ValidationResult(string.Format(Msg.IdConflict, "[Passport No.]", STF_PP_NO), new[] { nameof(STF_PP_NO) }));
        }

        private void CheckDuplicateId(DBnew db, Func<TBL_STAFF, string?> selector, string? value, string fieldName, string memberName, string currentStaffNo, List<ValidationResult> results)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                db.TBL_STAFF.Any(s => selector(s) != null && selector(s).Trim() == value.Trim() && s.STF_NO != currentStaffNo))
            {
                results.Add(new ValidationResult(string.Format(Msg.Duplicate, fieldName, value.Trim()), new[] { memberName }));
            }
        }

        private void ValidatePayrollLock(DBnew db, ICurrentUserService? userService, string currentStaffNo, List<ValidationResult> results)
        {
            if (AllowChangeBankAccount(currentStaffNo, userService, db)) return;

            var original = db.TBL_STAFF.AsNoTracking().FirstOrDefault(s => s.STF_NO == currentStaffNo);
            if (original == null) return;

            bool nameChanged = !string.Equals(original.STF_SURNAME?.Trim(), STF_SURNAME?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                               !string.Equals(original.STF_NAME?.Trim(), STF_NAME?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                               !string.Equals(original.STF_GIVENNAME?.Trim(), STF_GIVENNAME?.Trim(), StringComparison.OrdinalIgnoreCase);

            bool bankChanged = !string.Equals(original.STF_AC_BNK_CODE?.Trim(), STF_AC_BNK_CODE?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                               !string.Equals(original.STF_AC_CODE?.Trim(), STF_AC_CODE?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (nameChanged) results.Add(new ValidationResult(Msg.PayrollLockedName));
            if (bankChanged) results.Add(new ValidationResult(Msg.PayrollLockedBank));
        }

        private void ValidateCrossCenterWarning(DBnew db, ICurrentUserService? userService, string currentStaffNo, List<ValidationResult> results)
        {
            var userDept = userService?.GetCurrentUserDepartment();
            if (string.IsNullOrEmpty(userDept)) return;

            if (db.TBL_PTCNTR.Any(c => c.PCT_STFNO == currentStaffNo && c.PCT_DEL_FLG == "N" && c.PCT_CNTR_CTR != userDept))
                results.Add(new ValidationResult(Msg.OtherCenterWarning));
        }

        // Oracle logic translations
        private bool HasPermanentContract(string staffNo, DBnew? db) => db != null && (
            db.TBL_CNTR.Any(c => c.CNT_STFNO == staffNo && c.CNT_CESS_DATE == null) ||
            db.TBL_CNTR_TX.Any(c => c.CTT_STFNO == staffNo && c.CTT_CESS_DATE == null));

        private bool CanModifyStaff(string staffNo, ICurrentUserService? userService, DBnew? db)
        {
            if (userService == null || userService.IsHrbUser() || userService.IsSsUser()) return true;
            bool hasPartTime = db?.TBL_PTCNTR.Any(c => c.PCT_STFNO == staffNo && c.PCT_DEL_FLG == "N") ?? false;
            return !(hasPartTime && HasPermanentContract(staffNo, db));
        }

        private bool AllowChangeBankAccount(string staffNo, ICurrentUserService? userService, DBnew? db)
        {
            if (userService?.IsFsdOperator() == true) return true;
            var ctrl = db?.TBL_PYRLPROC_CTRL.FirstOrDefault();
            if (ctrl?.PPC_LCK_STF_BNKAC?.ToUpper() == "N") return true;
            return !(db?.TBL_PYRL_TX.Any(t => t.PRX_STFNO == staffNo && t.PRX_MRK_DELE == "N") ?? false);
        }

        public static bool IsValidHkid(string? hkid)
        {
            if (string.IsNullOrWhiteSpace(hkid)) return true;
            var clean = hkid.Replace("(", "").Replace(")", "").Replace(" ", "").ToUpper();
            if (clean.Length < 8 || clean.Length > 9) return false;
            var chars = clean.Take(clean.Length - 1).ToArray();
            char check = clean[^1];

            int sum = clean.Length == 9
                ? (10 + (chars[0] - 'A')) * 9 + (10 + (chars[1] - 'A')) * 8
                : 324 + (10 + (chars[0] - 'A')) * 8;

            for (int i = clean.Length == 9 ? 2 : 1; i < chars.Length; i++)
                sum += (chars[i] - '0') * (9 - i);

            int remainder = sum % 11;
            char expected = (11 - remainder) % 11 == 10 ? 'A' : (char)('0' + (11 - remainder) % 11);
            return expected == check;
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    // Custom Validation Attributes (add these to your project)
    // ──────────────────────────────────────────────────────────────────────
    public class DateNotInFutureAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) =>
            value is not DateTime dt || dt.Date <= DateTime.Today;
    }

    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        public MinimumAgeAttribute(int minAge) => _minAge = minAge;
        public override bool IsValid(object? value)
        {
            if (value is not DateTime dt) return true;
            int age = DateTime.Today.Year - dt.Year;
            if (DateTime.Today.DayOfYear < dt.DayOfYear) age--;
            return age >= _minAge;
        }
    }

    public class RequiredIfOtherHasValueAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;
        public RequiredIfOtherHasValueAttribute(string otherProperty) => _otherProperty = otherProperty;

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var otherProp = ctx.ObjectType.GetProperty(_otherProperty);
            var otherValue = otherProp?.GetValue(ctx.ObjectInstance);

            bool otherHasValue = otherValue switch
            {
                string s => !string.IsNullOrWhiteSpace(s),
                DateTime d => d != default,
                not null => true,
                _ => false
            };

            if (otherHasValue && (value == null || (value is string str && string.IsNullOrWhiteSpace(str))))
                return new ValidationResult(ErrorMessage ?? $"{ctx.DisplayName} is required.");

            return ValidationResult.Success;
        }
    }

    public class RequiredWhenMaritalStatusIsMAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var instance = ctx.ObjectInstance as TBL_STAFF_DTO;
            if (instance?.STF_MARITAL_STAT == "M" && string.IsNullOrWhiteSpace(value as string))
                return new ValidationResult(ErrorMessage ?? Msg.SpouseRequired);
            return ValidationResult.Success;
        }
    }

    public class BankCodeExistsAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is not string code || string.IsNullOrWhiteSpace(code)) return ValidationResult.Success;
            var db = ctx.GetService(typeof(DBnew)) as DBnew;
            if (db == null || !db.TBL_BANK.Any(b => b.BNK_CODE == code.Trim()))
                return new ValidationResult(string.Format(Msg.InvalidBank, code.Trim()));
            return ValidationResult.Success;
        }
    }

    public class UniqueAttribute : ValidationAttribute
    {
        private readonly string _propertyName;

        public UniqueAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var dbContext = (DBnew)validationContext.GetService(typeof(DBnew));
            var entityType = validationContext.ObjectType;
            var property = entityType.GetProperty(_propertyName);

            if (property == null)
                return new ValidationResult($"Property '{_propertyName}' not found.");

            var setMethod = typeof(DbContext)
                .GetMethods()
                .First(m => m.Name == nameof(DbContext.Set)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 0);

            var generic = setMethod.MakeGenericMethod(entityType);
            var queryable = (IQueryable<object>)generic.Invoke(dbContext, null);

            var exists = queryable.Any(e => property.GetValue(e) != null && property.GetValue(e).Equals(value));

            if (exists)
                return new ValidationResult($"{_propertyName} must be unique.");

            return ValidationResult.Success;
        }

    }

    // Custom Attributes
    public class NoChineseAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s) && Patterns.ChineseChars.IsMatch(s))
                return new ValidationResult(string.Format(Msg.NoChinese, ctx.DisplayName));
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
                    return new ValidationResult(Msg.HkMobileInvalid);
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
                if (!Patterns.HkIdBasic.IsMatch(s) || !IsValidHkid(s))
                    return new ValidationResult(string.Format(Msg.HkIdInvalid, s.Trim()));
            }
            return ValidationResult.Success;
        }
    }

    public class AccountCodeLengthAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is string s && s != null && s.Replace("-", "").Length > 12)
                return new ValidationResult(Msg.AccountCodeTooLong);
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