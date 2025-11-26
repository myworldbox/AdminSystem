// File: Validators/TBL_STAFF_DTO_VALIDATOR.cs
using FluentValidation;
using AdminSystem.Data;
using AdminSystem.Models.Tables_new;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using static AdminSystem.Models.Tables_new.TBL_STAFF_DTO;

namespace AdminSystem.Validators
{
    public class TBL_STAFF_DTO_VALIDATOR : AbstractValidator<TBL_STAFF_DTO>
    {
        private readonly DBnew _db;
        private readonly ICurrentUserService _userService;

        // Reuse exact same constants from your original code
        private abstract class Msg : TBL_STAFF_DTO.Msg;
        private abstract class Patterns : TBL_STAFF_DTO.Patterns;

        public TBL_STAFF_DTO_VALIDATOR(DBnew db, ICurrentUserService userService)
        {
            _db = db;
            _userService = userService;

            // ==================================================================
            // 1. Operation Mode: Display → skip mobile required
            // ==================================================================
            When(x => string.IsNullOrWhiteSpace(x.OperationMode) ||
                      x.OperationMode.Trim().ToLowerInvariant() != "display", () =>
                      {
                          RuleFor(x => x.STF_PHONE1AREACODE)
                              .NotEmpty().WithMessage(Msg.MobileRequired)
                              .OverridePropertyName(nameof(TBL_STAFF_DTO.STF_PHONE1AREACODE));

                          RuleFor(x => x.STF_PHONE1)
                              .NotEmpty().WithMessage(Msg.MobileRequired)
                              .Must(m => string.IsNullOrWhiteSpace(m) ||
                                         !Regex.IsMatch(m.Trim(), @"^[23]"))
                              .WithMessage(Msg.HkMobileInvalid);
                      });

            // ==================================================================
            // 2. Basic Required + Format Rules (same as attributes)
            // ==================================================================
            RuleFor(x => x.STF_NO).NotEmpty().WithMessage(Msg.Required);
            RuleFor(x => x.STF_SURNAME).NotEmpty().WithMessage(Msg.Required);
            RuleFor(x => x.STF_NAME).NotEmpty().WithMessage(Msg.Required);
            RuleFor(x => x.STF_DOB)
                .NotEmpty()
                .LessThanOrEqualTo(DateTime.Today).WithMessage(Msg.FutureDob)
                .Must(dob => dob.AddYears(15) <= DateTime.Today).WithMessage(Msg.Under15);
            RuleFor(x => x.STF_SEX).NotEmpty().WithMessage(Msg.Required);
            RuleFor(x => x.STF_MARITAL_STAT).NotEmpty().WithMessage(Msg.Required);
            RuleFor(x => x.STF_ADDR1).NotEmpty().WithMessage(Msg.Required);
            RuleFor(x => x.STF_ADDR_AREA).NotEmpty().WithMessage(Msg.Required);
            RuleFor(x => x.STF_AC_BNK_CODE).NotEmpty().WithMessage(Msg.Required);
            RuleFor(x => x.STF_AC_CODE).NotEmpty().WithMessage(Msg.Required);

            // Full name ≤ 75
            RuleFor(x => x)
                .Must(x => $"{x.STF_SURNAME} {x.STF_GIVENNAME}".Trim().Length <= 75)
                .WithMessage(Msg.NameTooLong);

            // No Chinese
            RuleFor(x => x.STF_NAME).NotChinese("Staff Name in English");
            RuleFor(x => x.STF_GIVENNAME).NotChinese("Given Name");
            RuleFor(x => x.STF_ADDR1).NotChinese("First Line of Address");
            RuleFor(x => x.STF_ADDR2).NotChinese("Address Line 2");
            RuleFor(x => x.STF_ADDR3).NotChinese("Address Line 3");
            RuleFor(x => x.STF_ADDR4).NotChinese("Address Line 4");
            RuleFor(x => x.STF_PP_ISCNTY).NotChinese("Passport Issue Country");
            RuleFor(x => x.STF_NAT).NotChinese("Nationality");

            // HKID Full Validation
            RuleFor(x => x.STF_HKID)
                .Matches(Patterns.HkIdBasic).WithMessage(x => string.Format(Msg.HkIdInvalid, x.STF_HKID))
                .Must(hkid => string.IsNullOrWhiteSpace(hkid) || TBL_STAFF_DTO.IsValidHkid(hkid))
                .WithMessage(x => string.Format(Msg.HkIdInvalid, x.STF_HKID))
                .When(x => !string.IsNullOrWhiteSpace(x.STF_HKID));

            // Account Code ≤ 12 digits
            RuleFor(x => x.STF_AC_CODE)
                .Must(ac => string.IsNullOrWhiteSpace(ac) || ac.Replace("-", "").Length <= 12)
                .WithMessage(Msg.AccountCodeTooLong);

            // At least one ID
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.STF_HKID) || !string.IsNullOrWhiteSpace(x.STF_PP_NO))
                .WithMessage(Msg.IdOrPassportRequired);

            // Spouse required if married
            RuleFor(x => x.STF_SPS_NAME)
                .NotEmpty().WithMessage(Msg.SpouseRequired)
                .When(x => x.STF_MARITAL_STAT == "M");

            // Bank code exists
            RuleFor(x => x.STF_AC_BNK_CODE)
                .MustAsync(async (code, ct) => string.IsNullOrWhiteSpace(code) ||
                    await _db.TBL_BANK.AnyAsync(b => b.BNK_CODE == code.Trim(), ct))
                .WithMessage(x => string.Format(Msg.InvalidBank, x.STF_AC_BNK_CODE));

            // ==================================================================
            // 3. Surname has space but no Given Name → WARNING (not error)
            // ==================================================================
            When(x => x.OperationMode?.Trim().ToLowerInvariant() != "display" &&
                     !string.IsNullOrWhiteSpace(x.STF_SURNAME) &&
                     x.STF_SURNAME.Trim().Contains(' ') &&
                     string.IsNullOrWhiteSpace(x.STF_GIVENNAME), () =>
                     {
                         RuleFor(x => x.STF_GIVENNAME)
                             .Must(_ => false)
                             .WithMessage(
                                 "Warning: If the full name can be separated, please enter it as Surname and Given Name.<br/>" +
                                 "<span style=\"margin-left:20px;\">For example:</span><br/>" +
                                 "<span style=\"margin-left:20px;\">\"CHAN TAI MAN\" should be entered as:</span><br/>" +
                                 "<span style=\"margin-left:20px;\">Surname: CHAN</span><br/>" +
                                 "<span style=\"margin-left:20px;\">Given Name: TAI MAN</span>")
                             .WithErrorCode("Warning");
                     });

            // ==================================================================
            // 4. DB-DEPENDENT RULES (Only when STF_NO exists)
            // ==================================================================

            TBL_STAFF_DTO? ctx = null;

            When(x => {
                ctx = x;
                return !string.IsNullOrWhiteSpace(x.STF_NO); 
                }, () =>
                    {

                        var staffNo = ctx!.STF_NO.Trim();
                        // Uniqueness
                        RuleFor(x => x.STF_HKID)
                            .MustAsync(async (hkid, ct) => !await IsDuplicateAsync(staffNo, hkid, s => s.STF_HKID, ct))
                            .WithMessage(x => string.Format(Msg.Duplicate, "HKID No.", x.STF_HKID))
                            .When(x => !string.IsNullOrWhiteSpace(x.STF_HKID));

                        RuleFor(x => x.STF_PP_NO)
                            .MustAsync(async (pp, ct) => !await IsDuplicateAsync(staffNo, pp, s => s.STF_PP_NO, ct))
                            .WithMessage(x => string.Format(Msg.Duplicate, "Passport No.", x.STF_PP_NO))
                            .When(x => !string.IsNullOrWhiteSpace(x.STF_PP_NO));

                        // ID Conflict
                        RuleFor(x => x.STF_HKID)
                            .MustAsync(async (hkid, ct) => !await IsConflictAsync(staffNo, hkid, s => s.STF_PP_NO, ct))
                            .WithMessage(x => string.Format(Msg.IdConflict, "[HKID No.]", x.STF_HKID))
                            .When(x => !string.IsNullOrWhiteSpace(x.STF_HKID));

                        RuleFor(x => x.STF_PP_NO)
                            .MustAsync(async (pp, ct) => !await IsConflictAsync(staffNo, pp, s => s.STF_HKID, ct))
                            .WithMessage(x => string.Format(Msg.IdConflict, "[Passport No.]", x.STF_PP_NO))
                            .When(x => !string.IsNullOrWhiteSpace(x.STF_PP_NO));

                        // Permanent Contract Lock
                        RuleFor(x => x)
                            .MustAsync(async (_, ct) => await CanModifyStaffAsync(staffNo, ct))
                            .WithMessage(Msg.PermanentLocked);

                        // Payroll Lock
                        RuleFor(x => x)
                            .MustAsync(async (_, ct) => await CanChangeBankAndNameAsync(ctx, staffNo, ct))
                            .WithMessage(x => GetPayrollLockMessage(x, staffNo))
                            .WhenAsync(async (x, ct) => !await CanChangeBankAndNameAsync(x, staffNo, ct));

                        // Cross-center Warning
                        RuleFor(x => x)
                            .MustAsync(async (_, ct) => !await HasOtherCenterContract(staffNo, ct))
                            .WithMessage(Msg.OtherCenterWarning)
                            .WithErrorCode("Warning");
                    });
        }

        private async Task<bool> IsDuplicateAsync(string currentStaffNo, string? value,
            Func<TBL_STAFF, string?> selector, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return await _db.TBL_STAFF.AnyAsync(s =>
                s.STF_NO != currentStaffNo &&
                selector(s) != null &&
                selector(s)!.Trim() == value.Trim(), ct);
        }

        private async Task<bool> IsConflictAsync(string currentStaffNo, string? value,
            Func<TBL_STAFF, string?> selector, CancellationToken ct)
            => await IsDuplicateAsync(currentStaffNo, value, selector, ct);

        private async Task<bool> CanModifyStaffAsync(string staffNo, CancellationToken ct)
        {
            if (_userService.IsHrbUser() || _userService.IsSsUser()) return true;

            bool hasPermanent = await _db.TBL_CNTR.AnyAsync(c => c.CNT_STFNO == staffNo && c.CNT_CESS_DATE == null, ct) ||
                                await _db.TBL_CNTR_TX.AnyAsync(c => c.CTT_STFNO == staffNo && c.CTT_CESS_DATE == null, ct);

            bool hasPartTime = await _db.TBL_PTCNTR.AnyAsync(c => c.PCT_STFNO == staffNo && c.PCT_DEL_FLG == "N", ct);

            return !(hasPermanent && hasPartTime);
        }

        private async Task<bool> CanChangeBankAndNameAsync(TBL_STAFF_DTO dto, string staffNo, CancellationToken ct)
        {
            if (_userService.IsFsdOperator()) return true;
            var ctrl = await _db.TBL_PYRLPROC_CTRL.FirstOrDefaultAsync(ct);
            if (ctrl?.PPC_LCK_STF_BNKAC?.ToUpper() == "N") return true;
            return !await _db.TBL_PYRL_TX.AnyAsync(t => t.PRX_STFNO == staffNo && t.PRX_MRK_DELE == "N", ct);
        }

        private string GetPayrollLockMessage(TBL_STAFF_DTO dto, string staffNo)
        {
            var original = _db.TBL_STAFF.AsNoTracking().FirstOrDefault(s => s.STF_NO == staffNo);
            if (original == null) return Msg.PayrollLockedName;

            bool nameChanged = !string.Equals(original.STF_SURNAME?.Trim(), dto.STF_SURNAME?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                               !string.Equals(original.STF_NAME?.Trim(), dto.STF_NAME?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                               !string.Equals(original.STF_GIVENNAME?.Trim(), dto.STF_GIVENNAME?.Trim(), StringComparison.OrdinalIgnoreCase);

            bool bankChanged = !string.Equals(original.STF_AC_BNK_CODE?.Trim(), dto.STF_AC_BNK_CODE?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                               !string.Equals(original.STF_AC_CODE?.Trim(), dto.STF_AC_CODE?.Trim(), StringComparison.OrdinalIgnoreCase);

            return nameChanged && bankChanged
                ? $"{Msg.PayrollLockedName}\r\n{Msg.PayrollLockedBank}"
                : nameChanged ? Msg.PayrollLockedName : Msg.PayrollLockedBank;
        }

        private async Task<bool> HasOtherCenterContract(string staffNo, CancellationToken ct)
        {
            var dept = _userService.GetCurrentUserDepartment();
            if (string.IsNullOrEmpty(dept)) return false;
            return await _db.TBL_PTCNTR.AnyAsync(c =>
                c.PCT_STFNO == staffNo &&
                c.PCT_DEL_FLG == "N" &&
                c.PCT_CNTR_CTR != dept, ct);
        }
    }

    // Extension
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string?> NotChinese<T>(this IRuleBuilder<T, string?> rule, string fieldName)
        {
            return rule
                .Must(s => string.IsNullOrWhiteSpace(s) || !Patterns.ChineseChars.IsMatch(s))
                .WithMessage(string.Format(Msg.NoChinese, fieldName));
        }
    }
}