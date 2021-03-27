using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarsFuelAPI.Models
{
    public class InputData
    {
        public double azsRest { get; set; }
        public double storeRest { get; set; }
        public int azsCount { get; set; }
        public int tankersCount { get; set; }
        public double tankerPrice { get; set; }
        public double deliverTime { get; set; }
        public double serviceTime { get; set; }
        public double profitPerOne { get; set; }
        public double averageCheck { get; set; }
        public double checkIncreaseCoef { get; set; }
        public double mainMaintainceCost { get; set; }
        public double otherMaintainceCost { get; set; }
        public double azsBuildTime { get; set; }
        public double placeBuildTime { get; set; }
        public double cashierSalary { get; set; }
        public double refuelerSalary { get; set; }
        public double directorSalary { get; set; }
        public double guardSalary { get; set; }
        public int newPlaceCondition { get; set; }
        public double dismissalProbability { get; set; }
        public int timeLength { get; set; }
    }
}
