using System;

namespace Mhyrenz_Interface.Domain.Services.SerialBarcodeService
{
    public interface ISerialBarcodeService
    {
        string TargetPortName { get; set; }

        event Action OnSerialDisconnected;
        event Action OnSerialReconnected;
        event Action<string> OnBarcodeReceived;
        event Action<string> OnConnectionError;

        void Start();
        void Stop();
    }
}