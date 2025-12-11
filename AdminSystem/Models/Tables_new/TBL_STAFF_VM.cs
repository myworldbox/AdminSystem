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
    internal abstract class TBL_STAFF_VM : TBL_STAFF_DTO { }
    partial class TBL_STAFF_DTO
    {
        // ──────────────────────────────────────────────────────────────────────
        // Centralized regex & constants — DO NOT TOUCH (as requested)
        // ──────────────────────────────────────────────────────────────────────
        internal abstract class Patterns
        {
            public static readonly Regex ChineseChars = new(@"[\u4E00-\u9FFF\u3400-\u4DBF\uF900-\uFAFF]", RegexOptions.Compiled);
            public static readonly Regex HkIdBasic = new(@"^[A-Z]{1,2}\d{6}\([0-9A]\)$|^[A-Z]{1,2}\d{6}[0-9A]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            public static readonly Regex NonDigits = new(@"\D", RegexOptions.Compiled);
        }

        internal abstract class Msg
        {
            public const string Required = "{0} is a compulsory field";
            public const string NoChinese = "Chinese characters are not allowed in {0}.";
            public const string HkMobileInvalid = "HK mobile number should not start with '2' or '3'. Please input a valid mobile number.";
            public const string HkIdInvalid = "[HKID No.] [{0}] is invalid. Please verify.";
            public const string AccountCodeTooLong = "Maximum length of [Account Code] is 12 excluding \"-\"";
            public const string NameTooLong = "The combined length of surname, space, and given name should not be more than 75 characters.";
            public const string MobileRequired = "Mobile and Country Code are mandatory fields.";
            public const string IdOrPassportRequired = "Please input [HK ID No.] or/and [Passport No.]";
            public const string PassportCountryRequired = "Please input [HK ID No.] or/and [Passport No.]";
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

        // Oracle logic translations
        private bool HasPermanentContract(string staffNo, DBnew? db) => db != null && (
            db.TBL_CNTR.Any(c => c.CNT_STFNO == staffNo && c.CNT_CESS_DATE == null) ||
            db.TBL_CNTR_TX.Any(c => c.CTT_STFNO == staffNo && c.CTT_CESS_DATE == null));

        public bool CanModifyStaff(string staffNo, ICurrentUserService? userService, DBnew? db)
        {
            if (userService == null || userService.IsHrbUser() || userService.IsSsUser()) return true;
            bool hasPartTime = db?.TBL_PTCNTR.Any(c => c.PCT_STFNO == staffNo && c.PCT_DEL_FLG == "N") ?? false;
            return !(hasPartTime && HasPermanentContract(staffNo, db));
        }

        public bool AllowChangeBankAccount(string staffNo, ICurrentUserService? userService, DBnew? db)
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

    public class UniqueStaffNoAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (value is not string staffNo || string.IsNullOrWhiteSpace(staffNo)) return ValidationResult.Success;
            var db = ctx.GetService(typeof(DBnew)) as DBnew;
            if (db?.TBL_STAFF.Any(s => s.STF_NO == staffNo.Trim()) == true)
                return new ValidationResult($"Staff No. ({staffNo}) already exists.");
            return ValidationResult.Success;
        }
    }

    public class UniqueHkIdOrPassportAttribute : ValidationAttribute
    {
        private readonly string _otherIdProperty;
        public UniqueHkIdOrPassportAttribute(string otherIdProperty) => _otherIdProperty = otherIdProperty;

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var db = ctx.GetService(typeof(DBnew)) as DBnew;
            if (db == null || string.IsNullOrWhiteSpace(value?.ToString())) return ValidationResult.Success;

            var instance = ctx.ObjectInstance as TBL_STAFF_DTO;
            var currentStaffNo = instance?.STF_NO?.Trim();
            var thisValue = value.ToString()!.Trim();

            var property = ctx.ObjectType.GetProperty(_otherIdProperty);
            var otherValue = property?.GetValue(instance)?.ToString()?.Trim();

            // Conflict: same as someone's HKID or Passport
            bool conflict = db.TBL_STAFF.Any(s =>
                s.STF_NO != currentStaffNo &&
                ((s.STF_HKID != null && s.STF_HKID.Trim() == thisValue) ||
                 (s.STF_PP_NO != null && s.STF_PP_NO.Trim() == thisValue)));

            if (conflict)
                return new ValidationResult(string.Format(Msg.IdConflict,
                    ctx.DisplayName == "HKID No." ? "[HKID No.]" : "[Passport No.]", thisValue));

            return ValidationResult.Success;
        }
    }

    public class MobileRequiredWithAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;
        public MobileRequiredWithAttribute(string otherProperty) => _otherProperty = otherProperty;

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (ctx.ObjectInstance is not TBL_STAFF_DTO instance) return ValidationResult.Success;
            if (instance.OperationMode.Equals("display", StringComparison.OrdinalIgnoreCase))
                return ValidationResult.Success;

            var otherProp = ctx.ObjectType.GetProperty(_otherProperty);
            var otherValue = otherProp?.GetValue(instance);

            bool thisHasValue = !string.IsNullOrWhiteSpace(value?.ToString());
            bool otherHasValue = !string.IsNullOrWhiteSpace(otherValue?.ToString());

            if ((thisHasValue && !otherHasValue) || (!thisHasValue && otherHasValue) || (!thisHasValue && !otherHasValue))
                return new ValidationResult(Msg.MobileRequired,
                    new[] { ctx.MemberName!, _otherProperty });

            return ValidationResult.Success;
        }
    }

    public class SpouseRequiredWhenMarriedAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (ctx.ObjectInstance is not TBL_STAFF_DTO instance) return ValidationResult.Success;
            if (instance.STF_MARITAL_STAT == "M" && string.IsNullOrWhiteSpace(instance.STF_SPS_NAME))
                return new ValidationResult(Msg.SpouseRequired);
            return ValidationResult.Success;
        }
    }

    public class FullNameMaxLengthAttribute : ValidationAttribute
    {
        private readonly int _max;
        public FullNameMaxLengthAttribute(int max) => _max = max;

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            if (ctx.ObjectInstance is not TBL_STAFF_DTO instance) return ValidationResult.Success;
            var full = $"{instance.STF_SURNAME?.Trim()} {instance.STF_GIVENNAME?.Trim()}".Trim();
            if (full.Length > _max)
                return new ValidationResult(Msg.NameTooLong);
            return ValidationResult.Success;
        }
    }

    public class PermitPairRequiredAttribute : ValidationAttribute
    {
        private readonly string _pairProperty;
        public PermitPairRequiredAttribute(string pairProperty) => _pairProperty = pairProperty;

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var instance = ctx.ObjectInstance as TBL_STAFF_DTO;
            var pairProp = ctx.ObjectType.GetProperty(_pairProperty);
            var pairValue = pairProp?.GetValue(instance);

            bool thisHasValue = value switch
            {
                string s => !string.IsNullOrWhiteSpace(s),
                DateTime d => d != default,
                _ => value != null
            };

            bool pairHasValue = pairValue switch
            {
                string s => !string.IsNullOrWhiteSpace(s),
                DateTime d => d != default,
                _ => pairValue != null
            };

            if (thisHasValue != pairHasValue)
                return new ValidationResult(string.Format(Msg.BothOrNone, ctx.DisplayName, pairProp?.Name?.Replace("STF_", "").Replace("_", " ")));

            return ValidationResult.Success;
        }
    }

    public class PermanentContractLockAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var instance = ctx.ObjectInstance as TBL_STAFF_DTO;
            var userService = ctx.GetService(typeof(ICurrentUserService)) as ICurrentUserService;
            var db = ctx.GetService(typeof(DBnew)) as DBnew;
            if (instance == null || string.IsNullOrWhiteSpace(instance.STF_NO)) return ValidationResult.Success;

            if (!instance.CanModifyStaff(instance.STF_NO, userService, db))
                return new ValidationResult(Msg.PermanentLocked);

            return ValidationResult.Success;
        }
    }

    public class CrossCenterContractWarningAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var instance = ctx.ObjectInstance as TBL_STAFF_DTO;
            var userService = ctx.GetService(typeof(ICurrentUserService)) as ICurrentUserService;
            var db = ctx.GetService(typeof(DBnew)) as DBnew;
            if (instance == null || string.IsNullOrWhiteSpace(instance.STF_NO)) return ValidationResult.Success;

            var userDept = userService?.GetCurrentUserDepartment();
            if (string.IsNullOrEmpty(userDept)) return ValidationResult.Success;

            if (db?.TBL_PTCNTR.Any(c => c.PCT_STFNO == instance.STF_NO && c.PCT_DEL_FLG == "N" && c.PCT_CNTR_CTR != userDept) == true)
                return new ValidationResult(Msg.OtherCenterWarning); // Warning, not error

            return ValidationResult.Success;
        }
    }

    public class NameChangeAllowedAfterPayrollAttribute : ValidationAttribute
    {
        private readonly string[] _nameFields;
        public NameChangeAllowedAfterPayrollAttribute(params string[] nameFields) => _nameFields = nameFields;

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var instance = ctx.ObjectInstance as TBL_STAFF_DTO;
            var db = ctx.GetService(typeof(DBnew)) as DBnew;
            var userService = ctx.GetService(typeof(ICurrentUserService)) as ICurrentUserService;
            if (instance == null || string.IsNullOrWhiteSpace(instance.STF_NO)) return ValidationResult.Success;

            if (instance.AllowChangeBankAccount(instance.STF_NO, userService, db)) return ValidationResult.Success;

            var original = db?.TBL_STAFF.AsNoTracking().FirstOrDefault(s => s.STF_NO == instance.STF_NO);
            if (original == null) return ValidationResult.Success;

            foreach (var field in _nameFields)
            {
                var prop = typeof(TBL_STAFF_DTO).GetProperty(field);
                var oldVal = prop?.GetValue(original)?.ToString()?.Trim();
                var newVal = prop?.GetValue(instance)?.ToString()?.Trim();
                if (!string.Equals(oldVal, newVal, StringComparison.OrdinalIgnoreCase))
                    return new ValidationResult(Msg.PayrollLockedName);
            }

            return ValidationResult.Success;
        }
    }

    public class BankAccountEditableAfterPayrollAttribute : ValidationAttribute
    {
        private readonly string _pairProperty;
        public BankAccountEditableAfterPayrollAttribute(string pairProperty) => _pairProperty = pairProperty;

        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var instance = ctx.ObjectInstance as TBL_STAFF_DTO;
            var db = ctx.GetService(typeof(DBnew)) as DBnew;
            var userService = ctx.GetService(typeof(ICurrentUserService)) as ICurrentUserService;
            if (instance == null || string.IsNullOrWhiteSpace(instance.STF_NO)) return ValidationResult.Success;

            if (instance.AllowChangeBankAccount(instance.STF_NO, userService, db)) return ValidationResult.Success;

            var original = db?.TBL_STAFF.AsNoTracking().FirstOrDefault(s => s.STF_NO == instance.STF_NO);
            if (original == null) return ValidationResult.Success;

            var thisChanged = !string.Equals(
                original.GetType().GetProperty(ctx.MemberName!)?.GetValue(original)?.ToString()?.Trim(),
                value?.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase);

            var pairProp = ctx.ObjectType.GetProperty(_pairProperty);
            var pairChanged = !string.Equals(
                pairProp?.GetValue(original)?.ToString()?.Trim(),
                pairProp?.GetValue(instance)?.ToString()?.Trim(), StringComparison.OrdinalIgnoreCase);

            if (thisChanged || pairChanged)
                return new ValidationResult(Msg.PayrollLockedBank);

            return ValidationResult.Success;
        }
    }

    public class RequireHkIdOrPassportAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext ctx)
        {
            var instance = ctx.ObjectInstance as TBL_STAFF_DTO;
            if (instance == null)
                return ValidationResult.Success;

            bool hasHkId = !string.IsNullOrWhiteSpace(instance.STF_HKID);
            bool hasPassport = !string.IsNullOrWhiteSpace(instance.STF_PP_NO);

            if (!hasHkId && !hasPassport)
                return new ValidationResult(Msg.IdOrPassportRequired);

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