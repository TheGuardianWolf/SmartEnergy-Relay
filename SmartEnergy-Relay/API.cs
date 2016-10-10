using SmartEnergy_Server.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RestSharp;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;

namespace SmartEnergy_Relay
{
    class API
    {
        private User user = new User();

        private const string apiUrl = "https://smart-energy-server.azurewebsites.net/api/";

        private RestClient client = new RestClient(apiUrl);

        private HttpClient batchClient = new HttpClient();

        private Task<IRestResponse> ExecuteAsync(RestRequest request)
        {
            request.JsonSerializer = new RestSharp.Newtonsoft.Json.NewtonsoftJsonSerializer();
            var taskCompletionSource = new TaskCompletionSource<IRestResponse>();
            client.ExecuteAsync(request, (response) => taskCompletionSource.SetResult(response));
            return taskCompletionSource.Task;
        }

        public API()
        {
        }

        public async Task<bool> Authunicate()
        {
            Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));

            Console.WriteLine();
            Console.WriteLine("Please enter your username:");
            user.Username = Console.ReadLine();
            Console.WriteLine();

            // Initialise user and verify with server
            RestRequest validationRequest = new RestRequest("Users/Username/" + user.Username, Method.GET)
            {
                RequestFormat = DataFormat.Json
            };

            Console.WriteLine("Connecting to API...");
            Console.WriteLine();

            IRestResponse validationResponse = await ExecuteAsync(validationRequest);
            if ((int)validationResponse.StatusCode == 404)
            {
                Console.WriteLine("User not found. Register as '" + user.Username + "'? (Y/n)" + Environment.NewLine);
                if(Console.ReadLine() != "Y")
                {
                    return false;
                }
                
                RestRequest registrationRequest = new RestRequest("Users/", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                registrationRequest.AddJsonBody(user);
                IRestResponse registrationResponse = await ExecuteAsync(registrationRequest);
                if (((int)registrationResponse.StatusCode) / 100 == 2)
                {
                    user = JsonConvert.DeserializeObject<User>(registrationResponse.Content);
                    Console.WriteLine("User registered.");
                    Console.WriteLine();
                    Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));
                    return true;
                }
                Console.WriteLine("An error occured. (Status code: " + registrationResponse.StatusCode.ToString() + ")");
                Console.WriteLine();
                Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));
                return false;
            }
            else if (((int)validationResponse.StatusCode) / 100 == 2)
            {
                user = JsonConvert.DeserializeObject<List<User>>(validationResponse.Content)[0];

                Console.WriteLine("Successfully authunicated with API as '" + user.Username.ToString() + "'.");
                Console.WriteLine();
                Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));

                return true;
            }

            Console.WriteLine("An error occured. (Status code: " + validationResponse.StatusCode.ToString() + ")");
            Console.WriteLine();
            Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));
            Console.WriteLine();
            return false;
        }

        public async Task<Device> SyncDevice(string hardwareId)
        {
            Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));

            RestRequest getDeviceRequest = new RestRequest("Devices/User/" + user.Id, Method.GET)
            {
                RequestFormat = DataFormat.Json
            };

            Console.WriteLine("Connecting to API...");
            Console.WriteLine();

            Console.WriteLine("Getting a list of your registered devices...");
            Console.WriteLine();

            IRestResponse getDeviceResponse = await ExecuteAsync(getDeviceRequest);

            if (((int)getDeviceResponse.StatusCode) / 100 == 2)
            {
                List<Device> userDevices = JsonConvert.DeserializeObject<List<Device>>(getDeviceResponse.Content);
                foreach (Device userDevice in userDevices)
                {
                    if (userDevice.HardwareId == hardwareId)
                    {
                        Console.WriteLine("Device is currently registered as " + userDevice.Alias + ".");
                        Console.WriteLine();
                        Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));
                        return userDevice;
                    }
                }

                Device syncedDevice = new Device()
                {
                    UserId = user.Id,
                    HardwareId = hardwareId
                };
                Console.WriteLine("Device is not current registered. Attempting to register, please provide an alias:");
                string alias = Console.ReadLine();
                syncedDevice.Alias = alias;
                RestRequest registerDeviceRequest = new RestRequest("Devices/", Method.POST);
                registerDeviceRequest.AddJsonBody(syncedDevice);
                IRestResponse registerDeviceResponse = await ExecuteAsync(registerDeviceRequest);

                if (((int)registerDeviceResponse.StatusCode) / 100 == 2)
                {
                    syncedDevice = JsonConvert.DeserializeObject<Device>(registerDeviceResponse.Content);
                    Console.WriteLine("Device registered.");
                    Console.WriteLine();
                    Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));
                    return syncedDevice;
                }
                Console.WriteLine("An error occured. (Status code: " + registerDeviceResponse.StatusCode.ToString() + ")");
                Console.WriteLine();
                Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));
                return null;
            }

            Console.WriteLine("An error occured. (Status code: " + getDeviceResponse.StatusCode.ToString() + ")");
            Console.WriteLine();
            Console.WriteLine(string.Concat(Enumerable.Repeat("=", 20)));
            Console.WriteLine();
            return null;
        }

        public async Task SubmitData(Queue<Data> dataList)
        {
            //Create the multipart/mixed message content
            MultipartContent multipartData = new MultipartContent("mixed", "batch_" + Guid.NewGuid().ToString());
            while (dataList.Count > 0)
            {
                multipartData.Add(
                    new HttpMessageContent(
                        new HttpRequestMessage(
                            HttpMethod.Post,
                            apiUrl + "Data/"
                        )
                        {
                            Content = new ObjectContent<Data>(dataList.Dequeue(), new JsonMediaTypeFormatter())
                        }
                    )
                );
            }

            await batchClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, apiUrl + "Batch/")
                {
                    Content = multipartData
                }
            );

            Console.WriteLine("Data submitted at: " + DateTime.Now.ToLocalTime().ToShortTimeString());
            Console.WriteLine();

        }
    }
}
