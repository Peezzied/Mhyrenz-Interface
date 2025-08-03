using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mhyrenz_Interface.Domain.Services.SerialBarcodeService
{
    public class SerialBarcodeService : ISerialBarcodeService, IDisposable
    {
        private SerialPort _serialPort;
        private CancellationTokenSource _cts;
        private Task _receiverTask;
        private string _targetPortName;
        private readonly object _syncLock = new object();

        private readonly ManagementEventWatcher _insertWatcher;

        public event Action OnSerialDisconnected;
        public event Action OnSerialReconnected;
        public event Action<string> OnConnectionError;
        public event Action<string> OnBarcodeReceived;

        public SerialBarcodeService()
        {
            CreateSerialPort();

            string queryInsert = "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity'";

            _insertWatcher = new ManagementEventWatcher(new WqlEventQuery(queryInsert));

            _insertWatcher.EventArrived += InsertWatcher_EventArrived;
        }

        private void CreateSerialPort()
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
        }

        public void Start(string port)
        {
            lock (_syncLock)
            {
                TryConnect(port);

                _cts = new CancellationTokenSource();
                _receiverTask = Task.Run(() => ReceiverLoop(_cts.Token), _cts.Token);

                _insertWatcher.Start();
            }
        }

        public void Stop()
        {
            lock (_syncLock)
            {
                _insertWatcher.Stop();

                _cts?.Cancel();
                try { _receiverTask?.Wait(); } catch { }

                Disconnect();

                _cts?.Dispose();
                _cts = null;
                _receiverTask = null;
            }
        }

        private async Task ReceiverLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_serialPort?.IsOpen == true)
                {
                    try
                    {
                        string line = await Task.Run(() => _serialPort.ReadLine(), token);
                        OnBarcodeReceived?.Invoke(line);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        Disconnect();
                        OnSerialDisconnected?.Invoke();
                    }
                    //catch (Exception ex)
                    //{
                    //    OnConnectionError?.Invoke($"Read error: {ex.Message}");
                    //    Disconnect();
                    //}
                }

                await Task.Delay(10, token); // small delay
            }
        }

        private void InsertWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            lock (_syncLock)
            {
                if (IsPortAvailable(_targetPortName) && !(_serialPort != null && !_serialPort.IsOpen))
                {
                    TryConnect(_targetPortName);
                    OnSerialReconnected?.Invoke();
                }
            }
        }

        private void TryConnect(string port)
        {
            try
            {
                if (_serialPort == null)
                    CreateSerialPort();

                if (!_serialPort.IsOpen && IsPortAvailable(port))
                {
                    _targetPortName = port;
                    _serialPort.PortName = _targetPortName;
                    _serialPort.Open();
                }
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke($"Connect error: {ex.Message}");
            }
        }

        private void Disconnect()
        {
            try
            {
                if (_serialPort?.IsOpen == true)
                    _serialPort.Close();
            }
            catch { }

            try
            {
                _serialPort?.Dispose();
            }
            catch { }

            _serialPort = null; // will be re-created by TryConnect
        }

        private static bool IsPortAvailable(string portName)
        {
            return SerialPort.GetPortNames().Contains(portName);
        }

        public void Dispose()
        {
            Stop();
            _insertWatcher?.Dispose();
        }
    }

}
