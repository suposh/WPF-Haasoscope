using System.Diagnostics;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SdxScope
{
    internal class BoardConfiguration: ViewModelBase
    {
        private SerialPort DevicePort;

        private UInt64 boardUniqueId;
        
        /// <value>
        ///  Unique serial number of board
        /// </value>
        public UInt64 BoardUniqueId
        {
            get
            {
                byte[] data = new byte[] { (byte)(30 + (byte)boardId), 142 };
                byte[] result = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                DevicePort.Write(data, 0, data.Length);
                Delay(10);
                try
                {
                    int readCount = DevicePort.Read(result, 0, 8);
                    if (readCount != 8)
                    {
                        throw new Exception("Recieved corrupt or incomplete Data");
                    }
                    result = result.Reverse().ToArray();
                }
                catch (Exception e)
                {
                    TraceMessage($"Read Failed" + $"{e.Message}");
                }
                boardUniqueId = BitConverter.ToUInt64(result, 0);
                TraceMessage($"{boardUniqueId:X}");
                return boardUniqueId;
            }
        }

        private Byte boardId;

        /// <value>
        /// Board Chain Index
        /// </value>
        public Byte BoardId
        {
            get { return boardId; }
            set { 
                byte[] data = new byte[] { (byte)(0 + (byte) value), 20 };
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                boardId = value;
            }
        }

        private Byte boardFirmware;

        /// <value>
        /// Verilog Configuration Version
        /// </value>
        public Byte BoardFirmware
        {
            get
            {
                byte[] data = new byte[] { (byte)(30 + (byte)boardId), 147 };
                DevicePort.Write(data, 0, data.Length);
                byte Version = 0xFF;
                try
                {
                    //Version = (Byte)
                    DevicePort.Read(data, 0, 1);
                    Version = data[0];
                    Trace.WriteLine("Get Version:" + Version);
                }
                catch (TimeoutException)
                { return Version; }
                return Version;
            }
            //set
            //{
            //    byte[] data = new byte[] { (byte)(30 + (byte)boardId), 147 };
            //    DevicePort.Write(data, 0, data.Length);
            //    try
            //    {
            //        DevicePort.Read(data, 0, 1);
            //        boardFirmware = data[0];
            //        Trace.WriteLine("Set Version:" + boardFirmware);
            //    }
            //    catch(TimeoutException)
            //    {   boardFirmware = 0xFF; }
            //}
        }

        private bool rollingEnabled;

        /// <value>
        /// Sets Enable/Disable Trigger Roll
        /// </value>
        public bool RollingEnabled
        {
            get{ return rollingEnabled;  }
            set{
                byte[] data;
                if (DevicePort != null)
                {
                    if(value)
                    {
                        data = new byte[] { 101 };
                        DevicePort.Write(data, 0, data.Length);
                    }
                    else
                    {
                        data = new byte[] { 102 };
                        DevicePort.Write(data, 0, data.Length);
                    }
                    TraceMessage(BitConverter.ToString(data, 0));
                    rollingEnabled = value;
                }
                else
                {
                    Trace.WriteLine("Board: " + BoardId.ToString() + "No Communication");
                }
            }
        }

        private UInt16 adcSampleSize;

        /// <value>
        /// Command 120
        /// </value>
        public UInt16 AdcSampleSize             
        {
            get{ return adcSampleSize;  }
            set{ 
                byte[] data = new byte[] { (byte)(120 + (byte)boardId) };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                adcSampleSize = value;
            }
        }

        private UInt16 triggerPaddingCount;

        /// <value>
        /// Command 121
        /// </value>
        public UInt16 TriggerPaddingCount       
        {
            get { return triggerPaddingCount; }
            set
            {
                byte[] data = new byte[] { 121 };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).Take(3).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                triggerPaddingCount = value;
            }
        }

        private UInt16 sampleTransmitCount;

        /// <value>
        /// Command 122
        /// </value>
        public UInt16 SampleTransmitCount       
        {
            get { return sampleTransmitCount;  }
            set {
                byte[] data = new byte[] { 122  };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                sampleTransmitCount = value;
            }
        }

        private Byte byteSkipCount;

        /// <value>
        /// Command 123
        /// </value>
        public Byte ByteSkipCount               
        {
            get{ return byteSkipCount;  }
            set{
                byte[] data = new byte[] { (byte)(123), value };
                //data = data;
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                byteSkipCount = value;
            }
        }

        private Byte downsample;

        /// <value>
        /// Command 124
        /// </value>
        public Byte Downsample                  
        {
            get{ return downsample;  }
            set{
                byte[] data = new byte[] { 124, value };
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                downsample = value; 
            }
        }

        private Byte ticksToWait;

        /// <value>
        /// Command 125
        /// </value>
        public Byte TicksToWait                 
        {
            get{ return ticksToWait;  }
            set{
                byte[] data = new byte[] { 125, value };
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                ticksToWait = value; 
            }
        }

        private Byte triggerThreshold;

        /// <value>
        /// Command 127
        /// </value>
        public Byte TriggerThreshold
        {
            get { return triggerThreshold; }
            set
            {
                byte[] data = new byte[] { (byte)(127), value };
                //data = data;
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                triggerThreshold = value;
                OnPropertyChanged();
            }
        }

        private UInt16 triggerTime;

        /// <value>
        /// Command 129
        /// </value>
        public UInt16 TriggerTime               
        {
            get{ return triggerTime;  }
            set{
                byte[] data = new byte[] { 129 };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                triggerTime = value; 
            }
        }

        private byte triggerEnable;

        /// <value>
        /// Command 130
        /// </value>
        public byte TriggerEnable
        {
            get { return triggerEnable; }
            set
            {
                byte[] data = new byte[] { 130 , value };
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                triggerEnable = value;
            }
        }


        private UInt16 serialWaitMax;

        /// <value>
        /// Command 135
        /// </value>
        public UInt16 SerialWaitMax             
        {
            get{ return this.serialWaitMax;  }
            set{
                byte[] data = new byte[] { 135 };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                this.serialWaitMax = value; 
            }
        }

        private UInt16 lockInShift;

        /// <value>
        /// Command 138
        /// </value>
        public UInt16 LockInShift               
        {
            get{ return lockInShift;  }
            set{
                byte[] data = new byte[] { 138  };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                lockInShift = value; 
            }
        }

        private bool hiResEnabled = false;

        /// <value>
        /// Command 143
        /// </value>
        public bool HiResEnabled                
        {
            get { return hiResEnabled; }
            set
            {
                byte[] data = new byte[] { 143 };
                if (DevicePort != null)
                {
                    DevicePort.Write(data, 0, data.Length);
                    TraceMessage(BitConverter.ToString(data, 0));
                    hiResEnabled = !HiResEnabled;
                }
                else
                {
                    Trace.WriteLine("Board: " + BoardId.ToString() + "No Communication");
                }
            }
        }

        private UInt16 adcWrite;

        /// <value>
        /// Command 131
        /// </value>
        public UInt16 AdcWrite
        {
            get { return adcWrite; }
            set
            {
                byte[] data = new byte[] { 131  };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                adcWrite = value;
            }
        }

        private UInt64 ioExp;

        /// <value>
        /// 
        /// </value>
        public UInt64 IoExp
        {
            get { return ioExp; }
            set
            {
                byte[] data = new byte[] { 136 };
                data = data.Concat(BitConverter.GetBytes(value)).Take(7).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                ioExp = value;
            }
        }

        public BoardConfiguration(Byte boardId, ref SerialPort devicePort)
        {
            DevicePort = devicePort;
            BoardId = boardId;
            Trace.WriteLine("CTOR GET BoardFirmware: " + BoardFirmware);
        }

        public void Initializer()
        {
            //DevicePort.Write(new byte[] { (byte)(00 + (byte)BoardId) }, 0, 1);
            //DevicePort.Write(new byte[] { (byte)(20 + (byte)BoardId) }, 0, 1);
            //DevicePort.Write(new byte[] { (byte)(30 + (byte)BoardId) }, 0, 1);
            //Trace.WriteLine("Get Version:" + BoardFirmware);

            RollingEnabled = true;
            AdcSampleSize       = 8;
            SampleTransmitCount = 512;
            ByteSkipCount       = 0;
            
            Downsample          = 2;
            LockInShift         = 0;
            TicksToWait         = 1;

            HiResEnabled        = true;
            TriggerTime         = 8193;
            TriggerThreshold    = 10;
            SerialWaitMax       = 100;

            DevicePort.Write(new byte[] { 0x00 }, 0, 1);
            DevicePort.Write(new byte[] { 0x64 }, 0, 1);
            DevicePort.Write(new byte[] { 0x00 }, 0, 1);

            AdcWrite            = 0x0800;      // 2048 not connected, 0.9V 
            AdcWrite            = 0x0610;      // offset binary output + no clock divide 131 0x0610
            AdcWrite            = 0x041B;      // 150 Ohm termination chA 131 0x041B
            AdcWrite            = 0x051B;      // 150 Ohm termination chB 131 0x051B
            AdcWrite            = 0x0100;      // not multiplexed output 131 0x0100

            IoExp = BitConverter.ToUInt64(new byte[] {0x02, 0x20, 0x00, 0x00, 0xFF, 0xC8, 0x00, 0x00 }, 0 );       // port A on IOexp 1 are outputs 
            IoExp = BitConverter.ToUInt64(new byte[] {0x02, 0x20, 0x01, 0x00, 0xFF, 0xC8, 0x00, 0x00 }, 0 );       // port B on IOexp 1 are outputs 
            IoExp = BitConverter.ToUInt64(new byte[] {0x02, 0x21, 0x00, 0x00, 0xFF, 0xC8, 0x00, 0x00 }, 0 );       // port A on IOexp 2 are outputs  
            IoExp = BitConverter.ToUInt64(new byte[] {0x02, 0x20, 0x12, 0xF0, 0xFF, 0xC8, 0x00, 0x00 }, 0 );       // port A of IOexp 1   
            IoExp = BitConverter.ToUInt64(new byte[] {0x02, 0x20, 0x13, 0x0F, 0xFF, 0xC8, 0x00, 0x00 }, 0 );       // port B of IOexp 1  
            IoExp = BitConverter.ToUInt64(new byte[] {0x02, 0x21, 0x12, 0x00, 0xFF, 0xC8, 0x00, 0x00 }, 0 );       // port A of IOexp 2
            IoExp = BitConverter.ToUInt64(new byte[] {0x02, 0x21, 0x01, 0x00, 0xFF, 0xC8, 0x00, 0x00 }, 0 );       // port B on IOexp 2 are outputs 
            IoExp = BitConverter.ToUInt64(new byte[] {0x02, 0x21, 0x13, 0x00, 0xFF, 0xC8, 0x00, 0x00 }, 0 );       // port B of IOexp 2  

            Trace.WriteLine("Get BoardUniqueId: " + BoardUniqueId);

            IoExp = BitConverter.ToUInt64(new byte[] {0x03, 0x60, 0x50, 0x87, 0xF8, 0x00, 0x00, 0x00 }, 0);
            IoExp = BitConverter.ToUInt64(new byte[] {0x03, 0x60, 0x52, 0x88, 0x34, 0x00, 0x00, 0x00 }, 0);
            IoExp = BitConverter.ToUInt64(new byte[] {0x03, 0x60, 0x54, 0x88, 0x16, 0x00, 0x00, 0x00 }, 0);
            IoExp = BitConverter.ToUInt64(new byte[] {0x03, 0x60, 0x56, 0x88, 0x0C, 0x00, 0x00, 0x00 }, 0);

            TriggerPaddingCount = 0x0101;
            TriggerPaddingCount = 0x0101;

            DevicePort.Write(new byte[] { 127, 50 }, 0, 2);
            //DevicePort.Write(new byte[] { 139 }, 0, 1); 
        }
    }
}
