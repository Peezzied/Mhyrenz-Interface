using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mhyrenz_Interface.Domain.Services.SerialBarcodeService
{
    public class SerialBarcodeService : ISerialBarcodeService
    {
        private SerialPort _serialPort;
        private readonly Thread _receiverThread;

        public SerialBarcodeService()
        {
            _serialPort = new SerialPort
            {
                PortName = "COM4", 
                BaudRate = 9600, 
                Parity = Parity.None, 
                DataBits = 8, 
                StopBits = StopBits.One, 
                Handshake = Handshake.None, 
                Encoding = Encoding.ASCII,
                NewLine = "\r"
            };
            _serialPort.Open();
            _receiverThread = new Thread(ReceiveLoop) { Name = "Barcode Service", IsBackground = true };
            _receiverThread.Start();
        }

        private void ReceiveLoop()
        {
            while (true)
            {
                try
                {
                    string line = _serialPort.ReadLine(); // blocks until line received
                    Received(line);
                }
                catch (Exception ex)
                {
                    // Optionally log and attempt to reconnect here
                    Debug.WriteLine("Serial read error: " + ex.Message);
                    Thread.Sleep(1000); // prevent crash loop
                }
            }
        }

        private void Received(string receive)
        {
        }
    }
}
