using System;
using System.Collections.Generic;

namespace AdminSystem.Entities;

public partial class TBL_STAFF
{
    public string STF_NO { get; set; } = null!;

    public string STF_NAME { get; set; } = null!;

    public string? STF_HKID { get; set; }

    public string? STF_PP_NO { get; set; }

    public string? STF_PP_ISCNTY { get; set; }

    public string? STF_NAT { get; set; }

    public DateTime STF_DOB { get; set; }

    public string STF_SEX { get; set; } = null!;

    public string STF_MARITAL_STAT { get; set; } = null!;

    public string STF_ADDR1 { get; set; } = null!;

    public string? STF_ADDR2 { get; set; }

    public string? STF_ADDR3 { get; set; }

    public string? STF_ADDR4 { get; set; }

    public string STF_ADDR_AREA { get; set; } = null!;

    public string? STF_AC_BNK_CODE { get; set; }

    public string? STF_AC_CODE { get; set; }

    public string? STF_SPS_NAME { get; set; }

    public string? STF_SPS_HKID { get; set; }

    public string? STF_SPS_PP_NO { get; set; }

    public string? STF_SPS_PP_ISCNTY { get; set; }

    public string? STF_SPS_HEALTH { get; set; }

    public string? STF_DAD_NAME { get; set; }

    public string? STF_DAD_HKID { get; set; }

    public string? STF_DAD_PP_NO { get; set; }

    public string? STF_DAD_PP_ISCNTY { get; set; }

    public string? STF_DAD_HEALTH { get; set; }

    public string? STF_MOM_NAME { get; set; }

    public string? STF_MOM_HKID { get; set; }

    public string? STF_MOM_PP_NO { get; set; }

    public string? STF_MOM_PP_ISCNTY { get; set; }

    public string? STF_MOM_HEALTH { get; set; }

    public string STF_ACTN { get; set; } = null!;

    public DateTime STF_ACTNDATE { get; set; }

    public string STF_ACTNUSER { get; set; } = null!;

    public DateTime TIMESTAMP { get; set; }

    public DateTime? STF_EMPVISA_XDATE { get; set; }

    public string? STF_PERMITNO { get; set; }

    public DateTime? STF_PERMIT_XDATE { get; set; }

    public string? STF_CNAME { get; set; }

    public string? STF_PHONE1 { get; set; }

    public string? STF_PHONE2 { get; set; }

    public string? STF_ACTNUSER_SSO { get; set; }

    public string STF_SURNAME { get; set; } = null!;

    public string? STF_GIVENNAME { get; set; }

    public string? STF_PHONE1AREACODE { get; set; }

    public string? STF_EMAIL { get; set; }

    public virtual TBL_BANK? STF_AC_BNK_CODENavigation { get; set; }

    public virtual ICollection<TBL_CHILD> TBL_CHILD { get; set; } = new List<TBL_CHILD>();

    public virtual ICollection<TBL_CNTR> TBL_CNTR { get; set; } = new List<TBL_CNTR>();
}
