/*
#include <ArduinoBLE.h>
#include <Servo.h>

Servo myServo;
BLEService angleService("19b10000-e8f2-537e-4f6c-d104768a1214"); // Custom BLE service
BLEIntCharacteristic angleCharacteristic("19b10001-e8f2-537e-4f6c-d104768a1214", BLERead | BLEWrite); // Custom BLE characteristic for sending/receiving angle data

void setup() {
  Serial.begin(9600);
  if (!BLE.begin()) {
    Serial.println("starting BLE failed!");
    while (1);
  }

  // Set up BLE
  BLE.setLocalName("Nano33_Servo");
  BLE.setAdvertisedService(angleService);
  angleService.addCharacteristic(angleCharacteristic);
  BLE.addService(angleService);
  angleCharacteristic.writeValue(90); // Default angle

  BLE.advertise();
  Serial.println("Bluetooth device active, waiting for connections...");

  // Set up servo
  myServo.attach(9);
}

void loop() {
  // Wait for BLE central device to connect
  BLEDevice central = BLE.central();

  if (central) {
    Serial.print("Connected to central: ");
    Serial.println(central.address());

    // While the central device is connected
    while (central.connected()) {
      if (angleCharacteristic.written()) {
        int angle = angleCharacteristic.value();
        if (angle >= 0 && angle <= 180) {
          myServo.write(angle); // Move the servo based on the received angle
          Serial.print("Servo angle set to: ");
          Serial.println(angle);
        }
      }
    }

    Serial.print("Disconnected from central: ");
    Serial.println(central.address());
  }
}

*/


//COM 14 Bluetooth address: 90:8a:fb:2c:b8:88
#include <ArduinoBLE.h>
#include <Servo.h>

Servo myServo;
BLEService angleService("19b10000-e8f2-537e-4f6c-d104768a1214"); // Custom BLE service
BLEIntCharacteristic angleCharacteristic("19b10001-e8f2-537e-4f6c-d104768a1214", BLERead | BLEWrite); // Custom BLE characteristic for sending/receiving angle data

void setup() {
  Serial.begin(9600);
  if (!BLE.begin()) {
    Serial.println("starting BLE failed!");
    while (1);
  }

  BLE.setLocalName("Nano33_Servo");
  BLE.setAdvertisedService(angleService);
  angleService.addCharacteristic(angleCharacteristic);
  BLE.addService(angleService);
  angleCharacteristic.writeValue(90);  // Initial angle

  BLE.advertise();
  Serial.println("Bluetooth device active, waiting for connections...");

  myServo.attach(9);  // Attach servo to pin 9
}

void loop() {
  BLEDevice central = BLE.central();

  if (central) {
    Serial.print("Connected to central: ");
    Serial.println(central.address());

    while (central.connected()) {
      //Serial.println("enter While");
      if (angleCharacteristic.written()) {
        int angle = angleCharacteristic.value();
        Serial.print("Received angle: ");
        Serial.println(angle);  // Log the received angle

        if (angle >= 0 && angle <= 180) {
          myServo.write(angle);  // Move the servo to the specified angle
          Serial.print("Servo angle set to: ");
          Serial.println(angle);  // Log that the servo angle has been set
        } else {
          Serial.println("Invalid angle received");
        }
      }
    }

    Serial.println("Disconnected from central");
  }
}


