using SmartEnergy_Server.Models;
using System;
using System.Threading;

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
            // Program HEAD
            //Console.WriteLine("Please enter your username.\n");
            //string username = Console.ReadLine();
            //Console.WriteLine("\n");
            //Console.WriteLine("Please wait for verification from server...\n");

            //User userData = Auth.send(username);

            BaseStation baseStation = new BaseStation(9);

            Thread.Sleep(new TimeSpan(1, 0, 0));
        }
    }
}
