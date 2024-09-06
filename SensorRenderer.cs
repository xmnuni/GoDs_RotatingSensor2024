
using System.Collections;
using System.Collections.Generic;  // Add this for Dictionary support
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class SensorRenderer : MonoBehaviour
{
    public SensorConfiguration Config { get; set; }   // the sensor configuration
    CageRenderer Cage;                                // includes display information about the cage
    static Dictionary<string, Sprite> Sprites;        // the sprites for different sensor types
    SpriteRenderer SensorSprite;                      // interface for visually changing the sensor sprite
    public GameObject radar;
    private GameObject currentRadarInstance;
    private float rotationSpeed = 90f;
    private float currentAngle;
    private bool rotatingClockwise;
    public Transform radarTransform; 
    private bool radarInitialized;

    // TCP connection variables
    private TcpClient client;
    private NetworkStream stream;

	// Really stupid fix
	public bool setupTCP;

    // Initialize TCP connection and radar setup
    void Start()
    {
		// // Setup TCP connection to Python
		// try
		// {
		// 	client = new TcpClient("127.0.0.1", 5555);  // IP of Python server
		// 	stream = client.GetStream();
		// }
		// catch (System.Exception e)
		// {
		// 	Debug.LogError("Failed to connect: " + e.Message);
		// }

		// Radar setup (if radar is enabled in the scene)
		// OnEnable();  // Assume OnEnable sets up the radar
    }

    // OnEnable initializes radar sprites and configuration
    void OnEnable()
    {
        // create a dictionary matching names of sensor types to corresponding sprites
        if (Sprites == null)
        {
            Sprites = new Dictionary<string, Sprite>();

            System.Type[] sensorTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Sensor)))
                .ToArray();

            foreach (System.Type type in sensorTypes)
            {
                Sprites.Add(type.Name, Resources.Load<Sprite>("Images/" + type.Name));
            }
        }

        SensorSprite = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
        Cage = GameObject.Find("Drone Cage").GetComponent<CageRenderer>() as CageRenderer;
    }

	// void OnDisable(){
	// 	if (stream != null)
    //     {
	// 		Debug.Log("stream closed");
    //         stream.Close();
    //     }
    //     if (client != null)
    //     {
	// 		Debug.Log("client closed");
    //         client.Dispose();
	// 		client.Close();
    //     }
	// 	Debug.Log("OnDisable Called!");
	// }

    // Main Update loop to move the radar and send angles to Python
    void Update() 
    {
		if (SceneManager.GetActiveScene().name == "SensorReadings" && !setupTCP && Config.type == "Radar"){
			try
			{
				client = new TcpClient("127.0.0.1", 5555);  // IP of Python server
				stream = client.GetStream();
			}
			catch (System.Exception e)
			{
				Debug.LogError("Failed to connect: " + e.Message);
			}
			setupTCP = true;
		}
        if (radarInitialized && currentRadarInstance != null)
        {
            float rotationThisFrame = rotationSpeed * Time.deltaTime;

            // Perform the oscillating rotation
            if (rotatingClockwise)
            {
                radarTransform.eulerAngles -= new Vector3(0, 0, rotationThisFrame);
                currentAngle += rotationThisFrame;
            }
            else
            {
                radarTransform.eulerAngles += new Vector3(0, 0, rotationThisFrame);
                currentAngle -= rotationThisFrame;
            }

            // Clamp and reverse rotation direction at 0 and 180 degrees
            if (currentAngle >= 180f)
            {
                rotatingClockwise = false;
                currentAngle = 180f;  // clamp to 180
            }
            else if (currentAngle <= 0f)
            {
                rotatingClockwise = true;
                currentAngle = 0f;  // clamp to 0
            }

			Config.hRotation = -currentAngle;

            // Send the current radar angle to Python (to forward to Arduino)
			SendAngleToPython((int)currentAngle);
        }
    }

    // Function to send angle data to Python
    void SendAngleToPython(int angle)
    {
        if (stream != null)
        {
            string message = angle.ToString();
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
            stream.Flush();  // Ensure data is sent immediately
            // Debug.Log("Sent angle to Arduino: " + angle);
        }
    }

    // Called when the application is quitting to close the TCP connection
    void OnApplicationQuit()
    {
		Debug.Log("Application Quit Called!");
        if (stream != null)
        {
            stream.Close();
        }
        if (client != null)
        {
            client.Close();
        }
    }

    // UpdateConfig handles sensor configuration, radar instantiation, and setup
    public void UpdateConfig(SensorConfiguration sensor)
    {
        if (Cage == null)
        {
            Cage = GameObject.Find("Drone Cage").GetComponent<CageRenderer>() as CageRenderer;
        }

        Config = sensor;
        Vector3 newPos = new Vector3();
        newPos.x = Cage.right - Cage.FeetToPixels((ConfigStorage.GetConfig().sectorLength * ConfigStorage.GetConfig().sectorCountX)) + Cage.FeetToPixels(Config.x);
        newPos.y = Cage.down + Cage.FeetToPixels(Config.y);
        newPos.z = 0;
        transform.position = newPos;

        Vector3 newRot = new Vector3();
        newRot.z = Config.hRotation;  
        transform.eulerAngles = newRot;

        if (Config.type == "Radar")
        {
            if (currentRadarInstance != null)
            {
                Destroy(currentRadarInstance);
                radarInitialized = false;
            }

            currentRadarInstance = Instantiate(radar);
            radarTransform = currentRadarInstance.transform.GetChild(0).GetComponent<Transform>();
            currentRadarInstance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            currentRadarInstance.transform.position = newPos;
            SensorSprite.sprite = Sprites[Config.type];

            rotationSpeed = 180f;
            currentAngle = 90f;  // Start at 90 degrees
            rotatingClockwise = true;

            radarInitialized = true;
        }
        else
        {
            if (currentRadarInstance != null)
            {
                Destroy(currentRadarInstance);
                radarInitialized = false;
            }
            SensorSprite.sprite = Sprites[Config.type];
        }
    }
}




/*
///old works
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

// Convert a sensor configuration into a visual sensor icon on the screen
public class SensorRenderer : MonoBehaviour
{
	
	public SensorConfiguration Config { get; set; }	// the sensor configuration
	CageRenderer Cage;								// includes display information about the cage
	static Dictionary<string, Sprite> Sprites;		// the sprites for different sensor types
	SpriteRenderer SensorSprite;					// interface for visually changing the sensor sprite
	public GameObject radar;
	private GameObject currentRadarInstance;

	// private Transform sensorTransform;
    private float rotationSpeed;
    private float currentAngle;
    private bool rotatingClockwise;
	public Transform radarTransform; 
	private bool radarInitialized;
	// public Bluetooth bluetooth;

	// runs when object is enabled
	void OnEnable()
	{
		// create a dictionary matching names of sensor types to corresponding sprites
		if (Sprites == null)
		{
			// create dictionary
			Sprites = new Dictionary<string, Sprite>();
			
			// get all subclasses of "Sensor"
			System.Type[] sensorTypes = System.AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.IsSubclassOf(typeof(Sensor)))
				.ToArray();

			// populate dictionary
			foreach (System.Type type in sensorTypes)
			{
				// add class type name matched with the sprite matching the name to the dictionary
				Sprites.Add(type.Name, Resources.Load<Sprite>("Images/" + type.Name));
			}
		}

		// get the components
		SensorSprite = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
		Cage = GameObject.Find("Drone Cage").GetComponent<CageRenderer>() as CageRenderer;
	}

	void Update() {
		if (radarInitialized && currentRadarInstance != null){
			float rotationThisFrame = rotationSpeed * Time.deltaTime;
		
			// Perform the oscillating rotation
			if (rotatingClockwise)
			{
				radarTransform.eulerAngles -= new Vector3(0, 0, rotationThisFrame);
				currentAngle += rotationThisFrame;
			}
			else
			{
				radarTransform.eulerAngles += new Vector3(0, 0, rotationThisFrame);
				currentAngle -= rotationThisFrame;
			}

			if (currentAngle >= 180f)
			{
				rotatingClockwise = false;
				currentAngle = 180f; // clamp the angle to 180
			}
			else if (currentAngle <= 0f)
			{
				rotatingClockwise = true;
				currentAngle = 0f; // clamp the angle to 0
			}
		}
	}

    public void UpdateConfig(SensorConfiguration sensor)
    {
		// it is possible for the stored CageRenderer to become null as the scene changes (confirm button clicked)
		if (Cage == null)
		{
			// to fix the Cage attribute, find the object named "Drone Cage" and get its CageRenderer component
			Cage = GameObject.Find("Drone Cage").GetComponent<CageRenderer>() as CageRenderer;
		}
		
		// save the updated configuration settings
		Config = sensor;
		
		// calculate the sensor's position on the screen
        //	x will be the right edge of the cage - the length of all sectors combined to get the
		//		left edge of the red team area (x = 0). Then add the x of the sensor
		//	y will be the bottom of the cage (y = 0) + the y of the sensor
		Vector3 newPos = new Vector3();
		newPos.x = Cage.right - 
			Cage.FeetToPixels((ConfigStorage.GetConfig().sectorLength * ConfigStorage.GetConfig().sectorCountX)) + 
			Cage.FeetToPixels(Config.x);
		newPos.y = Cage.down + Cage.FeetToPixels(Config.y);
		newPos.z = 0;	// z doesn't really matter as long as it shows in front of other objects
		// update position
		transform.position = newPos;
		
		// calculate the sensor's horizontal rotation on the screen
		Vector3 newRot = new Vector3();
		newRot.x = 0f;
		newRot.y = 0f;
		// z is the only one that will affect the visual rotation
		// it should be in degrees the rotation from the configuration
		newRot.z = Config.hRotation;	
		// update the rotaiton
		transform.eulerAngles = newRot;
		
		if (Config.type == "Radar"){
			if (currentRadarInstance != null)
			{
				Destroy(currentRadarInstance);
				radarInitialized = false;
			}
			currentRadarInstance = Instantiate(radar);// transform);

			// get the components
			// SensorSprite = currentRadarInstance.transform.GetChild(0).GetComponent<SpriteRenderer>() as SpriteRenderer;
			radarTransform = currentRadarInstance.transform.GetChild(0).GetComponent<Transform>();
			// bluetooth = currentRadarInstance.GetComponent<Bluetooth>();

			currentRadarInstance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			currentRadarInstance.transform.position = newPos;
			SensorSprite.sprite = Sprites[Config.type];
			//currentRadarInstance.transform.localPosition = Vector3.zero;
			//currentRadarInstance.transform.localRotation= Quaternion.identity;

			// sensorTransform = sensorObject.transform;
			rotationSpeed = 180f;
			currentAngle = 90f;  // start at 0 degrees
			rotatingClockwise = true;

			// Move to the initial 90 degrees position
			radarTransform.transform.eulerAngles = new Vector3(0, 0, currentAngle);
		
			radarInitialized = true;
		}
		else {

			if (currentRadarInstance != null)
			{
				Destroy(currentRadarInstance);
				currentRadarInstance = null;
				radarInitialized = false;
			}
			// update the sensor image (sprite) with the sprite that matches the sensor's type
			// Debug.Log("type: " + Config.type);
			// Debug.Log("sprite: " + Sprites[Config.type]);
			// Debug.Log("sensor sprite: " + SensorSprite);
			SensorSprite.sprite = Sprites[Config.type];
		}

		// Debug.Log("TYPE: " + Config.type);
    }
}

*/