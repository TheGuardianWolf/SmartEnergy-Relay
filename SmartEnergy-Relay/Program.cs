using SmartEnergy_Server.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartEnergy_Relay
{
    // SmartEnergy-Relay Tasks:
    // Authunicate via username with server, create user if needed
    // Retrieve hardware ID and data from port.
    // Add device to user
    // POST data via batch endpoint on server every 60 seconds.
    class Program
    {
        static void Main(string[] args)
        {
            BaseStation baseStation = new BaseStation();

            baseStation.Initialise();

            API smartEnergyApi = new API();

            bool asyncSuccess = false;

            while (!asyncSuccess)
            {
                Task.Run(async () =>
                {
                    asyncSuccess = await smartEnergyApi.Authunicate();
                }).GetAwaiter().GetResult();
            }

            Device syncedDevice = null;

            Task.Run(async () =>
            {
                syncedDevice = await smartEnergyApi.SyncDevice(baseStation.GetHardwareId());
            }).GetAwaiter().GetResult();

            if (syncedDevice == null)
            {
                throw new InvalidOperationException("Device sync failed, restart application.");
            }

            baseStation.Device = syncedDevice;

            baseStation.StartRelay();

            while (true)
            {
                Console.WriteLine("Entering data submission loop, press Control-C to exit. Data will be transmitted every minute.");
                Console.WriteLine();
                Thread.Sleep(new TimeSpan(0, 1, 0));
                Queue<Data> dataToSubmit = new Queue<Data>();
                foreach (KeyValuePair<string, Queue<Tuple<decimal, DateTime>>> param in baseStation.Display.DisplayValues)
                {
                    if (param.Value.Count > 0)
                    {
                        DateTime endRange;
                        decimal instanceSum;
                        List<DateTime> instanceTimes = new List<DateTime>();
                        while (param.Value.Contains(null))
                        {
                            endRange = param.Value.Peek().Item2.AddSeconds(8);
                            instanceSum = 0.0M;

                            while (param.Value.Count > 0)
                            {
                                var instance = param.Value.Dequeue();
                                instanceTimes.Add(instance.Item2);
                                instanceSum += instance.Item1;

                                if (param.Value.Peek() == null)
                                {
                                    break;
                                }
                            }

                            if (instanceTimes.Count > 0)
                            {
                                dataToSubmit.Enqueue(new Data()
                                {
                                    DeviceId = baseStation.Device.Id,
                                    Time = instanceTimes[instanceTimes.Count / 2],
                                    Label = param.Key,
                                    Value = instanceSum / instanceTimes.Count
                                });

                                instanceTimes.Clear();
                            }
                        }
                    }
                }
                Task.Run(async () =>
                {
                    await smartEnergyApi.SubmitData(dataToSubmit);
                }).GetAwaiter().GetResult();
            }
        }
    }
}
