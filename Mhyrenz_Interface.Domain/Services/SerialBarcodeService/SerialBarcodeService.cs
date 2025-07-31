using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mhyrenz_Interface.Domain.Services.SerialBarcodeService
{
    public class SerialBarcodeService : ISerialBarcodeService
    {
        private readonly SerialPort _serialPort;
        private readonly Thread _receiverThread;
        private readonly ManagementEventWatcher _insertWatcher;
        private readonly ManagementEventWatcher _removeWatcher;

        private bool _isReceiverOpen;

        private string _targetPortName;
        public string TargetPortName
        {
            get => _targetPortName;
            set
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    throw new InvalidOperationException("Can't change port while open");
                }

                _targetPortName = value;
                _serialPort.PortName = _targetPortName;
            }
        }

        public event Action OnSerialDisconnected;
        public event Action OnSerialReconnected;
        public event Action<string> OnConnectionError;
        public event Action<string> OnBarcodeReceived;

        public SerialBarcodeService()
        {
            _serialPort = new SerialPort
            {
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                Encoding = Encoding.ASCII,
                NewLine = "\r"
            };
            _receiverThread = new Thread(ReceiverLoop) { Name = "Barcode Service", IsBackground = true };

            string queryInsert = "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity'";
            string queryRemove = "SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity'";

            _insertWatcher = new ManagementEventWatcher(new WqlEventQuery(queryInsert));
            _removeWatcher = new ManagementEventWatcher(new WqlEventQuery(queryRemove));

            _insertWatcher.EventArrived += InsertWatcher_EventArrived;
            _removeWatcher.EventArrived += RemoveWatcher_EventArrived;
        }

        public void Start()
        {
            if (_targetPortName is null)
                throw new InvalidOperationException($"Unable to {nameof(Start)} while {nameof(TargetPortName)} is null.");
            //_serialPort.Open();
            //_receiverThread.Start();
            _isReceiverOpen = true;
            _insertWatcher.Start();
            _removeWatcher.Start();
        }

        public void Stop()
        {
            _insertWatcher.Stop();
            _removeWatcher.Stop();
            Disconnect();
        }

        #region "Events"
        private void RemoveWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (!IsPortAvailable(TargetPortName) && _serialPort.IsOpen)
            {
                Disconnect();
                OnSerialDisconnected?.Invoke();
            }
        }

        private void InsertWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (IsPortAvailable(TargetPortName) && !!!_serialPort.IsOpen)
            {
                TryConnect();
                OnSerialReconnected?.Invoke();
            }
        }

        private void ReceiverLoop()
        {
            while (_isReceiverOpen)
            {
                string line = _serialPort.ReadLine(); // blocks until line received
                OnBarcodeReceived?.Invoke(line);

                Thread.Sleep(1);
            }
        }

        #endregion

        private void TryConnect()
        {
            //if (_serialPort != null && _serialPort.IsOpen)
            //{
            //    OnConnectionError?.Invoke("Serial is not open.");
            //    return;
            //}

            if (IsPortAvailable(TargetPortName))
            {
                try
                {
                    _serialPort.Open();
                    _isReceiverOpen = true;
                }
                catch (Exception ex)
                {
                    OnConnectionError?.Invoke(ex.Message);
                }
            }
        }

        private void Disconnect()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _isReceiverOpen = false;
            }
        }

        private static bool IsPortAvailable(string portName)
        {
            return SerialPort.GetPortNames().Contains(portName);
        }
    }
}
