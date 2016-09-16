using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnergy_Relay
{
    public class Display
    {
        public const byte SyncPacket = 0xE0;
        public const byte TermPacket = 0xC2;

        public Dictionary<string, List<decimal>> DisplayValues = new Dictionary<string, List<decimal>>()
        {
            { "voltage", new List<decimal>() },
            { "current", new List<decimal>() },
            { "power", new List<decimal>() },
            { "frequency", new List<decimal>() },
            { "phase", new List<decimal>() }
        };

        private Dictionary<string, int> updateConfirms = new Dictionary<string, int>();
        private string Next = "";

        public void UpdateValues(string value)
        {
            decimal dec;
            bool isNumber = decimal.TryParse(value, out dec);
            bool isValid = false;
            string key = "";

            if (!isNumber)
            {
                switch (value)
                {
                    case "Volt":
                        isValid = true;
                        key = "voltage";
                        break;
                    case "Curr":
                        isValid = true;
                        key = "current";
                        break;
                    case "Poer":
                        isValid = true;
                        key = "power";
                        break;
                    case "Free":
                        isValid = true;
                        key = "frequency";
                        break;
                    case "PhA5":
                        isValid = true;
                        key = "phase";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                isValid = true;
            }
            
            if (isValid)
            {
                if (updateConfirms.ContainsKey(value))
                {
                    // Specifically chosen to require 2/3 of display values to agree per update.
                    // Update rate on display is per 400ms, with display refresh per 6ms, therefore 44*6 = 400/3.
                    if (updateConfirms[value] >= 10) 
                    {
                        if (!isNumber)
                        {
                            Next = key;
                        }
                        else
                        {
                            if (Next.Length > 0)
                            {
                                DisplayValues[Next].Add(dec);
                            }
                        }
                        updateConfirms.Clear();
                    }
                    else
                    {
                        updateConfirms[value]++;
                    }
                }
                else
                {
                    if (updateConfirms.Count > 3)
                    {
                        updateConfirms.Clear();
                    }
                    updateConfirms.Add(value, 1);
                }
            }
        }

        static public string DecodeChar(byte character)
        {
            if (character > 0x7F)
            {
                character -= 0x80;
                return DecodeChar(character) + ".";
            }

            switch (character)
            {
                case SyncPacket:
                    return "<";
                case TermPacket:
                    return ">";
                case 0x7E:
                    return "0";
                case 0x30:
                    return "1";
                case 0x6D:
                    return "2";
                case 0x79:
                    return "3";
                case 0x33:
                    return "4";
                case 0x5B:
                    return "5";
                case 0x5F:
                    return "6";
                case 0x70:
                    return "7";
                case 0x7F:
                    return "8";
                case 0x73:
                    return "9";
                case 0x77:
                    return "A";
                case 0x1F:
                    return "b";
                case 0x4E:
                    return "C";
                case 0x3D:
                    return "d";
                case 0x6F:
                    return "e";
                case 0x4F:
                    return "E";
                case 0x47:
                    return "F";
                case 0x37:
                    return "H";
                case 0x16:
                    return "h";
                case 0x06:
                    return "l";
                case 0x0E:
                    return "L";
                case 0x15:
                    return "n";
                case 0x1D:
                    return "o";
                case 0x67:
                    return "P";
                case 0x05:
                    return "r";
                case 0x0F:
                    return "t";
                case 0x1C:
                    return "u";
                case 0x3E:
                    return "U";
                case 0x27:
                    return "V";
                case 0x3B:
                    return "y";
                case 0x01:
                    return "-";
                case 0x08:
                    return "_";
                case 0x02:
                    return "'";
                case 0x22:
                    return "\"";
                case 0x62:
                    return "^";
                case 0x78:
                    return "]";
                case 0x00:
                    return " ";
                default:
                    return "?";
            }
        } 
    }
}
