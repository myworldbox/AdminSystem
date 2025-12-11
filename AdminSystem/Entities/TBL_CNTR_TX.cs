using System;
using System.Collections.Generic;

namespace AdminSystem.Entities;

public partial class TBL_CNTR_TX
{
    public string CTT_REFNO { get; set; } = null!;

    public string CTT_CNTR_CTR { get; set; } = null!;

    public string CTT_CNTR_YR { get; set; } = null!;

    public string CTT_CNTR_SQN { get; set; } = null!;

    public string CTT_STFNO { get; set; } = null!;

    public DateTime? CTT_FR_DATE { get; set; }

    public DateTime? CTT_TO_DATE { get; set; }

    public DateTime? CTT_CNTR_START_DATE { get; set; }

    public DateTime? CTT_CNTR_END_DATE { get; set; }

    public string? CTT_CNTR_STATUS { get; set; }

    public string? CTT_EMPTERMS { get; set; }

    public string? CTT_EMPPKG { get; set; }

    public string? CTT_SERV_CTR { get; set; }

    public string? CTT_STFTYP { get; set; }

    public string? CTT_SR_NO { get; set; }

    public decimal? CTT_POST_FRACT { get; set; }

    public string? CTT_PAYSCALE { get; set; }

    public string? CTT_CURPT { get; set; }

    public string? CTT_MAXPT { get; set; }

    public decimal? CTT_SALAMT { get; set; }

    public string? CTT_FUNDSRC { get; set; }

    public string? CTT_RANK { get; set; }

    public string? CTT_OMITPT1 { get; set; }

    public string? CTT_OMITPT2 { get; set; }

    public string? CTT_OMITPT3 { get; set; }

    public string? CTT_OMITPT4 { get; set; }

    public string? CTT_OMITPT5 { get; set; }

    public string? CTT_POST { get; set; }

    public DateTime? CTT_INCRDATE { get; set; }

    public string? CTT_INCR_STATUS { get; set; }

    public string? CTT_INCR_CHG_CODE { get; set; }

    public DateTime? CTT_STOPEND_DATE { get; set; }

    public DateTime? CTT_DEFREND_DATE { get; set; }

    public DateTime? CTT_FSTAPPT_DATE { get; set; }

    public DateTime? CTT_PROBEND_DATE { get; set; }

    public DateTime? CTT_TRIALEND_DATE { get; set; }

    public DateTime? CTT_CURRNK_DATE { get; set; }

    public DateTime? CTT_CURPST_DATE { get; set; }

    public DateTime? CTT_MAXPT_DATE { get; set; }

    public string? CTT_PF_SCHEM { get; set; }

    public string? CTT_PF_CTYP { get; set; }

    public DateTime? CTT_PF_JOINDATE { get; set; }

    public DateTime? CTT_PF5DATE { get; set; }

    public DateTime? CTT_PF10DATE { get; set; }

    public DateTime? CTT_PF15DATE { get; set; }

    public byte? CTT_MPF_EESC_PERC { get; set; }

    public DateTime? CTT_MPF_EESC_DATE { get; set; }

    public string? CTT_MPF_LB_EEC_FLG { get; set; }

    public string? CTT_MPF_UB_EEC_FLG { get; set; }

    public string? CTT_MPF_UB_ERC_FLG { get; set; }

    public string? CTT_MPF_PAYCTR { get; set; }

    public string? CTT_MPF_STATUS { get; set; }

    public DateTime? CTT_MPF_LSTC_DATE { get; set; }

    public byte? CTT_GRAT_PERC { get; set; }

    public string? CTT_CESS_CODE { get; set; }

    public DateTime? CTT_CESS_DATE { get; set; }

    public string? CTT_REMARKS { get; set; }

    public string? CTT_2NDSCH_CNTR { get; set; }

    public string CTT_REC_STATUS { get; set; } = null!;

    public string CTT_CNTR_ACTN { get; set; } = null!;

    public string CTT_ENTRY_BY { get; set; } = null!;

    public DateTime CTT_ENTRY_DATE { get; set; }

    public string? CTT_VRFY_BY { get; set; }

    public DateTime? CTT_VRFY_DATE { get; set; }

    public string? CTT_PROC_BY { get; set; }

    public DateTime? CTT_PROC_DATE { get; set; }

    public string? CTT_DCHK_BY { get; set; }

    public DateTime? CTT_DCHK_DATE { get; set; }

    public string? CTT_APRV_BY { get; set; }

    public DateTime? CTT_APRV_DATE { get; set; }

    public string? CTT_REVT_BY { get; set; }

    public DateTime? CTT_REVT_DATE { get; set; }

    public string CTT_REVERT { get; set; } = null!;

    public string CTT_MRK_DELE { get; set; } = null!;

    public string CTT_TX_TYP { get; set; } = null!;

    public DateTime? CTT_PAYR_MTH { get; set; }

    public string? CTT_OVRWRI_RMK_OPT { get; set; }

    public string? CTT_COMP_GEN { get; set; }

    public string? CTT_WITHDRAW_RSGN { get; set; }

    public DateTime TIMESTAMP { get; set; }

    public string? CTT_OMITPT6 { get; set; }

    public string? CTT_OMITPT7 { get; set; }

    public string? CTT_OMITPT8 { get; set; }

    public string? CTT_REMINDER_NOTE { get; set; }

    public DateTime? CTT_REMINDER_DATE { get; set; }

    public DateTime? CTT_SECONDMENT_END_DATE { get; set; }

    public string? CTT_MPF_NHAN_FLG { get; set; }

    public string? CTT_MPF_TW_FLG { get; set; }

    public DateTime? CTT_MPF_TW_1STDATE { get; set; }

    public DateTime? CTT_MPF_TW_2NDDATE { get; set; }

    public string? CTT_PAY_LVL { get; set; }

    public DateTime? CTT_PSR_DATE { get; set; }

    public string? CTT_NPS_NOTE { get; set; }

    public DateTime? CTT_NPS_DATE { get; set; }

    public string? CTT_CUR_PF_PVDR { get; set; }

    public DateTime? CTT_PRJEND_DATE { get; set; }

    public string? CTT_STF_GRP { get; set; }

    public string? CTT_STF_CATG { get; set; }

    public DateTime? CTT_EMP_DATE { get; set; }
}
