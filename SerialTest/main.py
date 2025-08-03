import serial
import time

# The COM port connected to your application (scanner input)
PORT = 'COM1'   # Change to your virtual scanner port
BAUD = 9600

def send_barcode_data(port_name):
    with serial.Serial(port_name, BAUD, timeout=1) as ser:
        print(f"[INFO] Sending barcode data to {port_name}...")
        barcode = 4806509880961
        
        # Simulate scanner: barcode + carriage return
        data = f"{barcode}\r"
        ser.write(data.encode('utf-8'))
        print(f"[SENT] {repr(data)}")

if __name__ == "__main__":
    try:
        send_barcode_data(PORT)
    except serial.SerialException as e:
        print(f"[ERROR] {e}")
