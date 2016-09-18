using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace SmartEnergy_Relay
{
    class DevicePort
    {
        private string selectedCOMPort = "";
        private string hardwareId = "";

        public string HardwareId { get; }

        public SerialPort port;

        public SerialPort Port { get; }

        private ManagementObject[] getCOMDevices()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "root\\CIMV2",
                "SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\""
            );
            ManagementObject[] COMDevices = new ManagementObject[searcher.Get().Count];
            searcher.Get().CopyTo(COMDevices, 0);
            return COMDevices;
        }

        public void userSelectPort()
        {
            bool isParsed;
            int option;
            Regex extractPort = new Regex(@"\((COM\d)\)");

            ManagementObject[] COMDevices = getCOMDevices();

            Console.WriteLine(string.Concat(Enumerable.Repeat("=", 50)));
            Console.WriteLine();
            Console.WriteLine("Available devices:");
            Console.WriteLine();
            for (int i = 0; i < COMDevices.Length; i++)
            {
                Console.WriteLine(i.ToString() + ". " + COMDevices[i]["Name"].ToString());
                Console.WriteLine();
            }

            Console.WriteLine("Please select a device: ");

            isParsed = int.TryParse(Console.ReadLine(), out option);
            if (isParsed)
            {
                Match match = extractPort.Match(COMDevices[option]["Name"].ToString());
                if (match.Success)
                {
                    selectedCOMPort = match.Groups[1].ToString();
                    hardwareId = COMDevices[option]["HardwareID"].ToString();
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
            Console.WriteLine(string.Concat(Enumerable.Repeat("=", 50)));
            Console.WriteLine();
        }

        public bool SelectPort()
        {
            userSelectPort();
            if (selectedCOMPort.Length != 0)
            {
                port = new SerialPort("COM" + selectedCOMPort.ToString(), 9600, Parity.Odd, 8);
                port.Handshake = Handshake.None;
                return true;
            }
            return false;
        }

        public DevicePort()
        {

        }
    }
}
