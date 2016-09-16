using Newtonsoft.Json;
using RestSharp;
using SmartEnergy_Server.Models;
using System;
using System.Collections.Generic;

namespace SmartEnergy_Relay
{
    class Auth
    {
        public static User send(string username)
        {
            // Initialise user and verify with server
            RestClient client = new RestClient("http://smart-energy-server.azurewebsites.net/api/");
            RestRequest validationRequest = new RestRequest("Users/Username/" + username, Method.GET);
            validationRequest.RequestFormat = DataFormat.Json;
            IRestResponse validationResponse = client.Execute(validationRequest);
            User userData = new User();
            userData.Username = username;
            if ((int)validationResponse.StatusCode == 404)
            {
                Console.WriteLine("User not found. Register as " + username + "?\n");
                Console.ReadLine();
                RestRequest registrationRequest = new RestRequest("Users/", Method.POST);
                registrationRequest.RequestFormat = DataFormat.Json;
                registrationRequest.AddBody(userData);
                IRestResponse registrationResponse = client.Execute(registrationRequest);
                userData = JsonConvert.DeserializeObject<User>(registrationResponse.Content);
            }
            else if (((int)validationResponse.StatusCode) / 100 == 2)
            {
                userData = JsonConvert.DeserializeObject<List<User>>(validationResponse.Content)[0];
                return userData;
            }
            Console.WriteLine(validationResponse.StatusCode);
            return userData;
        }
    }
}
