using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarsFuelAPI.Models
{
    public class Tanker
    {
        public double WorkTime { get; set; }
        public Busy BusyInfo { get; set; } = new Busy();
    }
}
