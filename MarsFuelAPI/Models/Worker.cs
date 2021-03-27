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
    public class Worker
    {
        public WorkerType position { get; set; }
        public double salary { get; set; }
        public string positionName { get; set; }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            positionName = position.GetAttributeOfType<DescriptionAttribute>().Description;
        }
    }
}
