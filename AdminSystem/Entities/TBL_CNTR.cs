using System;
using System.Collections.Generic;

namespace AdminSystem.Entities;

public partial class TBL_CNTR
{
    public string CNT_CNTR_CTR { get; set; } = null!;

    public string CNT_CNTR_YR { get; set; } = null!;

    public string CNT_CNTR_SQN { get; set; } = null!;

    public string CNT_STFNO { get; set; } = null!;

    public DateTime CNT_FR_DATE { get; set; }

    public DateTime? CNT_TO_DATE { get; set; }

    public DateTime? CNT_CNTR_START_DATE { get; set; }

    public DateTime? CNT_CNTR_END_DATE { get; set; }

    public string? CNT_CNTR_STATUS { get; set; }

    public string CNT_EMPTERMS { get; set; } = null!;

    public string CNT_EMPPKG { get; set; } = null!;

    public string CNT_SERV_CTR { get; set; } = null!;

    public string? CNT_STFTYP { get; set; }

    public string? CNT_SR_NO { get; set; }

    public decimal? CNT_POST_FRACT { get; set; }

    public string? CNT_PAYSCALE { get; set; }

    public string? CNT_CURPT { get; set; }

    public string? CNT_MAXPT { get; set; }

    public decimal? CNT_SALAMT { get; set; }

    public string? CNT_FUNDSRC { get; set; }

    public string? CNT_RANK { get; set; }

    public string? CNT_OMITPT1 { get; set; }

    public string? CNT_OMITPT2 { get; set; }

    public string? CNT_OMITPT3 { get; set; }

    public string? CNT_OMITPT4 { get; set; }

    public string? CNT_OMITPT5 { get; set; }

    public string? CNT_POST { get; set; }

    public DateTime? CNT_INCRDATE { get; set; }

    public string? CNT_INCR_STATUS { get; set; }

    public string? CNT_INCR_CHG_CODE { get; set; }

    public DateTime? CNT_STOPEND_DATE { get; set; }

    public DateTime? CNT_DEFREND_DATE { get; set; }

    public DateTime CNT_FSTAPPT_DATE { get; set; }

    public DateTime? CNT_PROBEND_DATE { get; set; }

    public DateTime? CNT_TRIALEND_DATE { get; set; }

    public DateTime? CNT_CURRNK_DATE { get; set; }

    public DateTime? CNT_CURPST_DATE { get; set; }

    public DateTime? CNT_MAXPT_DATE { get; set; }

    public string CNT_PF_SCHEM { get; set; } = null!;

    public DateTime? CNT_PFJOIN_DATE { get; set; }

    public string? CNT_PF_CTYP { get; set; }

    public DateTime? CNT_PF5DATE { get; set; }

    public DateTime? CNT_PF10DATE { get; set; }

    public DateTime? CNT_PF15DATE { get; set; }

    public string? CNT_MPF_LB_EEC_FLG { get; set; }

    public string? CNT_MPF_UB_EEC_FLG { get; set; }

    public string? CNT_MPF_UB_ERC_FLG { get; set; }

    public byte? CNT_MPF_EESC_PERC { get; set; }

    public DateTime? CNT_MPF_EESC_DATE { get; set; }

    public string? CNT_MPF_PAYCTR { get; set; }

    public string? CNT_MPF_STATUS { get; set; }

    public DateTime? CNT_MPF_LSTC_DATE { get; set; }

    public byte? CNT_GRAT_PERC { get; set; }

    public string? CNT_CESS_CODE { get; set; }

    public DateTime? CNT_CESS_DATE { get; set; }

    public string? CNT_REMARKS { get; set; }

    public string? CNT_2NDSCH_CNTR { get; set; }

    public string CNT_CNTR_ACTN { get; set; } = null!;

    public string CNT_REFNO { get; set; } = null!;

    public string CNT_REC_STATUS { get; set; } = null!;

    public DateTime TIMESTAMP { get; set; }

    public DateTime? CNT_MPFSRV_STARTDATE { get; set; }

    public string? CNT_OMITPT6 { get; set; }

    public string? CNT_OMITPT7 { get; set; }

    public string? CNT_OMITPT8 { get; set; }

    public string? CNT_REMINDER_NOTE { get; set; }

    public DateTime? CNT_REMINDER_DATE { get; set; }

    public DateTime? CNT_SECONDMENT_END_DATE { get; set; }

    public string? CNT_MPF_NHAN_FLG { get; set; }

    public string CNT_MPF_TW_FLG { get; set; } = null!;

    public DateTime? CNT_MPF_TW_1STDATE { get; set; }

    public DateTime? CNT_MPF_TW_2NDDATE { get; set; }

    public string? CNT_PAY_LVL { get; set; }

    public DateTime? CNT_PSR_DATE { get; set; }

    public string? CNT_NPS_NOTE { get; set; }

    public DateTime? CNT_NPS_DATE { get; set; }

    public string? CNT_CUR_PF_PVDR { get; set; }

    public DateTime? CNT_PRJEND_DATE { get; set; }

    public string? CNT_STF_GRP { get; set; }

    public string? CNT_STF_CATG { get; set; }

    public DateTime CNT_EMP_DATE { get; set; }

    public virtual TBL_STAFF CNT_STFNONavigation { get; set; } = null!;
}
