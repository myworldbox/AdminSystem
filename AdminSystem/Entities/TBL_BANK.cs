using System;
using System.Collections.Generic;

namespace AdminSystem.Entities;

public partial class TBL_BANK
{
    public string BNK_CODE { get; set; } = null!;

    public string BNK_NAME { get; set; } = null!;

    public DateTime TIMESTAMP { get; set; }

    public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; } = new List<TBL_STAFF>();
}
