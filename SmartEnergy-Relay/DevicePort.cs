using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace SmartEnergy_Relay
{
    /// <summary>
    /// Wrapper around the SerialPort object.
    /// </summary>
    class DevicePort : IDisposable
    {
        /// <summary>
        /// Currently selected COM port.
        /// </summary>
        private string selectedCOMPort = "";

        /// <summary>
        /// Physical ID of the hardware.
        /// </summary>
        private string hardwareId = "";
        public string HardwareId
        {
            get
            {
                return hardwareId;
            }
        }

        /// <summary>
        /// The SerialPort object.
        /// </summary>
        private SerialPort port;
        public SerialPort Port {
            get
            {
                return port;
            }
        }

        /// <summary>
        /// Gathers a list of all COM devices on the computer (Windows only).
        /// </summary>
        /// <returns>A ManagementObject array with the COM devices attached.</returns>
        private ManagementObject[] getCOMDevices()
        {
            // Select COM devices.
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "root\\CIMV2",
                "SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\""
            );

            // Create new ManagementObject array and extract the array from the search object.
            ManagementObject[] COMDevices = new ManagementObject[searcher.Get().Count];
            if (COMDevices.Length == 0)
            {
                return COMDevices;
            }
            searcher.Get().CopyTo(COMDevices, 0);

            return COMDevices;
        }

        /// <summary>
        /// Prompts user to select a COM port.
        /// </summary>
        public void userSelectPort()
        {
            bool isParsed;
            int option;

            // Regex to capture the COM port from Windows device names.
            Regex extractPort = new Regex(@"\((COM\d+)\)");

            Console.WriteLine("Scanning for COM devices.");
            Console.WriteLine();

            ManagementObject[] COMDevices = null;

            // Keep searching until matches are found for COM devices.
            while ((COMDevices == null) || (COMDevices.Length == 0))
            {
                COMDevices = getCOMDevices();
            }

            Console.WriteLine("Available devices:");
            Console.WriteLine();

            // List choices for user.
            for (int i = 0; i < COMDevices.Length; i++)
            {
                Console.WriteLine(i.ToString() + ". " + COMDevices[i]["Name"].ToString());
                Console.WriteLine();
            }

            Console.WriteLine("Please select a device: ");

            // Get user input.
            isParsed = int.TryParse(Console.ReadLine(), out option);
            if (isParsed)
            {
                // Validate user selection.
                Match match = extractPort.Match(COMDevices[option]["Name"].ToString());
                if (match.Success)
                {
                    selectedCOMPort = match.Groups[1].ToString();
                    hardwareId = ((string[]) COMDevices[option]["HardwareID"])[0];
                }
            }
            Console.WriteLine();
            if (selectedCOMPort.Length == 0)
            {
                Console.WriteLine("Sorry, there was an error in your selection.");
            }
            else
            {
                Console.WriteLine("COM Port set to " + selectedCOMPort + ".");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Wrapper for userSelectPort method.
        /// </summary>
        /// <returns></returns>
        public bool SelectPort()
        {
            userSelectPort();

            // If port exists, dispose the last one.
            if (port != null)
            {
                port.Dispose();
            }

            // Check for a specified COM port.
            if (selectedCOMPort.Length != 0)
            {
                // Set SerialPort to selected COM port, 9600 baud rate, odd parity with 8 data bits.
                port = new SerialPort(selectedCOMPort.ToString(), 9600, Parity.Odd, 8);
                port.Handshake = Handshake.None;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a SerialDataRecievedEventHandler to the current handlers.
        /// </summary>
        /// <param name="handle">An function to handle incoming serial data.</param>
        public void AddDataRecievedEventHandler(SerialDataReceivedEventHandler handle)
        {
            port.DataReceived += handle;
        }

        /// <summary>
        /// DevicePort constructor.
        /// </summary>
        public DevicePort()
        {

        }

        /// <summary>
        /// Class implements dispose() to properly remove its SerialPort after use.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Destructor for DevicePort.
        /// </summary>
        ~DevicePort()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        /// <summary>
        /// Private member function for disposing.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (port != null)
                {
                    port.Dispose();
                    port = null;
                }
            }
        }
    }
}
