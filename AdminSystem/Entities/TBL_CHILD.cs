using System;
using System.Collections.Generic;

namespace AdminSystem.Entities;

public partial class TBL_CHILD
{
    public string CHD_STFNO { get; set; } = null!;

    public bool CHD_CHILDNO { get; set; }

    public string CHD_NAME { get; set; } = null!;

    public string? CHD_ID_TYP { get; set; }

    public string? CHD_ID_NO { get; set; }

    public string? CHD_ID_ISSCNTY { get; set; }

    public string CHD_SEX { get; set; } = null!;

    public DateTime? CHD_DOB { get; set; }

    public string CHD_MARITAL_STATUS { get; set; } = null!;

    public string CHD_HEALTH { get; set; } = null!;

    public string CHD_ACTN { get; set; } = null!;

    public DateTime CHD_ACTNDATE { get; set; }

    public string CHD_ACTNUSER { get; set; } = null!;

    public DateTime TIMESTAMP { get; set; }

    public string? CHD_ACTNUSER_SSO { get; set; }

    public virtual TBL_STAFF CHD_STFNONavigation { get; set; } = null!;
}
