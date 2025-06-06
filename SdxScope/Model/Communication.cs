﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;

namespace SdxScope
{
    internal class Communication : ViewModelBase
    {
        static public SerialPort DevicePort;

        private bool connectionStatus = false;
        public bool ConnectionStatus
        {
            get { return connectionStatus;  }
            set { connectionStatus = value; OnPropertyChanged(); }
        }

        public Communication(ref SerialPort devicePort)
        {
            DevicePort = devicePort;
            DevicePort.BaudRate = 1500000;
            DevicePort.Parity = Parity.None;
            DevicePort.DataBits = 8;
            DevicePort.StopBits = StopBits.Two;
            DevicePort.Handshake = Handshake.None;
            DevicePort.ReadTimeout = 500;
            DevicePort.WriteTimeout = 500;
        }

        public void ConnectBoard(String PortName)
        {
            try
            {
                if (DevicePort.IsOpen is false)
                {
                    DevicePort.PortName = PortName;
                    DevicePort.Open();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{e.Message}");
            }
            
            ConnectionStatus = DevicePort.IsOpen;

        }

        public void DisconnectBoard()
        {
            try
            {
                if (DevicePort.IsOpen)
                {
                    DevicePort.Close();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"{e.Message}");
            }
            
            ConnectionStatus = false;
        }   
    }
}
