using System;

namespace Mhyrenz_Interface.Domain.Services.SerialBarcodeService
{
    public interface ISerialBarcodeService
    {
        event Action OnSerialDisconnected;
        event Action OnSerialConnected;
        event Action<string> OnBarcodeReceived;
        event Action<string> OnConnectionError;

        void Start(string port);
        void Stop();
    }
}