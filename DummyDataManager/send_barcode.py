import serial
import sys
import readline

def input_with_prefill(prompt, text):
    def hook():
        readline.insert_text(text)
        readline.redisplay()

    readline.set_pre_input_hook(hook)
    try:
        return input(prompt).strip()
    finally:
        readline.set_pre_input_hook()

def main():
    com_port = "COM1"
    baud_rate = 9600
    
    print("=" * 50)
    print("BARCODE SCANNER SIMULATOR")
    print("=" * 50)
    print(f"Target COM Port: {com_port}")
    print(f"Baud Rate: {baud_rate}")
    print("=" * 50)
    
    # Connect to COM port
    try:
        ser = serial.Serial(com_port, baud_rate, timeout=1)
        print(f"\n✓ Successfully connected to {com_port}")
        print("  Type a barcode and press Enter to send")
        print("=" * 50)
    except serial.SerialException as e:
        print(f"\n✗ ERROR: Could not connect to {com_port}")
        print(f"  {e}")
        print("\nMake sure:")
        print("  1. The COM port exists and is not in use")
        print("  2. Run as Administrator (required for virtual COM ports)")
        sys.exit(1)
    
    # Main loop
    try:
        last_barcode = ""
        while True:
            barcode = input_with_prefill("\nEnter barcode: ", last_barcode)
            
            if not barcode:
                print("Empty barcode. Please enter a value.")
                continue
            
            last_barcode = barcode
            
            # Send barcode with carriage return (Enter suffix)
            try:
                ser.write((barcode + "\r").encode('ascii'))
                print(f"✓ Sent: {barcode}")
            except serial.SerialException as e:
                print(f"✗ ERROR sending barcode: {e}")
                break
    
    except KeyboardInterrupt:
        print("\n\nInterrupted by user.")
    
    finally:
        ser.close()
        print(f"✓ Closed {com_port}")

if __name__ == "__main__":
    main()