using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnergy_Relay
{
    public class Display
    {
        // Set the line start and line terminator packets.
        // Line start is not needed but is used as the display needs the sync packet to function.
        public const byte SyncPacket = 0xE0;
        public const byte TermPacket = 0xC2;

        // Flag for if numbers are updating.
        private bool isUpdatingValues = true;

        // Storage for display parameter values.
        public Dictionary<string, Queue<Tuple<decimal, DateTime>>> DisplayValues = new Dictionary<string, Queue<Tuple<decimal, DateTime>>>()
        {
            { "voltage", new Queue<Tuple<decimal,DateTime>>() },
            { "current", new Queue<Tuple<decimal,DateTime>>() },
            { "power", new Queue<Tuple<decimal,DateTime>>() },
            { "frequency", new Queue<Tuple<decimal,DateTime>>() },
            { "phase", new Queue<Tuple<decimal,DateTime>>() }
        };

        // Storage for confirms
        private Dictionary<string, int> updateConfirms = new Dictionary<string, int>();
        // Next updated parameter.
        private string Next = "";

        // Function to update the parameter values from display input.
        // Accepts a string input value to be decoded.
        public void UpdateValues(string value)
        {
            decimal dec;
            // Is it a number? Check also for the negative shorthand used and recover the leading 0.
            bool isNumber = decimal.TryParse(value.Replace("-.'", "-0."), out dec);
            bool isValid = false;
            string key = "";

            if (!isNumber)
            {
                // If it's not a number, it is a display parameter.
                // Convert this to proper english.
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
                // If it's not a parameter, there's no way to check numerical values are valid. So default to true.
                isValid = true;
            }

            if (isValid)
            {
                if (updateConfirms.ContainsKey(value))
                {
                    // Specifically chosen to require 2/3 of display values to agree per update.
                    // Update rate on display is per 200ms, with display refresh per 7ms.
                    if (updateConfirms[value] >= 1)
                    {
                        if (!isNumber)
                        {
                            if ((isUpdatingValues) && (Next.Length > 0) && (DisplayValues[Next].Count > 0))
                            {
                                isUpdatingValues = false;
                                DisplayValues[Next].Enqueue(null);
                            }
                            else if (isUpdatingValues)
                            {
                                isUpdatingValues = false;
                            }
                            Next = key;
                        }
                        else
                        {
                            isUpdatingValues = true;
                            if (Next.Length > 0)
                            {
                                DisplayValues[Next].Enqueue(new Tuple<decimal, DateTime>(dec, DateTime.Now));
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
