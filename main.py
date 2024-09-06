""" # works
import socket
import serial
import time

# Set up the serial connection to Arduino
arduino_port = 'COM16'  # Change this to your Arduino's COM port
baud_rate = 9600  # Should match the baud rate set in Arduino
arduino = serial.Serial(arduino_port, baud_rate)
time.sleep(2)  # Wait for the connection to initialize

# Set up the TCP server
host = '127.0.0.1'  # Localhost
port = 5555  # Same port as in Unity

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((host, port))
server_socket.listen(1)
print("Server started, waiting for connection...")

conn, addr = server_socket.accept()
print(f"Connected by {addr}")

try:
    while True:
        data = conn.recv(1024)
        if not data:
            break
        angle = int(data.decode('ascii'))
        print(f"Received angle: {angle}")

        # Send the angle to Arduino
        arduino.write(f"{angle}\n".encode('utf-8'))
        print(f"Sent angle to Arduino: {angle}")

except KeyboardInterrupt:
    print("Interrupted by user")

finally:
    conn.close()
    server_socket.close()
    arduino.close()

"""


"""
import asyncio
import socket
from bleak import BleakClient

# UUIDs for the BLE service and characteristic
ANGLE_SERVICE_UUID = "19b10000-e8f2-537e-4f6c-d104768a1214"
ANGLE_CHARACTERISTIC_UUID = "19b10001-e8f2-537e-4f6c-d104768a1214"
ARDUINO_BLE_ADDRESS = "90:8a:fb:2c:b8:88"  # Your Arduino's Bluetooth address


# Function to send angle via BLE
async def send_angle(angle, client):
    try:
        await client.write_gatt_char(ANGLE_CHARACTERISTIC_UUID, bytearray([angle]))
        print(f"Sent angle: {angle}")
    except Exception as e:
        print(f"Failed to send angle: {e}")


# Function to handle incoming TCP data from Unity
async def handle_tcp_connection():
    host = '127.0.0.1'  # Listen on localhost
    port = 5555  # Same port as in Unity

    # Set up BLE client for Arduino
    client = BleakClient(ARDUINO_BLE_ADDRESS)
    await client.connect()

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind((host, port))
        server_socket.listen(1)
        print("Waiting for Unity connection...")

        conn, addr = server_socket.accept()
        print(f"Connected by {addr}")

        with conn:
            while True:
                data = conn.recv(1024).strip()  # Read data from Unity
                if not data:
                    print("No data received, stopping")
                    break  # Stop if the connection is closed
                try:
                    angle = int(data.decode('ascii').strip())
                    print(f"Received angle from Unity: {angle}")

                    # Send the angle to the Arduino via BLE asynchronously
                    await send_angle(angle, client)  # Ensure BLE data is written immediately

                except ValueError:
                    print("Invalid data received")


if __name__ == "__main__":
    asyncio.run(handle_tcp_connection())

"""


import asyncio
import socket
from bleak import BleakClient

# UUIDs for the BLE service and characteristic
ANGLE_SERVICE_UUID = "19b10000-e8f2-537e-4f6c-d104768a1214"
ANGLE_CHARACTERISTIC_UUID = "19b10001-e8f2-537e-4f6c-d104768a1214"
ARDUINO_BLE_ADDRESS = "90:8a:fb:2c:b8:88"  # Your Arduino's Bluetooth address


# Function to send angle via BLE
async def send_angle(angle, client):
    try:
        # Ensure the angle is within byte range
        if 0 <= angle <= 255:
            await client.write_gatt_char(ANGLE_CHARACTERISTIC_UUID, bytearray([angle]))
            print(f"Sent angle: {angle}")
        else:
            print(f"Invalid angle:") # {angle}, must be between 0 and 255")
    except Exception as e:
        print(f"Failed to send angle: {e}")


# Function to handle incoming TCP data from Unity
async def handle_tcp_connection():
    # while True:
    host = '127.0.0.1'  # Listen on localhost
    port = 5555  # Same port as in Unity

    # Set up BLE client for Arduino
    client = BleakClient(ARDUINO_BLE_ADDRESS)
    await client.connect()

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
        server_socket.bind((host, port))
        server_socket.listen(1)
        print("Waiting for Unity connection...")

        conn, addr = server_socket.accept()
        print(f"Connected by {addr}")

        with conn:
            while True:
                data = conn.recv(1024).strip()  # Read data from Unity
                if not data:
                    print("No data received, stopping")
                    # await client.disconnect()
                    break
                try:
                    # Ensure we split and handle each angle correctly
                    angle_str = data.decode('ascii').strip()
                    angles = angle_str.split()  # Split incoming string in case multiple angles are concatenated
                    for angle in angles:
                        if angle.isdigit():
                            angle_value = int(angle)
                            print(f"Received angle from Unity: {angle_value}")

                            # Send the angle to the Arduino via BLE asynchronously
                            await send_angle(angle_value, client)
                except ValueError:
                    print("Invalid data received")


if __name__ == "__main__":
    asyncio.run(handle_tcp_connection())
