using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarsFuelAPI.Models
{
    public class FuelSchedule
    {
        public int MonthId { get; set; }
        public DateTime Month { get; set; }
        public double FuelCount { get; set; }
    }
}
