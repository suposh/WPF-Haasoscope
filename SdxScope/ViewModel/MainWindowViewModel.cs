using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Threading;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot;
using OxyPlot.Annotations;
using System.IO;
using System.Windows;

namespace SdxScope
{

    internal partial class MainWindowViewModel : ViewModelBase
    {

        //Byte[] msg = { 0, 20, 30, 142 };

        public SerialPort DevicePort;

        private Communication _Uart;
        public Communication Uart
        {
            get => _Uart;
            set
            {
                _Uart = value;
                OnPropertyChanged(); // ← this uses CallerMemberName, so no need to pass the string
            }
        }

        private String _ConnectionButton;
        public String ConnectionButton
        {
            get => _ConnectionButton;
            set
            {
                _ConnectionButton = value;
                OnPropertyChanged(); // ← this uses CallerMemberName, so no need to pass the string
            }
        }

        private BoardConfiguration? _boardHandle;
        public BoardConfiguration? BoardHandle
        {
            get => _boardHandle;
            set
            {
                _boardHandle = value;
                OnPropertyChanged(); // ← this uses CallerMemberName, so no need to pass the string
            }
        }

        protected PlotModel mooodle;
        public PlotModel Model
        {
            get { return mooodle; }
            set
            {
                mooodle = value;
                OnPropertyChanged();
            }
        }

        private String[] _AvailableCOMDevices;
        public String[] AvailableCOMDevices
        {
            get { return _AvailableCOMDevices;  }
            set { OnPropertyChanged(); _AvailableCOMDevices = value; }
        }

        private int _SelectedCOMDevice;
        public int SelectedCOMDevice
        {
            get { return _SelectedCOMDevice;  }
            set { OnPropertyChanged(); _SelectedCOMDevice = value; }
        }

        static public String[] GetComPort
        {
            get{
                String[] COMDevicesList = SerialPort.GetPortNames();
                for (int i = 0; i < COMDevicesList.Length; i++)
                    Trace.WriteLine($"{i}: {COMDevicesList[i]}");
                return COMDevicesList;
            }
        }

        public LineSeries Channel_A, Channel_B, Channel_C, Channel_D;

        //public Thread readThread;
        public bool DataStreamStatus = false;
        DispatcherTimer DataFetchTimer;
        const int constantInterval = 50;//milliseconds

        public RelayCommand AddCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand ConnectBoardCommand { get; set; }
        //public RelayCommand DisconnectBoardCommand { get; set; }
        public RelayCommand StartDataLoop { get; set; }
        public RelayCommand GetBoardFirmware { get; set; }
        public RelayCommand DownsampleIncrease { get; set; }
        public RelayCommand DownsampleDecrease { get; set; }
        public RelayCommand DataScaleIncrease { get; set; }
        public RelayCommand DataScaleDecrease { get; set; }
        public RelayCommand ChannelTrigger { get; set; }
        public RelayCommand UpdateSelectedCOMPort { get; set; }
        

        public MainWindowViewModel()
        {
            //Items = new ObservableCollection<Item>();
            SelectedCOMDevice = 0;
            AvailableCOMDevices = GetComPort.OrderBy(o => o).ToArray();
            DevicePort = new SerialPort();
            Uart       = new Communication(ref DevicePort);
            Model      = new PlotModel { };
            ConnectionButton = new String("Connect");

            // Setup Data polling timer
            DataFetchTimer = new();
            DataFetchTimer.Interval = TimeSpan.FromMilliseconds(10);
            DataFetchTimer.Tick += Timer_Tick;

            // Load the image (any Stream or file path)
            var stream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/scope_bg.png"))?.Stream;
            using var ms = new MemoryStream();
            stream.CopyTo(ms);

            // Set background image for plot
            var bgImageAnnotation = new ImageAnnotation
            {
                ImageSource = new OxyImage(ms.ToArray()),
                X = new PlotLength(0, PlotLengthUnit.RelativeToPlotArea),
                Y = new PlotLength(1.0, PlotLengthUnit.RelativeToPlotArea),
                Width = new PlotLength(1.0, PlotLengthUnit.RelativeToPlotArea),
                Height = new PlotLength(1.0, PlotLengthUnit.RelativeToPlotArea),
                Opacity = 1,
                Layer = AnnotationLayer.BelowSeries,
                Interpolate = true,
                HorizontalAlignment = OxyPlot.HorizontalAlignment.Left,
                VerticalAlignment = OxyPlot.VerticalAlignment.Bottom
            };
            //Model.Annotations.Insert(0, bgImageAnnotation);
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left});
            
            Channel_A = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                MarkerSize = 2,
                Color = OxyPlot.OxyColors.Red,
                //Smooth = true
            };
            Channel_A.InterpolationAlgorithm = InterpolationAlgorithms.UniformCatmullRomSpline;
            
            Channel_B = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                MarkerSize = 2,
                Color = OxyPlot.OxyColors.Green,
                //Smooth = true
            };
            Channel_B.InterpolationAlgorithm = InterpolationAlgorithms.UniformCatmullRomSpline;

            Channel_C = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                MarkerSize = 2,
                Color = OxyPlot.OxyColors.Blue,
                //Smooth = true
            };
            Channel_C.InterpolationAlgorithm = InterpolationAlgorithms.UniformCatmullRomSpline;

            Channel_D = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                MarkerSize = 2,
                Color = OxyPlot.OxyColors.Coral,
                //Smooth = true
            };
            Channel_D.InterpolationAlgorithm = InterpolationAlgorithms.UniformCatmullRomSpline;

            this.Model.Series.Add(Channel_A);
            this.Model.Series.Add(Channel_B);
            this.Model.Series.Add(Channel_C);
            this.Model.Series.Add(Channel_D);
            this.OnPropertyChanged("Model");

            ConnectBoardCommand =       new RelayCommand(
                param => {
                    if (param is bool isChecked)
                    {
                        if (isChecked)
                        {
                            Uart.ConnectBoard(AvailableCOMDevices[SelectedCOMDevice]);
                            if (Uart.ConnectionStatus)
                            {
                                ConnectionButton = "Disconnect";
                                BoardHandle = new BoardConfiguration(0, ref DevicePort);
                                BoardHandle.Initializer();
                            }
                            else
                            {
                                ConnectionButton = "Connect";
                                Uart.ConnectionStatus = false;
                                Debug.WriteLine("Failed to Connect.");
                            }
                        }
                        else
                        { Uart.DisconnectBoard(); ConnectionButton = "Connect"; }
                    }
                    
                },
                canExecute => (true)
            );

            StartDataLoop =             new RelayCommand(
                execute => {
                    //this.Model = new PlotModel { Title = "Voltage Graph", Subtitle = "using OxyPlot" };
                    if (DataStreamStatus is false)
                    {
                    //readThread = new Thread(Read);
                    //readThread.Start();
                        DataStreamStatus = true;
                        DataFetchTimer.Start();
                    }
                    else
                    {
                        if (DataFetchTimer.IsEnabled)
                        {
                            DataStreamStatus = false;
                            DataFetchTimer.Stop();
                        //readThread.Join();
                    }
                    }
                },
                canExecute => (Uart.ConnectionStatus is true)
            );

            GetBoardFirmware =          new RelayCommand(
                execute => { BoardHandle.BoardFirmware.ToString(); }, 
                canExecute => (Uart.ConnectionStatus is true)
            );

            DownsampleIncrease =        new RelayCommand(
                execute => { BoardHandle.Downsample += 1; },
                canExecute => (Uart.ConnectionStatus is true)
            );

            DownsampleDecrease =        new RelayCommand(
                execute => { BoardHandle.Downsample -= 1; },
                canExecute => (Uart.ConnectionStatus is true)
            );
            
            DataScaleIncrease =         new RelayCommand(
                execute => { },
                canExecute => (Uart.ConnectionStatus is true)
            );

            DataScaleDecrease =         new RelayCommand(
                execute => { },
                canExecute => (Uart.ConnectionStatus is true)
            );

            ChannelTrigger =            new RelayCommand(
                execute => { BoardHandle.TriggerEnable = 0; },
                canExecute => (Uart.ConnectionStatus is true)
            );

            UpdateSelectedCOMPort =     new RelayCommand(
                execute => { AvailableCOMDevices = GetComPort;  },
                canExecute => (true)
            );
        }

        public void Read()
        {
            byte[] ReadBuffer = new byte[2048];
            int[] dataX = Enumerable.Range(0, 512).ToArray();
            int CurrentReadSize = 0;
            while (DataStreamStatus)
            {
                try
                {
                    DevicePort.Write(new byte[] { 100 }, 0, 1);
                    DevicePort.Write(new byte[] {  10 }, 0, 1);
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"Write Failed" + $"{e.Message}");
                }

                while (CurrentReadSize < 2048)
                {
                    try
                    {
                        CurrentReadSize += DevicePort.Read(ReadBuffer, CurrentReadSize, 2048 - CurrentReadSize);
                        Delay(10);

                        //PlotWindow.Plot.Clear();
                        //PlotWindow.Plot.Add.Scatter(dataX, ReadBuffer);
                        //PlotWindow.Refresh();
                        //Trace.WriteLine(BitConverter.ToString(ReadBuffer, 0));
                        //Trace.WriteLine($"readCount: {CurrentReadSize}");
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"Read Failed" + $"{e.Message}");
                    }
                }
                //Trace.WriteLine($"Read Complete: {CurrentReadSize}");

                Channel_A.Points.Clear();
                Channel_B.Points.Clear();
                Channel_C.Points.Clear();
                Channel_D.Points.Clear();
                Model.InvalidatePlot(true);
                foreach (var data in dataX)
                {
                    Channel_A.Points.Add(new DataPoint(data, ReadBuffer[0]));
                    Channel_B.Points.Add(new DataPoint(data, ReadBuffer[0 +  512 - 1]));
                    Channel_C.Points.Add(new DataPoint(data, ReadBuffer[0 + 1024 - 1]));
                    Channel_D.Points.Add(new DataPoint(data, ReadBuffer[0 + 1536 - 1]));
                }
                
                //this.Model = ab;
            }

            CurrentReadSize = 0;

        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            byte[] ReadBuffer = new byte[2048];
            int CurrentReadSize = 0;
            var Channel_A = (LineSeries)this.Model.Series[0];
            var Channel_B = (LineSeries)this.Model.Series[1];
            var Channel_C = (LineSeries)this.Model.Series[2];
            var Channel_D = (LineSeries)this.Model.Series[3];

            if (DataStreamStatus)
            {
                try
                {
                    DevicePort.Write(new byte[] { 100 }, 0, 1);
                    DevicePort.Write(new byte[] { 10 }, 0, 1);
                }
                catch (Exception er)
                {
                    Trace.WriteLine($"Read Failed: {er.Message}");
                    return;
                }

                while (CurrentReadSize < 2048)
                {
                    try
                    {
                        CurrentReadSize += DevicePort.Read(ReadBuffer, CurrentReadSize, 2048 - CurrentReadSize);
                    }
                    catch (Exception er)
                    {
                        Trace.WriteLine($"Read Failed: {er.Message}");
                        return;
                    }
                }

                lock (Model.SyncRoot)
                {
                    List<DataPoint> aPoints = new(512);
                    List<DataPoint> bPoints = new(512);
                    List<DataPoint> cPoints = new(512);
                    List<DataPoint> dPoints = new(512);

                    for (int x = 0; x < 512; x++)
                    {
                        aPoints.Add(new DataPoint(x, ReadBuffer[x]));
                        bPoints.Add(new DataPoint(x, ReadBuffer[512 + x]));
                        cPoints.Add(new DataPoint(x, ReadBuffer[1024 + x]));
                        dPoints.Add(new DataPoint(x, ReadBuffer[1536 + x]));
                    }

                    Channel_A.Points.Clear();
                    Channel_B.Points.Clear();
                    Channel_C.Points.Clear();
                    Channel_D.Points.Clear();

                    Channel_A.Points.AddRange(aPoints);
                    Channel_B.Points.AddRange(bPoints);
                    Channel_C.Points.AddRange(cPoints);
                    Channel_D.Points.AddRange(dPoints);
                }

                Model.InvalidatePlot(true);
            }

            // Adjust timer interval
            var now = DateTime.Now;
            var nowMilliseconds = (int)now.TimeOfDay.TotalMilliseconds;
            var timerInterval = constantInterval - nowMilliseconds % constantInterval + 5; //5: sometimes the tick comes few millisecs early
            //Trace.WriteLine($"Interval: {timerInterval}");
            DataFetchTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
        }
    }

}
