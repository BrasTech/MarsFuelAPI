using MarsFuelAPI.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MarsFuelAPI.Models
{
    public class Storage
    {
        /// <summary>
        /// Количество оставшегося топлива
        /// </summary>
        public double fuelRemained { get; set; }
        /// <summary>
        /// Суммарная прибыль +
        /// </summary>
        public double totalProfit { get; set; }
        /// <summary>
        /// Актуальный месяц +
        /// </summary>
        public DateTime currentMonth { get; set; }
        /// <summary>
        /// Актуальный месяц +
        /// </summary>
        public string currentMonthString { get; set; }

        public int currentMonthId { get; set; }
        /// <summary>
        /// Список АЗС +
        /// </summary>
        public List<GasStation> gasStations { get; set; }
        /// <summary>
        /// Список танкеров
        /// </summary>
        public List<Tanker> tankers { get; set; }
        /// <summary>
        /// Список доступных рабочих
        /// </summary>
        public List<Worker> workers { get; set; }
        /// <summary>
        /// Количество АЗС +
        /// </summary>
        public int gasStationsCount { get; set; }
        /// <summary>
        /// Стоимость танкера
        /// </summary>
        public double tankerPrice { get; set; }
        /// <summary>
        /// Время постройки АЗС
        /// </summary>
        public double gasStationBuildTime { get; set; }
        /// <summary>
        /// Время работы
        /// </summary>
        public float monthSeconds { get; set; }
        /// <summary>
        /// Скорость работы
        /// </summary>
        public int speed { get; set; } = 1000;


        public Storage(InputData inputData)
        {
            fuelRemained = inputData.storeRest;
            tankerPrice = inputData.tankerPrice;
            gasStationBuildTime = inputData.azsBuildTime;
            monthSeconds = inputData.timeLength;
            workers = new List<Worker>
            {
                new Worker
                {
                    position = Enums.WorkerType.Director,
                    salary = inputData.directorSalary,
                },
                new Worker
                {
                    position = Enums.WorkerType.Cashier,
                    salary = inputData.cashierSalary
                },
                new Worker
                {
                    position = Enums.WorkerType.Guard,
                    salary = inputData.guardSalary
                },
                new Worker
                {
                    position = Enums.WorkerType.Refluerer,
                    salary = inputData.refuelerSalary
                },
            };
            gasStations = new List<GasStation>();
            tankers = new List<Tanker>();
            for(int i = 0; i < inputData.azsCount; i++)
            {
                double[] coordinates = GetCoordinates();
                GasStation gasStation = new GasStation
                {
                    isEmpty = true,
                    id = i + 1,
                    name = CreateMD5(i.ToString() + DateTime.Now.ToString()),
                    xCoordinate = coordinates[0],
                    yCoordinate = coordinates[1],
                    carMaintenance = inputData.serviceTime,
                    carProfit = inputData.profitPerOne,
                    avgBill = inputData.averageCheck,
                    magnificationFactor = inputData.checkIncreaseCoef,
                    basicCostOfMaintenance = inputData.mainMaintainceCost,
                    additionalCostOfMaintenance = inputData.otherMaintainceCost,
                    fuelRemained = inputData.azsRest,
                    buildTime = inputData.placeBuildTime,
                    workers = new List<WorkerContract>(),
                    workPlacesCountForNewWorker = inputData.newPlaceCondition,
                    workerTerminationP = inputData.dismissalProbability,
                    workersExample = new List<Worker>(workers),
                };
                gasStations.Add(gasStation);
            }
            for(int i = 0; i < inputData.tankersCount; i++)
            {
                Tanker tanker = new Tanker
                {
                    WorkTime = inputData.deliverTime,
                };
                tankers.Add(tanker);
            }
        }
        public async Task NextStep(FuelSchedule fuelSchedule, bool flag)
        {
            Random r = new Random();

            if(flag)
                this.fuelRemained += fuelSchedule.FuelCount;

            currentMonthId = fuelSchedule.MonthId;

            var emptyGasStations = gasStations.Where(u => u.isEmpty).ToList();
            double counter = emptyGasStations.Count;
            double mn = counter / gasStations.Count;

            if (emptyGasStations.Count >= 0) // Костыль для количества пустых айтемов, пусть будет пока что ноль
            {
                double[,] matrix = new double[2, emptyGasStations.Count + 1];
                for(int i = 0; i < emptyGasStations.Count; i++)
                {
                    matrix[0, 0] = fuelRemained * 0.2;
                    matrix[1, i + 1] = -emptyGasStations[i].AvgCash;
                    matrix[0, i + 1] = 1;
                }
                Simplex simplex = new Simplex(matrix);
                double[] result = new double[emptyGasStations.Count];
                var table_result = simplex.Calculate(result);
                if(result.Where(u=>u == 0).Count() == result.Count())
                {
                    for (int i = 0; i < emptyGasStations.Count; i++)
                    {
                        if (result[i] != 0)
                        {
                            var tanker = tankers.FirstOrDefault(u => !u.BusyInfo.IsBusy);
                            if (fuelRemained > 0)
                            {
                                var fuelCount = Math.Round(GetRandomNumber(0.05 * fuelRemained, 0.07 * fuelRemained));
                                // var fuelCount = r.Next(0, (int)Math.Floor(fuelRemained));
                                tanker.BusyInfo.GoodsCount = fuelCount;
                                tanker.BusyInfo.IsForward = true;
                                tanker.BusyInfo.TimeLeft = tanker.WorkTime;
                                emptyGasStations[i].SendTanker(tanker);
                                fuelRemained -= fuelCount;
                            }
                        }
                    }
                    
                }
                else
                {
                    for(int i=0;i<emptyGasStations.Count; i++)
                    {
                        if(result[i] != 0)
                        {
                            var tanker = tankers.FirstOrDefault(u => !u.BusyInfo.IsBusy);
                            if (tanker != null)
                            {
                                if(fuelRemained > 0)
                                {
                                    tanker.BusyInfo.GoodsCount = result[i];
                                    tanker.BusyInfo.IsForward = true;
                                    tanker.BusyInfo.TimeLeft = tanker.WorkTime;
                                    emptyGasStations[i].SendTanker(tanker);
                                    fuelRemained -= result[i];
                                }
                            }
                        }
                    }
                }

            }
            foreach(var gasStation in gasStations)
            {
                //gasStation.AddStation();

               /* if(gasStation.isEmpty)
                {
                    var tanker = tankers.FirstOrDefault(u => !u.BusyInfo.IsBusy);
                    if (tanker != null)
                    {
                        if (fuelRemained > 0) {
                            var fuelCount = Math.Round(GetRandomNumber(0.05 * fuelRemained, 0.07 * fuelRemained));
                           // var fuelCount = r.Next(0, (int)Math.Floor(fuelRemained));
                            tanker.BusyInfo.GoodsCount = fuelCount;
                            tanker.BusyInfo.IsForward = true;
                            tanker.BusyInfo.TimeLeft = tanker.WorkTime;
                            gasStation.SendTanker(tanker);
                            fuelRemained -= fuelCount;
                        }
                    }
                }*/
                gasStation.DoSteps();
                gasStation.CheckWorkers();

                if (currentMonth.Day == DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month))
                {
                    gasStation.DoSalary();
                    gasStation.PayBills();

                    var orderedStations = gasStations.OrderByDescending(u => u.CurrentCash).ToList();

                    var bestGasStation = orderedStations.FirstOrDefault();
                    double result = r.NextDouble();
                    if(result <= 0.7)
                    {
                        bestGasStation.AddStation();
                    }

                    var worstGasStation = orderedStations.LastOrDefault();
                    int count = worstGasStation.stationsCount;
                    if(count > 1)
                    {
                        result = r.NextDouble();
                        if(result < 0.1)
                        {
                            worstGasStation.RemoveStation();
                        }
                    }


                }
            }

            // День прошёл.

            totalProfit = 0;
            foreach(var gasStation in gasStations)
            {
                totalProfit += gasStation.CurrentCash;
            }

            await UpdateSpeed(fuelSchedule);
        }
        private async Task UpdateSpeed(FuelSchedule fuelSchedule)
        {
            if (currentMonth.Day == 1)
            {
                int days = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
                speed = (int)Math.Round(((monthSeconds / days) * 1000));
            }
            currentMonth = currentMonth.AddDays(1);
            await Task.Delay(speed);
        }

        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().Substring(0, 7).ToUpper();
            }
        }
        private double[] GetCoordinates()
        {
            Random r = new Random();
            double[] coordinates = new double[2];
            int radius = r.Next(0,43);
            coordinates[0] = r.Next(-radius, radius);
            coordinates[1] = Math.Round(Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(coordinates[0], 2)));
            int multiplier = r.NextDouble() > 0.5 ? 1 : -1;
            coordinates[1] = multiplier * coordinates[1];
            return coordinates;
        }
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            currentMonthString = currentMonth.ToString("dd/MM/yyyy");

        }
        public double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
