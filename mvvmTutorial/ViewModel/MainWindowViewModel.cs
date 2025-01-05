using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace SdxScope
{
    internal class MainWindowViewModel : ViewModelBase
    {

        Byte[] msg = { 0, 20, 30, 142 };
        public Communication Uart;
        public SerialPort DevicePort;
        public BoardConfiguration BoardHandle;
        ScottPlot.WPF.WpfPlot PlotWindow { get; set; }

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

        //DevicePort.Write(msg, 0, 6);

        public MainWindowViewModel(ScottPlot.WPF.WpfPlot plotWindow)
        {
            //Items = new ObservableCollection<Item>();
            DevicePort = new SerialPort();
            Uart = new Communication(ref DevicePort);
            PlotWindow = plotWindow;

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
                    PlotWindow.Plot.Clear();
                    int readCount = 0;
                    //BoardHandle.RollingEnabled = true;
                    DevicePort.Write(new byte[] { 100 }, 0, 1);
                    DevicePort.Write(new byte[] { 10  }, 0, 1);
                    Delay(50);
                    byte[] inputBuf2048 = new byte[2048];
                    int totBytesRead = 0;
                    try
                    {
                        totBytesRead = DevicePort.Read(inputBuf2048, 0, 2048);
                        //while (totBytesRead < inputBuf2048.Length)
                        //{
                        //    readCount = DevicePort.Read(inputBuf2048, totBytesRead, inputBuf2048.Length - totBytesRead);
                        //    totBytesRead += readCount;
                        //    //Delay(2);
                        //}
                        //Uart.readThread.Start();
                        Trace.WriteLine($"readCount: {totBytesRead}");
                    }
                    catch (Exception e)
                    {
                        TraceMessage($"Read Failed - " + $"{e.Message}");
                    }

                    int[] dataX = Enumerable.Range(0, inputBuf2048.Length).ToArray();
                    //byte[] dataY = { 1, 4, 9, 16, 25 };
                    PlotWindow.Plot.Add.Scatter(dataX, inputBuf2048);
                    PlotWindow.Refresh();
                    Trace.WriteLine(BitConverter.ToString(inputBuf2048, 0));
                },
                canExecute => (Communication.ConnectionStatus is true)
            );

            GetBoardFirmware = new RelayCommand(
                execute => { Fw = BoardHandle.BoardFirmware.ToString(); }, 
                canExecute => (Communication.ConnectionStatus is true)
            );
        }
    }
}
