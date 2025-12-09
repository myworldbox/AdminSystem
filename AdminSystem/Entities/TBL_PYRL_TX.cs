using System;
using System.Collections.Generic;

namespace AdminSystem.Entities;

public partial class TBL_PYRL_TX
{
    public int PRX_SQ { get; set; }

    public string PRX_TX_REFNO { get; set; } = null!;

    public string PRX_CNTR_CTR { get; set; } = null!;

    public string PRX_CNTR_YR { get; set; } = null!;

    public string PRX_CNTR_SQN { get; set; } = null!;

    public string PRX_PAYM_CODE { get; set; } = null!;

    public string PRX_STFNO { get; set; } = null!;

    public string PRX_CHRG_CTR { get; set; } = null!;

    public DateTime PRX_PYRL_MTH { get; set; }

    public string? PRX_STF_TYP { get; set; }

    public DateTime PRX_FR_DATE { get; set; }

    public DateTime? PRX_TO_DATE { get; set; }

    public string? PRX_U_PAYSCALE { get; set; }

    public string? PRX_U_PT { get; set; }

    public string? PRX_L_PAYSCALE { get; set; }

    public string? PRX_L_PT { get; set; }

    public decimal? PRX_U_SCLAMT { get; set; }

    public decimal? PRX_L_SCLAMT { get; set; }

    public decimal? PRX_U_AMT { get; set; }

    public decimal? PRX_L_AMT { get; set; }

    public decimal PRX_POST_FRACT { get; set; }

    public decimal PRX_FRACT { get; set; }

    public decimal PRX_GROSS_AMT { get; set; }

    public decimal PRX_EEPF_TOTAL { get; set; }

    public decimal PRX_NET_AMT { get; set; }

    public decimal PRX_ERPF_TOTAL { get; set; }

    public string PRX_REVERT { get; set; } = null!;

    public byte? PRX_BATCH_NO { get; set; }

    public string PRX_MRK_DELE { get; set; } = null!;

    public DateTime TIMESTAMP { get; set; }

    public string? PRX_CHRG_AC { get; set; }

    public string? PRX_PFCHRG_AC { get; set; }
}
