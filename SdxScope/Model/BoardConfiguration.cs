using System.Diagnostics;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Data;

namespace SdxScope
{
    internal class BoardConfiguration: ViewModelBase 
    {
        private SerialPort DevicePort;

        /// <value>
        ///  Unique serial number of board
        /// </value>
        public String BoardUniqueId
        {
            get
            {
                byte[] data = new byte[] { (byte)(30 + (byte)_BoardId), 142 };
                byte[] result = new byte[8];
                int maxRetries = 5; // try 5 times
                int attempt = 0;

                DevicePort.DiscardInBuffer();
                DevicePort.DiscardOutBuffer();

                DevicePort.Write(data, 0, data.Length);

                Delay(10); // short delay for device to respond

                while (attempt < maxRetries)
                {
                    try
                    {
                        int totalBytesRead = 0;
                        while (totalBytesRead < 8)
                        {
                            int bytesRead = DevicePort.Read(result, totalBytesRead, 8 - totalBytesRead);
                            if (bytesRead == 0)
                            {
                                throw new Exception("No data read from device");
                            }
                            totalBytesRead += bytesRead;
                        }

                        // Successfully read 8 bytes
                        result = result.Reverse().ToArray();
                        _BoardUniqueId = $"{BitConverter.ToUInt64(result, 0):X}";
                        TraceMessage($"Board Unique ID fetched: {_BoardUniqueId}");
                        return _BoardUniqueId;
                    }
                    catch (Exception ex)
                    {
                        TraceMessage($"Attempt {attempt + 1} failed: {ex.Message}");
                        attempt++;
                        Delay(20); // delay before retry
                    }
                }

                TraceMessage($"Failed to fetch Board Unique ID after {maxRetries} attempts.");
                return "UNKNOWN";
            }
        }
        private String _BoardUniqueId;

        /// <value>
        /// Board Chain Index
        /// </value>
        public Byte BoardId
        {
            get { return _BoardId; }
            set { 
                byte[] data = new byte[] { (byte)(0 + (byte) value), 20 };
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _BoardId = value;
            }
        }
        private Byte _BoardId;

        /// <value>
        /// Verilog Configuration Version
        /// </value>
        public Byte BoardFirmware
        {
            get
            {
                byte[] data = new byte[] { (byte)(30 + (byte)_BoardId), 147 };
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
            //    byte[] data = new byte[] { (byte)(30 + (byte)_BoardId), 147 };
            //    DevicePort.Write(data, 0, data.Length);
            //    try
            //    {
            //        DevicePort.Read(data, 0, 1);
            //        _BoardFirmware = data[0];
            //        Trace.WriteLine("Set Version:" + _BoardFirmware);
            //    }
            //    catch(TimeoutException)
            //    {   _BoardFirmware = 0xFF; }
            //}
        }
        private Byte _BoardFirmware;

        /// <value>
        /// Sets Enable/Disable Trigger Roll
        /// </value>
        public bool RollingEnabled
        {
            get{ return _RollingEnabled;  }
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
                    _RollingEnabled = value;
                }
                else
                {
                    Trace.WriteLine("Board: " + BoardId.ToString() + "No Communication");
                }
            }
        }
        private bool _RollingEnabled;

        /// <value>
        /// Command 120
        /// </value>
        public UInt16 AdcSampleSize             
        {
            get{ return _AdcSampleSize;  }
            set{ 
                byte[] data = new byte[] { (byte)(120 + (byte)_BoardId) };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _AdcSampleSize = value;
            }
        }
        private UInt16 _AdcSampleSize;

        /// <value>
        /// Command 121
        /// </value>
        public UInt16 TriggerPaddingCount       
        {
            get { return _TriggerPaddingCount; }
            set
            {
                byte[] data = new byte[] { 121 };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).Take(3).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _TriggerPaddingCount = value;
            }
        }
        private UInt16 _TriggerPaddingCount;

        /// <value>
        /// Command 122
        /// </value>
        public UInt16 SampleTransmitCount       
        {
            get { return _SampleTransmitCount;  }
            set {
                byte[] data = new byte[] { 122  };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _SampleTransmitCount = value;
            }
        }
        private UInt16 _SampleTransmitCount;

        /// <value>
        /// Command 123
        /// </value>
        public Byte ByteSkipCount               
        {
            get{ return _ByteSkipCount;  }
            set{
                byte[] data = new byte[] { (byte)(123), value };
                //data = data;
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _ByteSkipCount = value;
            }
        }
        private Byte _ByteSkipCount;

        /// <value>
        /// Command 124
        /// </value>
        public Byte Downsample                  
        {
            get{ return _Downsample;  }
            set{
                byte[] data = new byte[] { 124, value };
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _Downsample = value; 
            }
        }
        private Byte _Downsample;

        /// <value>
        /// Command 125
        /// </value>
        public Byte TicksToWait                 
        {
            get{ return _TicksToWait;  }
            set{
                byte[] data = new byte[] { 125, value };
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _TicksToWait = value; 
            }
        }
        private Byte _TicksToWait;

        /// <value>
        /// Command 127
        /// </value>
        public Byte TriggerThreshold
        {
            get { return _TriggerThreshold; }
            set
            {
                byte[] data = new byte[] { (byte)(127), value };
                //data = data;
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _TriggerThreshold = value;
                OnPropertyChanged();
            }
        }
        private Byte _TriggerThreshold;

        /// <value>
        /// Command 128
        /// </value>
        public bool TriggerEdgeType
        {
            get { return _TriggerEdgeType; }
            set {
                byte[] data = new byte[] { 128 };
                data = data.Concat(BitConverter.GetBytes( !value && true )).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _TriggerEdgeType = value;
            }
        }
        private bool _TriggerEdgeType;


        /// <value>
        /// Command 129
        /// </value>
        public UInt16 TriggerTime               
        {
            get{ return _TriggerTime;  }
            set{
                byte[] data = new byte[] { 129 };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _TriggerTime = value; 
            }
        }
        private UInt16 _TriggerTime;

        /// <value>
        /// Command 130
        /// </value>
        public byte TriggerEnable
        {
            get { return _TriggerEnable; }
            set
            {
                byte[] data = new byte[] { 130 , value };
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _TriggerEnable = value;
            }
        }
        private byte _TriggerEnable;


        /// <value>
        /// Command 135
        /// </value>
        public UInt16 SerialWaitMax             
        {
            get{ return this._SerialWaitMax;  }
            set{
                byte[] data = new byte[] { 135 };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                this._SerialWaitMax = value; 
            }
        }
        private UInt16 _SerialWaitMax;

        /// <value>
        /// Command 138
        /// </value>
        public UInt16 LockInShift               
        {
            get{ return _LockInShift;  }
            set{
                byte[] data = new byte[] { 138  };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _LockInShift = value; 
            }
        }
        private UInt16 _LockInShift;

        /// <value>
        /// Command 143
        /// </value>
        public bool HiResEnabled                
        {
            get { return _HiResEnabled; }
            set
            {
                byte[] data = new byte[] { 143 };
                if (DevicePort != null)
                {
                    DevicePort.Write(data, 0, data.Length);
                    TraceMessage(BitConverter.ToString(data, 0));
                    _HiResEnabled = !HiResEnabled;
                }
                else
                {
                    Trace.WriteLine("Board: " + BoardId.ToString() + "No Communication");
                }
            }
        }
        private bool _HiResEnabled = false;

        /// <value>
        /// Command 131
        /// </value>
        public UInt16 AdcWrite
        {
            get { return _AdcWrite; }
            set
            {
                byte[] data = new byte[] { 131  };
                data = data.Concat(BitConverter.GetBytes(value).Reverse()).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _AdcWrite = value;
            }
        }
        private UInt16 _AdcWrite;

        /// <value>
        /// 
        /// </value>
        public UInt64 IoExp
        {
            get { return _IoExp; }
            set
            {
                byte[] data = new byte[] { 136 };
                data = data.Concat(BitConverter.GetBytes(value)).Take(7).ToArray();
                DevicePort.Write(data, 0, data.Length);
                TraceMessage(BitConverter.ToString(data, 0));
                _IoExp = value;
            }
        }
        private UInt64 _IoExp;

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

            //Trace.WriteLine("Get BoardUniqueId: " + BoardUniqueId);

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
