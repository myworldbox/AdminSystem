using System;
using System.Collections.Generic;

namespace AdminSystem.Models.Tables_new;

public partial class TBL_PTCNTR
{
    public string PCT_CNTR_CTR { get; set; } = null!;

    public string PCT_CNTR_SQN { get; set; } = null!;

    public string PCT_CNTR_YR { get; set; } = null!;

    public string PCT_STFNO { get; set; } = null!;

    public int? PCT_EMPNO { get; set; }

    public string? PCT_POSTID { get; set; }

    public string PCT_CNTR_POST { get; set; } = null!;

    public string? PCT_CNTR_CPOST { get; set; }

    public string PCT_WATCHMAN { get; set; } = null!;

    public decimal? PCT_MTH_AMT { get; set; }

    public decimal? PCT_DAY_AMT { get; set; }

    public decimal? PCT_HR_AMT { get; set; }

    public string PCT_SRV_CTR { get; set; } = null!;

    public DateTime PCT_CNTR_START { get; set; }

    public DateTime PCT_CNTR_END { get; set; }

    public DateTime? PCT_CESS_DATE { get; set; }

    public decimal PCT_POST_FRACT1 { get; set; }

    public byte PCT_POST_FRACT2 { get; set; }

    public string PCT_STD_POST { get; set; } = null!;

    public string? PCT_EMP_STATUS { get; set; }

    public string PCT_STATUS { get; set; } = null!;

    public string? PCT_PAYM_FLG { get; set; }

    public string PCT_EMP_REASON { get; set; } = null!;

    public string PCT_SUPL_TECHR { get; set; } = null!;

    public string PCT_SUB_TECHR { get; set; } = null!;

    public string PCT_QUALIFY { get; set; } = null!;

    public string PCT_ENTRY_BY { get; set; } = null!;

    public DateTime PCT_ENTRY_DATE { get; set; }

    public string? PCT_APRV_BY { get; set; }

    public DateTime? PCT_APRV_DATE { get; set; }

    public string? PCT_HRB_CHK1_BY { get; set; }

    public DateTime? PCT_HRB_CHK1_DATE { get; set; }

    public string? PCT_HRB_CHK2_BY { get; set; }

    public DateTime? PCT_HRB_CHK2_DATE { get; set; }

    public string? PCT_REVERT_BY { get; set; }

    public DateTime? PCT_REVERT_DATE { get; set; }

    public string PCT_DEL_FLG { get; set; } = null!;

    public string PCT_REVERT { get; set; } = null!;

    public string? PCT_REMARKS { get; set; }

    public DateTime TIMESTAMP { get; set; }

    public string PCT_REVERT_AFT_CUTOFF { get; set; } = null!;

    public DateTime? PCT_STF_LAST_TS { get; set; }

    public string PCT_IMP_WKR { get; set; } = null!;

    public string? PCT_ENTRY_BY_SSO { get; set; }

    public string? PCT_APRV_BY_SSO { get; set; }

    public string? PCT_HRB_CHK1_BY_SSO { get; set; }

    public string? PCT_HRB_CHK2_BY_SSO { get; set; }

    public string? PCT_REVERT_BY_SSO { get; set; }
}
