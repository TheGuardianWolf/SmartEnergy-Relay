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
    // POST data via batch endpoint on server every 5 seconds.
    class Program
    {
        static void Main(string[] args)
        {
            BaseStation baseStation = new BaseStation();

            baseStation.Initialise();

            API smartEnergyApi = new API();

            // Success flag for authunication.
            bool asyncSuccess = false;

            // If we can't authunicate, keep trying.
            while (!asyncSuccess)
            {
                // Need to wrap in Task.Run otherwise console application cannot wait.
                Task.Run(async () =>
                {
                    asyncSuccess = await smartEnergyApi.Authunicate();
                }).GetAwaiter().GetResult();
            }

            // Create new device.
            Device syncedDevice = null;

            Task.Run(async () =>
            {
                syncedDevice = await smartEnergyApi.SyncDevice(baseStation.Device.HardwareId);
            }).GetAwaiter().GetResult();

            // Check if we recieved the device details from the server.
            if (syncedDevice == null)
            {
                throw new InvalidOperationException("Device sync failed, restart application.");
            }

            // Set the baseStation device to the device recieved from server.
            baseStation.Device = syncedDevice;

            // Begin the relay program.
            baseStation.StartRelay();

            Console.WriteLine("Entering data submission loop, press Control-C to exit. Data will be transmitted every 5 seconds.");
            Console.WriteLine();

            // The Loop.
            while (true)
            {
                // Submission frequency.
                Thread.Sleep(new TimeSpan(0, 0, 5));

                // Queue of data that's ready to submit.
                Queue<Data> dataToSubmit = new Queue<Data>();

                // Iterate over the display values to find any that is ready for submission.
                foreach (KeyValuePair<string, Queue<Tuple<decimal, DateTime>>> param in baseStation.Display.DisplayValues)
                {
                    // Ignore the parameters with no values stored.
                    if (param.Value.Count > 0)
                    {
                        decimal instanceSum;

                        // Track the submission times of individual reporting instances from the base station, 
                        // will be merging several readings into one as to not flood the database.
                        List<DateTime> instanceTimes = new List<DateTime>();
                        while ((param.Value.Count > 0) && (param.Value.Contains(null)))
                        {
                            // Remove any invalid nulls, this should not run if data is formatted correctly.
                            while ((param.Value.Count > 0) && (param.Value.Peek() == null))
                            {
                                param.Value.Dequeue();
                            }

                            // Then recheck count
                            if (param.Value.Count == 0)
                            {
                                break;
                            }

                            // Initialise the sum of the reporting instances.
                            instanceSum = 0.0M;

                            // While we have valid values next in the parameter value queue.
                            while ((param.Value.Count > 0) && (param.Value.Peek() != null))
                            {
                                // Deque and add time to the time list, the sum to the instance sum.
                                var instance = param.Value.Dequeue();
                                instanceTimes.Add(instance.Item2);
                                instanceSum += instance.Item1;
                            }

                            // Now we check if the queue is empty before checking for a null.
                            if ((param.Value.Count > 0) && (param.Value.Peek() == null))
                            {
                                // Remove the null if it's there.
                                param.Value.Dequeue();
                            }

                            // Check to see if we have any data in the queue to submit.
                            if (instanceTimes.Count > 0)
                            {
                                // If we do, enqueue it.
                                dataToSubmit.Enqueue(new Data()
                                {
                                    DeviceId = baseStation.Device.Id,
                                    Time = instanceTimes[instanceTimes.Count / 2], // Median of the instance times.
                                    Label = param.Key,
                                    Value = instanceSum / instanceTimes.Count // Average of the instance sums.
                                });

                                // Finally clear the list for reuse.
                                instanceTimes.Clear();
                            }
                        }
                    }
                }

                // Submit the data to the server.
                Task.Run(async () =>
                {
                    await smartEnergyApi.SubmitData(dataToSubmit);
                }).GetAwaiter().GetResult();
            }
        }
    }
}
