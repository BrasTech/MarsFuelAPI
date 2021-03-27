using MarsFuelAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MarsFuelAPI.Classes
{
    public class BackgroundService
    {
        private readonly SocketManager _SocketManager;

        public bool isRunning { get; set; }

        private MarsFuelController _marsFuelController;

        private CancellationTokenSource _tokenSrc;
        private CancellationToken _token;

        public BackgroundService(SocketManager SocketManager)
        {
            _SocketManager = SocketManager;
            isRunning = false;
        }
        // Конфигурация модели
        public void Configure(InputData inputData)
        {
            _marsFuelController = new MarsFuelController(inputData);
        }
        // Запуск модели
        public async Task Start(List<MonthInit> monthInits)
        {
            isRunning = true;

            _tokenSrc = new CancellationTokenSource();
            _token = _tokenSrc.Token;

            _marsFuelController.InitModel(monthInits, _token);

            while (isRunning)
            {
                await SendData(JsonConvert.SerializeObject(_marsFuelController));
                await Task.Delay(1000);
            }
        }
        public void Stop()
        {
            isRunning = false;
            _tokenSrc.Cancel();
        }

        public void ChangeMonthData(MonthModificator monthModificator)
        {
            _marsFuelController.UpdateMonth(monthModificator);
        }
        public void AddMonthData(double value)
        {
            _marsFuelController.AddMonth(value);
        }


        public async Task SendData(string data)
        {
            await _SocketManager.SendMessageToAllAsync(data);
        }

    }
}
