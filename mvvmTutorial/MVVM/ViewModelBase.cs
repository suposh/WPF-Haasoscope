using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SdxScope
{
    internal class ViewModelBase: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Trace.WriteLine("OnPropertyChanged: " + propertyName);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void TraceMessage(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            Trace.WriteLine(memberName.PadRight(25,'_') + ": " + message);
            //Trace.WriteLine("message: " + message);
            //Trace.WriteLine("source file path: " + sourceFilePath);
            //Trace.WriteLine("source line number: " + sourceLineNumber);
        }

        public async void Delay(int milliseconds)
        {
            await DelayAsync(milliseconds); // 2-second delay
        }

        private async Task DelayAsync(int milliseconds)
        {
            // Perform actions before delay
            //MessageBox.Show("Before delay");

            // Introduce a delay
            await Task.Delay(milliseconds);

            // Perform actions after delay
            //MessageBox.Show("After delay");
        }
    }
}
