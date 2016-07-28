using Newtonsoft.Json;
using RestSharp;
using SmartEnergy_Server.Models;
using System;
using System.Collections.Generic;
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
            // Initialise user and verify with server
            Console.WriteLine("Please enter your username.\n");
            string username = Console.ReadLine();
            Console.WriteLine("\n");
            Console.WriteLine("Please wait for verification from server...\n");

            var client = new RestClient("http://smart-energy-server.azurewebsites.net/api/");
            var validationRequest = new RestRequest("Users/Username/" + username, Method.GET);
            validationRequest.RequestFormat = DataFormat.Json;
            var validationResponse = client.Execute(validationRequest);

            if ((int)validationResponse.StatusCode == 404)
            {
                Console.WriteLine("User not found. Register as " + username + "?\n");
                Console.ReadLine();
                var registrationRequest = new RestRequest("Users/", Method.POST);
                registrationRequest.RequestFormat = DataFormat.Json;
                registrationRequest.AddBody(new User
                {
                    Username = username
                });
                var registrationResponse = client.Execute(registrationRequest);
                var userData = JsonConvert.DeserializeObject<User>(registrationResponse.Content);
            }
            else if (((int)validationResponse.StatusCode)/100 == 2)
            {
                var userData = JsonConvert.DeserializeObject<List<User>>(validationResponse.Content)[0];
            }
            else
            {
                Console.WriteLine(validationResponse.StatusCode);
                return;
            }


            Thread.Sleep(new TimeSpan(1, 0, 0));
        }
    }
}
