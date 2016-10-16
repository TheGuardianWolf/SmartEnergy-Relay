using SmartEnergy_Server.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace SmartEnergy_Relay
{
    /// <summary>
    /// A class representing the base station the relay is connected to.
    /// </summary>
    class BaseStation : IDisposable 
    {
        /// <summary>
        /// SerialPort Wrapper.
        /// </summary>
        private DevicePort devicePort = new DevicePort();

        /// <summary>
        /// Abstract device object.
        /// </summary>
        private Device device = new Device();
        public Device Device
        {
            get
            {
                return device;
            }
            set
            {
                device = value;
            }
        }

        /// <summary>
        /// Abstract display object.
        /// </summary>
        private Display display = new Display();
        public Display Display
        {
            get
            {
                return display;
            }
        }

        /// <summary>
        /// Storage for the incoming bytes over SerialPort.
        /// </summary>
        private List<byte> byteStorage = new List<byte>();

        /// <summary>
        /// Member function for assembling collected bytes into meaningful display strings.
        /// </summary>
        /// <returns></returns>
        private List<string> parseStorage()
        {
            // If there is less than 6 bytes, there is nothing meaningful we can get from this.
            if (byteStorage.Count < 6)
            {
                return new List<string>();
            }

            List<string> decodedStrings = new List<string>();

            string decodedString = "";

            // Byte array for loading the bytes from list to be parsed.
            byte[] temp = new byte[6] { 0, 0, 0, 0, 0, 0 };

            int i = 0;

            // Load the byte array with the first 6 bytes.
            for (i = 0; i < 6; i++)
            {
                temp[i] = byteStorage[i];
            }

            // Check we have 6 loaded values and a sync packet as the first.
            if ((i == 6) && (temp[0] == Display.SyncPacket))
            {
                // Check we have a term packet as the last.
                if (temp[5] == Display.TermPacket)
                {
                    // Decode the middle 4 bytes into the display string.
                    for (i = 1; i < 5; i++)
                    {
                        decodedString = Display.DecodeChar(temp[i]) + decodedString;
                    }
                    // Add to the list of decoded strings.
                    decodedStrings.Add(decodedString);
                }
                // Clean up the byte storage by removing processed bytes.
                byteStorage.RemoveRange(0, 5);
            }

            // Remove any invalid trailing bytes until the next sync packet.
            while ((byteStorage.Count > 0) && (byteStorage[0] != Display.SyncPacket))
            {
                byteStorage.RemoveAt(0);
            }

            // Recurse and parse until byte storage is empty.
            decodedStrings.AddRange(parseStorage());

            return decodedStrings;
        }

        /// <summary>
        /// Triggers when data is recieved over SerialPort.
        /// </summary>
        /// <param name="sender">The SerialPort object sending the data.</param>
        /// <param name="e">Event arguments.</param>
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = (SerialPort)sender;
            //using (SerialPort port = (SerialPort) sender)
            //{
                // Read bytes into bytes buffer.
                int bytes = port.BytesToRead;
                byte[] buffer = new byte[bytes];
                port.Read(buffer, 0, bytes);

                // Set up list of parsed strings.
                List<string> parsed = new List<string>();

                // For every 6 bytes stored, call parseStorage() to resolve the byte storage.
                for (int i = 0; i < bytes; i++)
                {
                    // Transfer buffer contents to list storage.
                    byteStorage.Add(buffer[i]);
                    if (byteStorage.Count >= 6)
                    {
                        parsed = parseStorage();
                    }
                }

                // If a display string is detected, send the strings to the display object.
                if (parsed.Count > 0)
                {
                    for (int i = 0; i < parsed.Count; i++)
                    {
                        display.UpdateValues(parsed[i]);
                    }
                }
            //}
        }

        /// <summary>
        /// Allows user to select the serial port and assigns the data handler to the port's data recieved event.
        /// </summary>
        public void Initialise()
        {
            bool success = false;

            // If selection is not valid, keep prompting user to select a valid option.
            while (!success)
            {
                success = devicePort.SelectPort();
            }

            devicePort.AddDataRecievedEventHandler(new SerialDataReceivedEventHandler(DataReceivedHandler));

            device.HardwareId = devicePort.HardwareId;
        }

        /// <summary>
        /// Begins the serial port read.
        /// </summary>
        public void StartRelay()
        {
            devicePort.Port.Open();
        }

        public BaseStation()
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
        /// Private member function for disposing.
        /// </summary>
        /// <param name="disposing">Overload to differentiate from external calls.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (devicePort != null)
                {
                    devicePort.Dispose();
                    devicePort = null;
                }
            }
        }

    }
}
