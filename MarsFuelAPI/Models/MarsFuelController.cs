using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MarsFuelAPI.Models
{
    public class MarsFuelController
    {
        /// <summary>
        /// Хранилище
        /// </summary>
        public Storage Storage { get; set; }
        /// <summary>
        /// Расписание по месяцам
        /// </summary>
        public List<FuelSchedule> FuelSchedule { get; set; }
        private FuelSchedule CurrentSchedule { get; set; }
        public MarsFuelController(InputData inputData)
        {
            Storage = new Storage(inputData);
        }
        public void InitModel(List<MonthInit> monthInits, CancellationToken token)
        {
            FuelSchedule = new List<FuelSchedule>();
            for(int i = 0; i < monthInits.Count; i++)
            {
                FuelSchedule fuelSchedule = new FuelSchedule
                {
                    MonthId = i + 1,
                    Month = monthInits[i].month,
                    FuelCount = monthInits[i].value
                };
                FuelSchedule.Add(fuelSchedule);
            }
            var date = monthInits.FirstOrDefault();
            if (date != null)
                Storage.currentMonth = date.month;
            else
                InitDefaultMonth();

            CurrentSchedule = FuelSchedule.First();

            Start(token);
        }
        public void InitDefaultMonth()
        {
            DateTime date = DateTime.Now;
            Storage.currentMonth = date;
            FuelSchedule.Add(new Models.FuelSchedule
            {
                MonthId = 1,
                Month = date,
                FuelCount = 0,
            });
        }
        public void Start(CancellationToken token)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await NextStep();
                    }
                    catch(Exception ex) {
                        string excepr = ex.Message;
                    }
                }
            });
        }
        bool flag = true;
        public async Task NextStep()
        {
            await Storage.NextStep(CurrentSchedule, flag);

            var currentDate = FuelSchedule.FirstOrDefault(u => u.Month.Month == Storage.currentMonth.Month && u.Month.Year == Storage.currentMonth.Year);
            if (currentDate == null)
            {
                currentDate = InitNextMonth();
                flag = true;
            }
            else flag = false;
            CurrentSchedule = currentDate;
        }
        public FuelSchedule InitNextMonth()
        {
            FuelSchedule _fuelSchedule = new FuelSchedule
            {
                MonthId = FuelSchedule.Count() + 1,
                Month = Storage.currentMonth,
                FuelCount = FuelSchedule.Last().FuelCount,
            };
            FuelSchedule.Add(_fuelSchedule);
            return _fuelSchedule;
        }
        public void UpdateMonth(MonthModificator monthModificator)
        {
            var monthData = FuelSchedule.FirstOrDefault(u => u.MonthId == monthModificator.month);
            if(monthData != null)
            {
                monthData.FuelCount = monthModificator.value;
            }
        }
        public void AddMonth(double value)
        {
            var lastMonth = FuelSchedule.OrderByDescending(u => u.Month).FirstOrDefault();
            if(lastMonth != null)
            {
                FuelSchedule.Add(new Models.FuelSchedule
                {
                    FuelCount = value,
                    Month = lastMonth.Month.AddMonths(1),
                    MonthId = FuelSchedule.Count()
                });
            }
        }
    }
}
