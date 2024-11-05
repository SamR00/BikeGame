using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class BicycleVehicle : MonoBehaviour
{
    public string portName = "/dev/tty.usbmodem11401";
    public int baudRate = 115200;
    public int readTimeout = 1000;
    private SerialPort serialPort;
    private Thread serialThread;
    private bool isSerialRunning = false;

    private string lastReceivedData = ""; // Store the last received value.

    float horizontalInput;
    float vereticallInput;
    float steeringInput;

    private readonly object lockObject = new object(); // For thread-safe access to data

    public Transform handle;
    bool braking;

    public Vector3 COG;

    [SerializeField] float movementSpeed = 10f;
    [SerializeField] float brakeSpeed = 5f;

    float steeringAngle;
    [SerializeField] float currentSteeringAngle;
    [Range(0f, 0.1f)][SerializeField] float speedteercontrolTime;
    [SerializeField] float maxSteeringAngle;
    [Range(0.000001f, 1)][SerializeField] float turnSmoothing;

    [SerializeField] float maxlayingAngle = 45f;
    public float targetlayingAngle;
    [Range(-40, 40)] public float layingammount;
    [Range(0.000001f, 1)][SerializeField] float leanSmoothing;

    [SerializeField] Transform frontWheeltransform;
    [SerializeField] Transform backWheeltransform;

    [SerializeField] TrailRenderer fronttrail;
    [SerializeField] TrailRenderer rearttrail;

    public bool frontGrounded;
    public bool rearGrounded;

    void Start()
    {
        StopEmitTrail();

        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = readTimeout;

        try
        {
            serialPort.Open();
            Debug.Log("Serial port opened successfully.");

            isSerialRunning = true;
            serialThread = new Thread(SerialReadThread);
            serialThread.Start();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to open serial port: {ex.Message}");
        }
    }

    void Update()
    {
        GetInput(); 
        HandleEngine();
        HandleSteering();
        UpdateWheels();
        UpdateHandle();
        LayOnTurn();
        EmitTrail();
    }

    private void SerialReadThread()
    {
        while (isSerialRunning && serialPort.IsOpen)
        {
            try
            {
                string arduinoData = serialPort.ReadLine();
                lock (lockObject)
                {
                    lastReceivedData = arduinoData;
                }
            }
            catch (System.TimeoutException)
            {
                // Timeout occurred, can skip handling to avoid blocking
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Serial read error: {ex.Message}");
            }
        }
    }

    //data komt nu van seperate thread waardoor programma niet elke frame ligt te wachten op input
    public void GetInput()
    {
        string[] dataParts;

        lock (lockObject)
        {
            // Make a local copy of the last received data
            dataParts = lastReceivedData.Trim().Split(',');
        }

        if (dataParts.Length >= 3)
        {
            // Parse steering input(a)
            if (float.TryParse(dataParts[0], out float parsedSteering))
            {
                steeringInput = -parsedSteering;
            }
            else
            {
                Debug.LogWarning("Steering data could not be parsed to a float.");
            }

            // Parse speed input(c)
            if (float.TryParse(dataParts[2], out float parsedSpeed))
            {
                float newSpeed = parsedSpeed / 10f;
                vereticallInput = Mathf.Clamp(newSpeed, 0f, 5f);
            
            }
            else
            {
                vereticallInput = Input.GetAxis("Vertical");
                Debug.LogWarning("Speed data could not be parsed.");
            }
        }
        else
        {
            Debug.LogWarning($"Incomplete data received: '{lastReceivedData}'");
        }

        horizontalInput = Input.GetAxis("Horizontal");
        braking = Input.GetKey(KeyCode.Space);
    }

    public void HandleEngine()
    {
        float speed = vereticallInput * movementSpeed * Time.deltaTime;

        if (braking)
        {
            speed = Mathf.Max(speed - brakeSpeed * Time.deltaTime, 0);
        }

        transform.Translate(Vector3.forward * speed);
    }

    public void HandleSteering()
    {
        currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, steeringInput, turnSmoothing);
        currentSteeringAngle = Mathf.Clamp(currentSteeringAngle, -maxSteeringAngle, maxSteeringAngle);

        targetlayingAngle = maxlayingAngle * -steeringInput / maxSteeringAngle;

        transform.Rotate(Vector3.up * currentSteeringAngle * Time.deltaTime);
    }

    private void LayOnTurn()
    {
        Vector3 currentRot = transform.rotation.eulerAngles;

        if (Mathf.Abs(currentSteeringAngle) < 0.5f)
        {
            layingammount = Mathf.LerpAngle(layingammount, 0f, leanSmoothing);
        }
        else
        {
            layingammount = Mathf.LerpAngle(layingammount, targetlayingAngle, leanSmoothing);
        }

        transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, layingammount);
    }

    public void UpdateWheels()
    {
        UpdateSingleWheel(frontWheeltransform);
        UpdateSingleWheel(backWheeltransform);
    }

    public void UpdateHandle()
    {
        Quaternion sethandleRot = frontWheeltransform.rotation;
        handle.localRotation = Quaternion.Euler(handle.localRotation.eulerAngles.x, currentSteeringAngle, handle.localRotation.eulerAngles.z);
    }

    private void EmitTrail()
    {
        fronttrail.emitting = true;
        rearttrail.emitting = true;
    }

    private void StopEmitTrail()
    {
        fronttrail.emitting = false;
        rearttrail.emitting = false;
    }

    private void UpdateSingleWheel(Transform wheelTransform)
    {
        wheelTransform.localRotation = Quaternion.Euler(new Vector3(0, currentSteeringAngle, 0));
    }

    void OnApplicationQuit()
    {
        isSerialRunning = false;
        Thread.Sleep(100); //Allow the thread to exit
        if (serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }
}
