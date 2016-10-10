using SmartEnergy_Server.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace SmartEnergy_Relay
{
    class BaseStation : IDisposable 
    {
        private DevicePort devicePort = new DevicePort();

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

        private Display display = new Display();
        public Display Display
        {
            get
            {
                return display;
            }
        }

        private List<byte> byteStorage = new List<byte>();

        private List<string> parseStorage()
        {
            if (byteStorage.Count < 6)
            {
                return new List<string>();
            }

            List<string> decodedStrings = new List<string>();

            string decodedString = "";

            byte[] temp = new byte[6] { 0, 0, 0, 0, 0, 0 };

            int i = 0;

            for (i = 0; i < 6; i++)
            {
                temp[i] = byteStorage[i];
            }

            if ((i == 6) && (temp[0] == Display.SyncPacket))
            {
                if (temp[5] == Display.TermPacket)
                {
                    for (i = 1; i < 5; i++)
                    {
                        decodedString = Display.DecodeChar(temp[i]) + decodedString;
                    }
                    decodedStrings.Add(decodedString);
                }
                byteStorage.RemoveRange(0, 5);
            }

            while ((byteStorage.Count > 0) && (byteStorage[0] != Display.SyncPacket))
            {
                byteStorage.RemoveAt(0);
            }

            decodedStrings.AddRange(parseStorage());

            return decodedStrings;
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            using (SerialPort port = (SerialPort) sender)
            {
                int bytes = port.BytesToRead;
                byte[] buffer = new byte[bytes];
                port.Read(buffer, 0, bytes);
                List<string> parsed = new List<string>();
                for (int i = 0; i < bytes; i++)
                {
                    byteStorage.Add(buffer[i]);
                    if (byteStorage.Count >= 6)
                    {
                        parsed = parseStorage();
                    }
                }

                if (parsed.Count > 0)
                {
                    for (int i = 0; i < parsed.Count; i++)
                    {
                        display.UpdateValues(parsed[i]);
                    }
                }
            }
        }

        public void Initialise()
        {
            bool success = false;
            while (!success)
            {
                success = devicePort.SelectPort();
            }

            devicePort.AddDataRecievedEventHandler(new SerialDataReceivedEventHandler(DataReceivedHandler));

            device.HardwareId = devicePort.HardwareId;
        }

        public void StartRelay()
        {
            devicePort.Port.Open();
        }

        public BaseStation()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
