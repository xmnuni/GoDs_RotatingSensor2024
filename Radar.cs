// THIS CODE IS WHAT IM WORKING ONNNNNNN ***********************************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO.Ports;

public class Radar : Sensor
{

    private SerialPort serialPort;
    public string portName; // change accordinly to Bluetooth COM port
    public int baudRate;

	// default constructor
	public Radar()
	{
		// Radar sensors are type 9
		Type = 9;
	}

    void Start()
    {
        portName = "COM6"; //10? was COM9 when last worked 7?
        baudRate = 9600;
        serialPort = new SerialPort(portName, baudRate);
        serialPort.Open();
        Debug.Log("Entered Start");
    }

    public void SendRotation(float rotationDelta)
    {
        if(serialPort.IsOpen)
        {
            Console.WriteLine(((int)rotationDelta).ToString());
            // Debug.Log("rotation" + rotationDelta);
        }
    }

    void Quit()
    {
        if (serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Entered quit");
        }
    }

	
	// calculate the position of a sensor ping
    protected override Vector3 CalculatePing(Ping ping)
	{
		// get the data from the ping
		// LIDAR packets only have the distance as the data
		Debug.Assert(ping.data.Count == 4, "Invalid packet size: " + ping.data.Count.ToString(), this);
		int distance = Convert.ToInt16(ping.data[0]);
	
		// calculate the position of the ping
		
		Vector3 coord = new Vector3();	// the coordinates (xy) of the ping
		
		// the ping should be <distance> away from the sensor
		coord.x = distance;
		coord.y = 0f;
		coord.z = 0f;

        foreach (SensorRenderer sensor in Manager.Sensors){
            if (sensor.Config.id == ping.id){
                //Debug.Log(sensor.radar.transform.GetChild(0).name);
                float rotationDelta = sensor.radarTransform.eulerAngles.z;
                //Debug.Log("Rotation: " + rotationDelta);
                coord = Quaternion.AngleAxis(rotationDelta, Vector3.forward) * coord;
            
                // new added code
                SendRotation(rotationDelta);

            }
        }

        Debug.Log("Coord: " + coord);
        Debug.Log("distance: " + distance);
		
		// apply the sensor's angle and position to the coordinates
		coord = ApplySensorAngle(coord);
		
		coord.x = CmToScreen(coord.x);
		coord.y = CmToScreen(coord.y);
		coord.z *= 0.0328084f;
		
		coord = AddSensorPosition(coord);
		
		// return the ping coordinates
		return coord;
	}
	
	// generate a random fake ping for jamming
	protected override Ping CalculateJamPing()
	{
		// start with an empty string
		string pingPacket = "";
		
		// LIDAR packets are in the form type,id,distance
		// randomize distance to be between 0 and 1000
		pingPacket += Type.ToString();
		pingPacket += "," + SensorData.id.ToString();
		pingPacket += "," + ((int)Mathf.Ceil(UnityEngine.Random.value * 1000f)).ToString();
		
		// create the randomized fake ping
		Ping jamPing = new Ping(pingPacket);
		
		// return the fake ping
		return jamPing;
	}



}



