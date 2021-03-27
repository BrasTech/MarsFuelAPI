using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarsFuelAPI.Models
{
    public class Busy
    {
        public bool IsBusy { get; set; }
        public bool IsForward { get; set; }
        public double GoodsCount { get; set; }
        public double TimeLeft { get; set; }
    }
}
