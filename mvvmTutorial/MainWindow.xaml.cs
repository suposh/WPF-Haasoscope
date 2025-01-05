using System.Windows;
using System.IO.Ports;

namespace SdxScope
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            MainWindowViewModel vm = new MainWindowViewModel(WpfPlot1);
            ScottPlot.Image bgImage = new("I://WinUI Haasoscope/mvvmTutorial/static_image.jpg");
            WpfPlot1.Plot.DataBackground.Image = bgImage;
            DataContext = vm;
        }
    }
}