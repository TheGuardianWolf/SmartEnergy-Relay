using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace SmartEnergy_Relay
{
    class BaseStation
    {
        private SerialPort devicePort;

        public Display display = new Display();

        private List<byte> byteStorage = new List<byte>();

        private List<string> parseStorage()
        {
            if (byteStorage.Count < 6)
            {
                return new List<string>();
            }

            List<string> l = new List<string>();

            string s = "";

            byte[] temp = new byte[6]{ 0, 0, 0, 0, 0, 0};

            int i = 0;

            for (i = 0; i < 6; i++)
            {
                temp[i] = byteStorage[i];
            }

            if ( (i == 6) && (temp[0] == Display.SyncPacket))
            {
                if (temp[5] == Display.TermPacket)
                {
                    for (i = 1; i < 5; i++)
                    {
                        s = Display.DecodeChar(temp[i]) + s;
                    }
                    l.Add(s);
                }
                byteStorage.RemoveRange(0, 5);
            }
            
            while((byteStorage.Count > 0) && (byteStorage[0] != Display.SyncPacket))
            {
                byteStorage.RemoveAt(0);
            }

            l.AddRange(parseStorage());

            return l;
        }

        public BaseStation(uint port)
        {
            devicePort = new SerialPort("COM" + port.ToString(), 9600, Parity.Odd, 8);
            devicePort.Handshake = Handshake.None;
            devicePort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            devicePort.Open();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = (SerialPort) sender;
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
}
