using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Threading;
using OxyPlot.Axes;
using OxyPlot;

namespace SdxScope
{
    using OxyPlot.Series;
    using System.Threading.Channels;
    using WinRT;

    internal partial class MainWindowViewModel : ViewModelBase
    {

        Byte[] msg = { 0, 20, 30, 142 };
        public Communication Uart;
        public SerialPort DevicePort;
        public BoardConfiguration BoardHandle;
        //public PlotModel PlotContainer { get; set; }
        public LineSeries Channel_A, Channel_B, Channel_C, Channel_D;

        protected PlotModel mooodle;
        public PlotModel Model
        {
            get { return mooodle; }
            set {
                    mooodle = value;
                    OnPropertyChanged();
            }
        }


        //public Thread readThread;
        public bool DataStreamStatus = false;
        DispatcherTimer DataFetchTimer;
        const int constantInterval = 50;//milliseconds

        private string fw;
        public string Fw
        {
            get { return fw; }
            set
            {
                fw = "V" + value;
                OnPropertyChanged();
            }
        }

        public RelayCommand AddCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand ConnectBoardCommand { get; set; }
        public RelayCommand DisconnectBoardCommand { get; set; }
        public RelayCommand TestMsg_1 { get; set; }
        public RelayCommand GetBoardFirmware { get; set; }
        public RelayCommand DownsampleIncrease { get; set; }
        public RelayCommand DownsampleDecrease { get; set; }
        public RelayCommand DataScaleIncrease { get; set; }
        public RelayCommand DataScaleDecrease { get; set; }
        public RelayCommand ChannelTrigger { get; set; }
        //public RelayCommand TriggerThreshold_ValueChanged { get; set; }
        

        public MainWindowViewModel()
        {
            //Items = new ObservableCollection<Item>();
            DevicePort = new SerialPort();
            Uart = new Communication(ref DevicePort);
            Model = new PlotModel { Title = "Volto", Subtitle = "using OxyPlot" };
            Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left});
            Channel_A = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                MarkerSize = 2,
                Color = OxyPlot.OxyColors.Red,
                //Smooth = true
            };
            //Channel_A.InterpolationAlgorithm = InterpolationAlgorithms.UniformCatmullRomSpline;
            
            Channel_B = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                MarkerSize = 2,
                Color = OxyPlot.OxyColors.Green,
                //Smooth = true
            };
            Channel_B.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;

            Channel_C = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                MarkerSize = 2,
                Color = OxyPlot.OxyColors.Blue,
                //Smooth = true
            };
            Channel_C.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;

            Channel_D = new LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                MarkerSize = 2,
                Color = OxyPlot.OxyColors.Coral,
                //Smooth = true
            };
            Channel_D.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;

            this.Model.Series.Add(Channel_A);
            this.Model.Series.Add(Channel_B);
            this.Model.Series.Add(Channel_C);
            this.Model.Series.Add(Channel_D);

            this.OnPropertyChanged("Model");
            DataFetchTimer = new();
            DataFetchTimer.Interval = TimeSpan.FromMilliseconds(100);
            DataFetchTimer.Tick += Timer_Tick;

            DisconnectBoardCommand = new RelayCommand(
                execute => { Uart.DisconnectBoard(); },
                canExecute => ( Communication.ConnectionStatus is true )
            );

            ConnectBoardCommand = new RelayCommand(
                execute => {

                    Uart.ConnectBoard();
                    BoardHandle = new BoardConfiguration(0, ref DevicePort);
                    BoardHandle.Initializer();
                },
                canExecute => (Communication.ConnectionStatus is false)
            );

            TestMsg_1 = new RelayCommand(
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
                canExecute => (Communication.ConnectionStatus is true)
            );

            GetBoardFirmware = new RelayCommand(
                execute => { Fw = BoardHandle.BoardFirmware.ToString(); }, 
                canExecute => (Communication.ConnectionStatus is true)
            );

            DownsampleIncrease = new RelayCommand(
                execute => { BoardHandle.Downsample += 1; },
                canExecute => (Communication.ConnectionStatus is true)
            );

            DownsampleDecrease = new RelayCommand(
                execute => { BoardHandle.Downsample -= 1; },
                canExecute => (Communication.ConnectionStatus is true)
            );
            
            DataScaleIncrease = new RelayCommand(
                execute => { },
                canExecute => (Communication.ConnectionStatus is true)
            );

            DataScaleDecrease = new RelayCommand(
                execute => { },
                canExecute => (Communication.ConnectionStatus is true)
            );

            ChannelTrigger = new RelayCommand(
                execute => { BoardHandle.TriggerEnable = 0; },
                canExecute => (Communication.ConnectionStatus is true)
            );

        }

        private void SetTimebase(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            Trace.WriteLine(e);
        }
        public void Read()
        {
            byte[] inputBuf2048 = new byte[2048];
            int[] dataX = Enumerable.Range(0, 512).ToArray();
            int totBytesRead = 0;
            while (DataStreamStatus)
            {
                DevicePort.Write(new byte[] { 100 }, 0, 1);
                DevicePort.Write(new byte[] {  10 }, 0, 1);
                while (totBytesRead < 2048)
                {
                    try
                    {
                        totBytesRead += DevicePort.Read(inputBuf2048, totBytesRead, 2048 - totBytesRead);
                        Delay(10);

                        //PlotWindow.Plot.Clear();
                        //PlotWindow.Plot.Add.Scatter(dataX, inputBuf2048);
                        //PlotWindow.Refresh();
                        //Trace.WriteLine(BitConverter.ToString(inputBuf2048, 0));
                        //Trace.WriteLine($"readCount: {totBytesRead}");
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine($"Read Failed" + $"{e.Message}");
                    }
                }
                //Trace.WriteLine($"Read Complete: {totBytesRead}");

                Channel_A.Points.Clear();
                Channel_B.Points.Clear();
                Channel_C.Points.Clear();
                Channel_D.Points.Clear();
                Model.InvalidatePlot(true);
                foreach (var data in dataX)
                {
                    Channel_A.Points.Add(new DataPoint(data, inputBuf2048[0]));
                    Channel_B.Points.Add(new DataPoint(data, inputBuf2048[0 +  512 - 1]));
                    Channel_C.Points.Add(new DataPoint(data, inputBuf2048[0 + 1024 - 1]));
                    Channel_D.Points.Add(new DataPoint(data, inputBuf2048[0 + 1536 - 1]));
                }
                
                //this.Model = ab;
            }

            totBytesRead = 0;

        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            byte[] inputBuf2048 = new byte[2048];
            int[] dataX = Enumerable.Range(0, 512).ToArray();
            int totBytesRead = 0;
            var Channel_A = (LineSeries)this.Model.Series[0];
            var Channel_B = (LineSeries)this.Model.Series[1];
            var Channel_C = (LineSeries)this.Model.Series[2];
            var Channel_D = (LineSeries)this.Model.Series[3];
            if (DataStreamStatus)
            {
                //DevicePort.Write(new byte[] { 101 }, 0, 1);
                //BoardHandle.RollingEnabled = true;
                DevicePort.Write(new byte[] { 100 }, 0, 1);
                DevicePort.Write(new byte[] {  10 }, 0, 1);
                while (totBytesRead < 2048)
                {
                    try
                    {
                        totBytesRead += DevicePort.Read(inputBuf2048, totBytesRead, 2048 - totBytesRead);
                        Delay(10);
                        //Trace.WriteLine(BitConverter.ToString(inputBuf2048, 0));
                        //Trace.WriteLine($"readCount: {totBytesRead}");
                    }
                    catch (Exception er)
                    {
                        Trace.WriteLine($"Read Failed" + $"{er.Message}");
                        return;
                    }
                }
                //Trace.WriteLine($"Read Complete: {totBytesRead}");

                lock (Model.SyncRoot)
                {
                    
                    Channel_A.Points.Clear();
                    Channel_B.Points.Clear();
                    Channel_C.Points.Clear();
                    Channel_D.Points.Clear();

                    foreach (var x_idx in dataX)
                    {
                        Channel_A.Points.Add(new DataPoint(x_idx, inputBuf2048[0 + x_idx]));
                        Channel_B.Points.Add(new DataPoint(x_idx, inputBuf2048[0 +  512 - 1 + x_idx]));
                        Channel_C.Points.Add(new DataPoint(x_idx, inputBuf2048[0 + 1024 - 1 + x_idx]));
                        Channel_D.Points.Add(new DataPoint(x_idx, inputBuf2048[0 + 1536 - 1 + x_idx]));
                        ////this.RaisePropertyChanged("PlotModel");
                    }
                }
                Model.InvalidatePlot(true);
            }

            totBytesRead = 0;
            var now = DateTime.Now;
            var nowMilliseconds = (int)now.TimeOfDay.TotalMilliseconds;
            var timerInterval = constantInterval - nowMilliseconds % constantInterval + 5;//5: sometimes the tick comes few millisecs early
            //Trace.WriteLine($"Interval: {timerInterval}");
            DataFetchTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
        }
    }


}
