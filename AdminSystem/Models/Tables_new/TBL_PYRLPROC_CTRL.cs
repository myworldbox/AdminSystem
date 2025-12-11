using System;
using System.Collections.Generic;

namespace AdminSystem.Models.Tables_new;

public partial class TBL_PYRLPROC_CTRL
{
    public byte? PPC_BATCH_NO { get; set; }

    public string? PPC_BATCH_STATUS { get; set; }

    public string? PPC_CNTR_INPROGRESS { get; set; }

    public string? PPC_FPD_INPROGRESS { get; set; }

    public string? PPC_LVD_INPROGRESS { get; set; }

    public string? PPC_OPD_INPROGRESS { get; set; }

    public string? PPC_OT_INPROGRESS { get; set; }

    public string? PPC_NCL_INPROGRESS { get; set; }

    public string? PPC_LCK_STF_BNKAC { get; set; }
}
