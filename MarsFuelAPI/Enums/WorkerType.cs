using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MarsFuelAPI.Enums
{
    public enum WorkerType
    {
        [Description("cashier")]
        Cashier,
        [Description("refueler")]
        Refluerer,
        [Description("director")]
        Director,
        [Description("guard")]
        Guard
    }
}
