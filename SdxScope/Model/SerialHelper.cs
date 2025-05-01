using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;

namespace SdxScope.Model
{
    public static class SerialHelper
    {
        // Stores the actual port data
        static private List<(string comPort, string description)> _comPortInfo = new();

        // Call this once to populate the list
        static private void LoadComPorts()
        {
            _comPortInfo.Clear();

            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'");

            foreach (var obj in searcher.Get())
            {
                string fullName = obj["Name"]?.ToString() ?? "";
                var match = Regex.Match(fullName, @"\((COM\d+)\)");

                if (!match.Success)
                    continue;

                string comPort = match.Groups[1].Value;
                string description = fullName.Replace($"({comPort})", "").Trim();

                _comPortInfo.Add((comPort, description));
            }
        }

        // Returns only COM port names: COM3, COM4, ...
        public static string[] GetComPorts()
        {
            LoadComPorts();
            return _comPortInfo.ConvertAll(p => p.comPort).ToArray();
        }

        // Returns display format: COM3 - CH340
        public static string[] GetComPortDescriptions()
        {
            LoadComPorts();
            return _comPortInfo.ConvertAll(p => $"{p.comPort} - {p.description}").ToArray();
        }

        // (Optional) Returns the internal list for advanced use
        public static List<(string comPort, string description)> GetComPortInfo()
        {
            LoadComPorts();
            return new List<(string, string)>(_comPortInfo);
        }
    }
}
