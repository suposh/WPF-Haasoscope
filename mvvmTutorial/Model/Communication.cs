using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Composition;

namespace SdxScope
{
    internal class Communication
    {
        static public SerialPort DevicePort;

        static private bool connectionStatus = false;
        static public bool ConnectionStatus
        {
            get { return connectionStatus;  }
            set { connectionStatus = value; }
        }
        

        public Communication(ref SerialPort devicePort)
        {
            DevicePort = devicePort;
            DevicePort.PortName = "COM4";
            DevicePort.BaudRate = 1500000;
            DevicePort.Parity = Parity.None;
            DevicePort.DataBits = 8;
            DevicePort.StopBits = StopBits.Two;
            DevicePort.Handshake = Handshake.None;
            DevicePort.ReadTimeout = 1500;
            DevicePort.WriteTimeout = 1500;
        }

        public void ConnectBoard()
        {
            DevicePort.Open();
            ConnectionStatus = true;

            //readThread.Start();
        }
        public void DisconnectBoard()
        {
            ConnectionStatus = false;
            DevicePort.Close();
        }

        
    }
    
}
