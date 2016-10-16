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
        /// <summary>
        /// User the device belongs to.
        /// </summary>
        private User user = new User();

        /// <summary>
        /// URL of the SmartEnergy-Server to request from.
        /// </summary>
        private const string apiUrl = "https://smart-energy-server.azurewebsites.net/api/";

        /// <summary>
        /// RestSharp Client facillitates the simple REST requests.
        /// </summary>
        private RestClient client = new RestClient(apiUrl);

        /// <summary>
        /// Batch requests require a batch client only supported using HttpClient.
        /// </summary>
        private HttpClient batchClient = new HttpClient();

        /// <summary>
        /// Wrapper for the RestSharp request execution. Uses Newtonsoft JSON for deserialsing.
        /// </summary>
        /// <param name="request">The original RestRequest object.</param>
        /// <returns>An awaitable request task with IRestResponse on resolution.</returns>
        private Task<IRestResponse> ExecuteAsync(RestRequest request)
        {
            // Newtonsoft is superior, switch from default JSON serialiser.
            request.JsonSerializer = new RestSharp.Newtonsoft.Json.NewtonsoftJsonSerializer();
            var taskCompletionSource = new TaskCompletionSource<IRestResponse>();
            client.ExecuteAsync(request, (response) => taskCompletionSource.SetResult(response));
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// API class constructor. Takes no arguments.
        /// </summary>
        public API()
        {
        }

        /// <summary>
        /// Authunication protocol to be run from the main console program.
        /// </summary>
        /// <returns>An awaitable task with success flag on resolution.</returns>
        public async Task<bool> Authunicate()
        {
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

            // Wait for the response before continuing.
            IRestResponse validationResponse = await ExecuteAsync(validationRequest);

            // Not found error if User not registered.
            if ((int)validationResponse.StatusCode == 404)
            {
                // Confirm registration intent.
                Console.WriteLine("User not found. Register as '" + user.Username + "'? (Y/n)" + Environment.NewLine);
                if(Console.ReadLine() != "Y")
                {
                    return false;
                }
                
                // Scaffold new POST for user registration.
                RestRequest registrationRequest = new RestRequest("Users/", Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                registrationRequest.AddJsonBody(user);

                // Wait for successful registration to get the User ID.
                IRestResponse registrationResponse = await ExecuteAsync(registrationRequest);

                // 200 level result, success.
                if (((int)registrationResponse.StatusCode) / 100 == 2)
                {
                    // Recover user object from JSON and display response on console.
                    user = JsonConvert.DeserializeObject<User>(registrationResponse.Content);
                    Console.WriteLine("User registered.");
                    Console.WriteLine();
                    return true;
                }

                // Otherwise there's an error.
                Console.WriteLine("An error occured. (Status code: " + registrationResponse.StatusCode.ToString() + ")");
                Console.WriteLine();
                return false;
            }
            // 200 level result, success.
            else if (((int)validationResponse.StatusCode) / 100 == 2)
            {
                // Recover user object from JSON and display response on console.
                user = JsonConvert.DeserializeObject<List<User>>(validationResponse.Content)[0];
                Console.WriteLine("Successfully authunicated with API as '" + user.Username.ToString() + "'.");
                Console.WriteLine();
                return true;
            }

            // Otherwise there's an error.
            Console.WriteLine("An error occured. (Status code: " + validationResponse.StatusCode.ToString() + ")");
            Console.WriteLine();
            return false;
        }

        /// <summary>
        /// Synchronise device details with the SmartEnergy server.
        /// </summary>
        /// <param name="hardwareId">The physical ID of the device.</param>
        /// <returns>Awaitable task returning deserialised Device object on resolution.</returns>
        public async Task<Device> SyncDevice(string hardwareId)
        {
            RestRequest getDeviceRequest = new RestRequest("Devices/User/" + user.Id, Method.GET)
            {
                RequestFormat = DataFormat.Json
            };

            Console.WriteLine("Connecting to API...");
            Console.WriteLine();

            Console.WriteLine("Getting a list of your registered devices...");
            Console.WriteLine();

            // Wait for a responce from the server.
            IRestResponse getDeviceResponse = await ExecuteAsync(getDeviceRequest);

            // 200 level result, success.
            if (((int)getDeviceResponse.StatusCode) / 100 == 2)
            {
                List<Device> userDevices = JsonConvert.DeserializeObject<List<Device>>(getDeviceResponse.Content);

                // Check each device in the list sent back to see if our device has a matching hardwareID.
                foreach (Device userDevice in userDevices)
                {
                    if (userDevice.HardwareId == hardwareId)
                    {
                        Console.WriteLine("Device is currently registered as " + userDevice.Alias + ".");
                        Console.WriteLine();
                        return userDevice;
                    }
                }

                // If not, then create a new device for submission to server.
                Device syncedDevice = new Device()
                {
                    UserId = user.Id,
                    HardwareId = hardwareId
                };

                // Require a human readable name for the device.
                Console.WriteLine("Device is not current registered. Attempting to register, please provide an alias:");
                string alias = Console.ReadLine();
                syncedDevice.Alias = alias;

                // Send POST request.
                RestRequest registerDeviceRequest = new RestRequest("Devices/", Method.POST);
                registerDeviceRequest.AddJsonBody(syncedDevice);

                // Wait for response.
                IRestResponse registerDeviceResponse = await ExecuteAsync(registerDeviceRequest);

                // 200 level result, success.
                if (((int)registerDeviceResponse.StatusCode) / 100 == 2)
                {
                    syncedDevice = JsonConvert.DeserializeObject<Device>(registerDeviceResponse.Content);
                    Console.WriteLine("Device registered.");
                    Console.WriteLine();
                    return syncedDevice;
                }

                // On failure
                Console.WriteLine("An error occured. (Status code: " + registerDeviceResponse.StatusCode.ToString() + ")");
                Console.WriteLine();
                return null;
            }

            // On failure
            Console.WriteLine("An error occured. (Status code: " + getDeviceResponse.StatusCode.ToString() + ")");
            Console.WriteLine();
            return null;
        }

        /// <summary>
        /// Pushes data to the api via its RESTful batch api.
        /// </summary>
        /// <param name="dataList">List of data objects to serialise and send.</param>
        /// <returns>Awaitable task.</returns>
        public async Task SubmitData(Queue<Data> dataList)
        {
            // Create the multipart/mixed message content from the data list.
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

            // Wait for request to complete.
            await batchClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, apiUrl + "Batch/")
                {
                    Content = multipartData
                }
            );

            Console.WriteLine("Data submitted at: " + DateTime.Now.ToLocalTime().ToLongDateString());
            Console.WriteLine();
        }
    }
}
