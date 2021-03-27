using MarsFuelAPI.Classes;
using MarsFuelAPI.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MarsFuelAPI.Models
{
    public class WorkerContract
    {
        public Worker worker { get; set; }
        public ContractType contractType { get; set; }
        public string contractName { get; set; }
        public int monthCount { get; set; }
        public int daysWorked { get; set; } = 0;

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            contractName = contractType.GetAttributeOfType<DescriptionAttribute>().Description;
        }
    }
}
