using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Runtime.InteropServices;



namespace OpenYA
{
    
    public partial class MainWindow : Window
    {
        //DLLs for Sleep/Hibernate
        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        DispatcherTimer alarm = new DispatcherTimer(); //Main alarm
        TimeSpan untilStart = new TimeSpan();          //Used for time until start label
        TimeSpan untilEnd = new TimeSpan();            //Used for time until close label
        DateTime closeTimePickerValue = new DateTime();//TextBox that acts as a TimePicker for selecting closing time
        DateTime openTimePickerValue = new DateTime(); //TextBox that acts as a TimePicker for selecting opening time

        string filename;        
        string processString;

        bool started = false;
        bool ended = false;
        
        public MainWindow()
        {
            InitializeComponent();
 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetLabels();//Sets TimePickers to DateTime.Now

            if (statusLabel.Foreground == Brushes.Red)
            {
                startButton.IsEnabled = false;
                stopButton.IsEnabled = false;
            }
    
            //Declaring alarm
            alarm.Interval = new TimeSpan(0, 0, 1);
            alarm.Tick += Alarm_Tick;
        }
        
        private void Alarm_Tick(object sender, EventArgs e)
        {
                CheckForAlarm();//Each tick checks for alarm
                AvoidNegativeLabels();//Since there are two timers, method checks 
                //if one or both of them has ended and doesn't let them go below 0
        }
        
        private void CheckForAlarm()
        {
            CalculateTimeLeft();// Calculates time left for TimeSpans

            HandleTimeError();//Solves error that occurs when open time is > close time

            CheckIfStart();//Checks if it's time to start the program, if it is - executes starting code

            CheckIfStop(); //Checks if it's time to stop the program, if it is - executes stopping code
        }

        private void CheckIfStart()
        {
            if (openTimePickerValue.Hour == DateTime.Now.Hour && openTimePickerValue.Minute == DateTime.Now.Minute && openTimePickerValue.Second == DateTime.Now.Second)
            {
                started = true;

                Process.Start(filename);

                timeUntilStart.Content = "Time until start left: 00:00:00";
               
                Task.Factory.StartNew(() =>
                {
                    MessageBox.Show("Started at: " + DateTime.Now, "OpenYA", MessageBoxButton.OK, MessageBoxImage.Asterisk);

                });

            }
        }

        private void CheckIfStop()
        {
            if (closeTimePickerValue.Hour == DateTime.Now.Hour && closeTimePickerValue.Minute == DateTime.Now.Minute && closeTimePickerValue.Second == DateTime.Now.Second)
            {
                foreach (var process in Process.GetProcessesByName(processString))
                {
                    process.Kill();
                }

                StopButton_Click(null, null);   

                Task.Factory.StartNew(() =>
                {
                    MessageBox.Show("Ended on: " + DateTime.Now, "OpenYA", MessageBoxButton.OK, MessageBoxImage.Asterisk);
               
                  });
                
                CheckRadioButtonSelection();

            }
        }

        private void HandleTimeError()
        {
            try
            {
                if (closeTimePickerValue < DateTime.Now && openTimePickerValue > DateTime.Now)
                {
                    untilEnd = closeTimePickerValue - DateTime.Now;
                    untilEnd += TimeSpan.FromDays(1);
                    untilStart = openTimePickerValue - DateTime.Now;
                   
                }
                else if (openTimePickerValue < DateTime.Now && closeTimePickerValue > DateTime.Now)
                {
                    untilEnd = closeTimePickerValue - DateTime.Now;
                    untilStart = openTimePickerValue - DateTime.Now;
                    untilStart += TimeSpan.FromDays(1);
                }
                else if (openTimePickerValue < DateTime.Now && closeTimePickerValue < DateTime.Now)
                {
                    untilEnd = closeTimePickerValue - DateTime.Now;
                    untilEnd += TimeSpan.FromDays(1);
                    untilStart = openTimePickerValue - DateTime.Now;
                    untilStart += TimeSpan.FromDays(1);
                }
                else
                {
                    untilEnd = closeTimePickerValue - DateTime.Now;
                    
                    untilStart = openTimePickerValue - DateTime.Now;
                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void CheckRadioButtonSelection()
        {
            if (shutDown.IsChecked == true)
            {
                Process.Start(new ProcessStartInfo("shutdown", "/s /t 0") { CreateNoWindow = true, UseShellExecute = false });
            }

            else if (sleep.IsChecked == true)
            {
                SetSuspendState(false, true, true);
            }
        }

        private void SetLabels()
        {
            openTimePicker.Text = DateTime.Now.ToString(@"HH\:mm\:ss");
            closeTimePicker.Text = DateTime.Now.ToString(@"HH\:mm\:ss");
        }

        private void AvoidNegativeLabels()
        {
            if (started == true)
            {
                timeUntilStart.Content = "Time until start left: 00:00:00";
            }
            else
            {
                timeUntilStart.Content = "Time until start left: " + untilStart.ToString(@"hh\:mm\:ss");
            }

            if (ended == true)
            {
                timeUntilEnd.Content = "Time until end left: 00:00:00";
            }
            else
            {
                timeUntilEnd.Content = "Time until end left: " + untilEnd.ToString(@"hh\:mm\:ss");
            }
        }

        private void CalculateTimeLeft()
        {
            untilStart = openTimePickerValue - DateTime.Now;
            untilEnd = closeTimePickerValue - DateTime.Now;
        }

        private void ExtractTimePickerData()
        {
            openTimePickerValue = DateTime.ParseExact(openTimePicker.Text, @"H\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture);
            closeTimePickerValue = DateTime.ParseExact(closeTimePicker.Text, @"H\:mm\:ss", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void OpenAndCloseLabelsStart()
        {
            openLabel.Content = "Opening time";
            openLabel.Foreground = Brushes.Green;
            closeLabel.Content = "Closing time";
            closeLabel.Foreground = Brushes.Red;
        }

        private void OpenAndCloseLabelsStop()
        {
            openLabel.Content = "Select opening time";
            openLabel.Foreground = Brushes.White;
            closeLabel.Content = "Select closing time";
            closeLabel.Foreground = Brushes.White;
        }

        private void StartBooleans()
        {
            openTimePicker.IsEnabled = false;
            closeTimePicker.IsEnabled = false;
            startButton.IsEnabled = false;
            stopButton.IsEnabled = true;
            fileSelect.IsEnabled = false;
            shutDown.IsEnabled = false;
            sleep.IsEnabled = false;
            nothing.IsEnabled = false;
            ended = false;
            started = false;
        }

        private void StopBooleans()
        {
            startButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            fileSelect.IsEnabled = true;
            shutDown.IsEnabled = true;
            sleep.IsEnabled = true;
            nothing.IsEnabled = true;
            openTimePicker.IsEnabled = true;         
            closeTimePicker.IsEnabled = true;
            started = true;
            ended = true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ExtractTimePickerData(); //Gets data from TimePickers

            OpenAndCloseLabelsStart(); //Cuztomizes openLabel and closeLabel text and colors

            StartBooleans(); //Sets bools

            alarm.Start(); 

            CheckForAlarm();

            timeUntilStart.Content = "Time until start left: " + untilStart.ToString(@"hh\:mm\:ss");
            timeUntilEnd.Content = "Time until end left: " + untilEnd.ToString(@"hh\:mm\:ss");
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            alarm.Stop();
            StopBooleans();
            OpenAndCloseLabelsStop();
            timeUntilStart.Content = "Time until start left: 00:00:00";
            timeUntilEnd.Content = "Time until end left: 00:00:00";
        }

        private void FileSelect_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".exe";
            dlg.Filter = "Executable files (*.exe)|*.exe";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
               filename = dlg.FileName;

                processString = filename.Substring(filename.LastIndexOf("\\") + 1);
                processString = processString.Substring(0, processString.IndexOf("."));

                statusLabel.Content = "File: " + processString;
                statusLabel.Foreground = Brushes.Green;

                startButton.IsEnabled = true;
                
            }
        }

        private void Dragger_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

    }
}
