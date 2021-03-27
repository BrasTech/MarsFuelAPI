using MarsFuelAPI.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MarsFuelAPI.Models
{
    public class GasStation
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Название АЗС
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Оставшееся топливо
        /// </summary>
        public double fuelRemained { get; set; }
        /// <summary>
        /// X - координата
        /// </summary>
        public double xCoordinate { get; set; }
        /// <summary>
        /// Y - координата
        /// </summary>
        public double yCoordinate { get; set; }
        /// <summary>
        /// Время обслуживания одной машины
        /// </summary>
        public double carMaintenance { get; set; }
        /// <summary>
        /// Прибыль с одной машины
        /// </summary>
        public double carProfit { get; set; } // Прибыль с одной машины
        public double avgBill { get; set; } // Средний чек
        public double magnificationFactor { get; set; } // Кэф увеличения среднего чека
        public double basicCostOfMaintenance { get; set; } // Базовая стоимость содержания
        public double additionalCostOfMaintenance { get; set; } // Доп стоимость содержания одного обслуживаемого места
        public double buildTime { get; set; } // Время постройки одного обслуживающего места
        public List<WorkerContract> workers { get; set; } //  Список рабочих
        public int workPlacesCountForNewWorker { get; set; } // Количество рабочих мест для нового кассира
        public double workerTerminationP { get; set; } // Вероятность увольнения по ГПХ
        /// <summary>
        /// Количество станций
        /// </summary>
        public int stationsCount { get; set; } = 1;
        private readonly int carCapacity = 50;
        public List<Worker> workersExample { get; set; }

        public bool isEmpty = true;

        public double CurrentCash { get; set; }
        public double AvgCash { get; set; }

        private double minFuel { get; set; } = 10;
        public List<double> newStationsOnCreate { get; set; } = new List<double>();
        public List<Tanker> tankersComing { get; set; } = new List<Tanker>();
        /// <summary>
        /// Отправка топлива на АЗС
        /// </summary>
        /// <param name="value"></param>
        public void Send(double value)
        {
            this.fuelRemained += Math.Round(value);
            if (this.fuelRemained > minFuel)
                this.isEmpty = false;
            else this.isEmpty = true;
        }

        public void DoSteps()
        {
            // Нанимаем рабочих, для которых требуется обслуживание того или иного АЗС
            var director = workers.FirstOrDefault(u => u.worker.position == Enums.WorkerType.Director);
            var guard = workers.FirstOrDefault(u => u.worker.position == Enums.WorkerType.Guard);
            var cashier = workers.FirstOrDefault(u => u.worker.position == Enums.WorkerType.Cashier);
            var refluerers = workers.Where(u => u.worker.position == Enums.WorkerType.Refluerer);
            var cashiers = workers.Where(u => u.worker.position == Enums.WorkerType.Cashier);

            if (director == null)
                AddWorker(WorkerType.Director);
            if (guard == null)
                AddWorker(WorkerType.Guard);
            if (cashier == null)
                AddWorker(WorkerType.Cashier);
            // --------------------------------------

            // Танкеры
            for(int i=0;i<tankersComing.Count;i++)
            {
                if(tankersComing[i].BusyInfo.TimeLeft >= 24)
                {
                    tankersComing[i].BusyInfo.TimeLeft -= 24;
                }
                else
                {
                    if (tankersComing[i].BusyInfo.IsForward)
                    {
                        fuelRemained += Math.Round(tankersComing[i].BusyInfo.GoodsCount);
                        tankersComing[i].BusyInfo.IsForward = false;
                        tankersComing[i].BusyInfo.TimeLeft = tankersComing[i].WorkTime;
                    }
                    else
                    {
                        tankersComing[i].BusyInfo.IsBusy = false;
                        tankersComing.RemoveAt(i);
                    }
                }
            }

            // Нанимаем кассиров
            int cashierCount = stationsCount % workPlacesCountForNewWorker;
            if (cashierCount > refluerers.Count())
            {
                for(int i = cashiers.Count(); i< cashierCount; i++)
                {
                    AddWorker(WorkerType.Cashier);
                }
            }
            // --------------------------------------
            // Нанимаем заправщиков
            if(refluerers.Count() < stationsCount)
            {
                for (int i = refluerers.Count(); i < stationsCount; i++)
                    AddWorker(WorkerType.Refluerer);
            }
            // --------------------------------------

            if (fuelRemained >= minFuel)
            {
                isEmpty = false;
                // Получаем средний чек и прибыль
                double cash = 0;
                int statCount = cashierCount * workPlacesCountForNewWorker > stationsCount ? stationsCount : cashierCount * workPlacesCountForNewWorker;
                // Из этого берем станции, на которые есть кассир.
                for (int i = 0; i < statCount; i++)
                {
                    //if(fuelRemained / carCapacity)
                    double carsCount = 24.0 * 60.0 / carMaintenance;
                    double enoughCars = Math.Floor(fuelRemained / carCapacity);
                    if(enoughCars == 0)
                    {
                        isEmpty = true;
                        break;
                    }
                    if (enoughCars < carsCount)
                    {
                        cash += enoughCars * carProfit;
                        fuelRemained -= Math.Round(enoughCars * carCapacity);
                        isEmpty = false;
                    }
                    else
                    {
                        cash += carsCount * carProfit;
                        fuelRemained -= Math.Round(carsCount * carCapacity);
                    }
                }
                CurrentCash += Math.Round(cash);
                AvgCash = carMaintenance * magnificationFactor * stationsCount;
            }
            else
            {
                isEmpty = true;
            }
            // --------------------------------------
            // Создание новой станции
            for(int i = 0; i < newStationsOnCreate.Count; i++)
            {
                if(newStationsOnCreate[i] < 24.0 * 60.0)
                {
                    stationsCount++;
                    newStationsOnCreate.RemoveAt(i);
                }
                else
                {
                    newStationsOnCreate[i] -= 24.0 * 60.0;
                }
            }
            // --------------------------------------

        }
        public void SendTanker(Tanker tanker)
        {
            tanker.BusyInfo.IsBusy = true;
            tankersComing.Add(tanker);
        }
        public void PayBills()
        {
            CurrentCash -= Math.Round(basicCostOfMaintenance);
            for(int i=0;i< stationsCount; i++)
            {
                CurrentCash -= Math.Round(additionalCostOfMaintenance);
            }
        }
        public void AddStation()
        {
            newStationsOnCreate.Add(buildTime);
        }
        public void RemoveStation()
        {
            stationsCount--;
        }
        /// <summary>
        /// Выплата ЗП
        /// </summary>
        public void DoSalary()
        {
            foreach(var workerItem in workers)
            {
                CurrentCash -= workerItem.worker.salary;
            }
        }
        /// <summary>
        /// Увольнение рабочих
        /// </summary>
        public void CheckWorkers()
        {
            Random r = new Random();
            for(int i=0;i<workers.Count();i++)
            {
                if(workers[i].contractType == ContractType.TK)
                {
                    workers[i].daysWorked++;
                    if(workers[i].daysWorked / 30 >= workers[i].monthCount)
                    {
                        AvgCash -= workers[i].worker.salary;
                        workers.RemoveAt(i);
                    }
                }
                else
                {
                    if(r.NextDouble() <= workerTerminationP / 100)
                    {
                        workers.RemoveAt(i);
                    }
                }
            }
        }
        /// <summary>
        /// Добавление рабочего
        /// </summary>
        /// <param name="workerType"></param>
        private void AddWorker(WorkerType workerType)
        {
            Random r = new Random();
            var contract = r.NextDouble() > 0.8 ? ContractType.GPH : ContractType.TK;
            int _monthCount = 0;
            if(contract == ContractType.TK)
            {
                _monthCount = r.Next(1, 48);
            }
            var _worker = workersExample.FirstOrDefault(u => u.position == workerType);
            workers.Add(new WorkerContract
            {
                contractType = ContractType.TK,
                monthCount = _monthCount,
                worker = new Worker
                {
                    position = _worker.position,
                    salary = _worker.salary,
                }
            });
        }
    }
}
