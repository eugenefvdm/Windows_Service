using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Net;
using System.IO.Ports;

public enum ServiceState
{
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007,
}

[StructLayout(LayoutKind.Sequential)]
public struct ServiceStatus
{
    public long dwServiceType;
    public ServiceState dwCurrentState;
    public long dwControlsAccepted;
    public long dwWin32ExitCode;
    public long dwServiceSpecificExitCode;
    public long dwCheckPoint;
    public long dwWaitHint;
};

namespace WindowsService2
{
    public partial class Service1 : ServiceBase
    {
        public int eventID { get; private set; }

        private SerialPort _serialPort = new SerialPort();

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public Service1(string[] args)
        {
            InitializeComponent();

            string eventSourceName = "MySource";
            string logName = "MyNewLog";

            if (args.Count() > 0) {
                eventSourceName = args[0];
            }

            if (args.Count() > 1) {
                logName = args[1];
            }

            eventLog1 = new System.Diagnostics.EventLog();

            if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
                {
                    System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
            }

            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;

        }

        protected override void OnStart(string[] args)
        {

            // Update the service state to Start Pending.
            ///ServiceStatus serviceStatus = new ServiceStatus();
            ///serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            ///serviceStatus.dwWaitHint = 100000;
            ///SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            eventLog1.WriteEntry("In OnStart");

            // Set up a timer to trigger every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            //timer.Interval = 60000; // 60 seconds
            timer.Interval = 30000; // 60 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();


            // Update the service state to Running.
            ///serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            ///SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }

        protected override void OnStop()
        {

            // Update the service state to Start Pending.
            ///ServiceStatus serviceStatus = new ServiceStatus();
            ///serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            ///serviceStatus.dwWaitHint = 100000;
            ///SetServiceStatus(this.ServiceHandle, ref serviceStatus);


            eventLog1.WriteEntry("In onStop.");

            // Update the service state to Running.
            ///serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            ///SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            //eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, eventID);

            string valueOriginal = "Retrieving message...";

            using (WebClient webClient = new System.Net.WebClient())
            {

                WebClient n = new WebClient();
                var json = n.DownloadString("http://wh6.snowball.co.za/laravel/public/led");
                valueOriginal = Convert.ToString(json);

            }

            if (valueOriginal !=  "\"blank\"")
            {
                showMessage(valueOriginal);
            }

            //else
            //{
                //var blank = "#1a1W4013                                                                ";
                //showMessage(blank, false);
            //}
           
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue.");
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }

        private void showMessage(string message, bool log = true)
        {
            // Write the entry to the event log

            if (log)
            {
                eventLog1.WriteEntry("LED Display: " + message, EventLogEntryType.Information, eventID++);
            }

            // Write the entry to the COM port

            _serialPort.PortName = "COM1";
            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.Two;
            _serialPort.Handshake = Handshake.None;

            _serialPort.Open();

            if (_serialPort.IsOpen)
            {
                //eventLog1.WriteEntry("Port opened", EventLogEntryType.Information, eventID++);
            }
            else
            {
                //eventLog1.WriteEntry("Port did not open!", EventLogEntryType.Information, eventID++);
            }

            _serialPort.Write(message);

            _serialPort.Close();

            //eventLog1.WriteEntry("The message has been sent", EventLogEntryType.Information, eventID);
        }

    }
}
