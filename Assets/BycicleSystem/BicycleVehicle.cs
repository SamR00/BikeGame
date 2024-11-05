using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class BicycleVehicle : MonoBehaviour
{
    public string portName = "COM3";
    public int baudRate = 115200;
    public int readTimeout = 1000;
    private SerialPort serialPort;

    private string lastReceivedData = ""; // Store the last received value.

    float horizontalInput;
    float vereticallInput;
    float steeringInput;

    public Transform handle;
    bool braking;

    public Vector3 COG;

    [SerializeField] float movementSpeed = 10f; // Direct movement speed multiplier.
    [SerializeField] float brakeSpeed = 5f; // Speed reduction when braking.

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
        // Initialize the serial port.
        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = readTimeout;

        try
        {
            serialPort.Open();
            Debug.Log("Serial port opened successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to open serial port: {ex.Message}");
        }
    }

    void FixedUpdate()
    {
        GetInput();
        HandleEngine();
        HandleSteering();
        UpdateWheels();
        UpdateHandle();
        LayOnTurn();
        EmitTrail();
    }

    private float lastValidValue = 0f;
    private float threshold = 5f;

    private void ProcessReceivedValue(float newValue)
    {
        if (Mathf.Abs(newValue - lastValidValue) < threshold)
        {
            steeringInput = newValue;
            lastValidValue = newValue;
        }
        else
        {
            Debug.LogWarning($"Filtered out an outlier: {newValue}");
        }
    }

    public void GetInput()
    {
        try
        {
            if (serialPort.IsOpen)
            {
                string arduinoData = serialPort.ReadExisting();
                string[] dataParts = arduinoData.Split(',');

                if (!string.IsNullOrEmpty(arduinoData))
                {
                    Debug.Log(arduinoData);

                    if (float.TryParse(dataParts[0], out float parsedValue))
                    {
                        steeringInput = -parsedValue;
                    }
                    else
                    {
                        Debug.LogWarning("Received data could not be parsed to a float.");
                    }

                    if (float.TryParse(dataParts[2], out float speedValue))
                    {
                        float newSpeed = speedValue / 10;
                        vereticallInput = Mathf.Clamp(newSpeed, 0f, 40f);
                    }
                    else
                    {
                        vereticallInput = Input.GetAxis("Vertical");
                        Debug.LogWarning("Speed data could not be parsed.");
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Serial read error: {ex.Message}");
        }

        horizontalInput = Input.GetAxis("Horizontal");
        braking = Input.GetKey(KeyCode.Space);

        // Additional code to handle Z and S keys
        if (Input.GetKey(KeyCode.Z))
        {
            vereticallInput = 1f; // Moving forward
        }
        else if (Input.GetKey(KeyCode.S))
        {
            vereticallInput = -1f; // Moving backward
        }
        else if (serialPort == null || !serialPort.IsOpen)
        {
            vereticallInput = 0f; // No input if neither key is pressed
        }
    }

    public void HandleEngine()
    {
        float speed = vereticallInput * movementSpeed * Time.fixedDeltaTime;

        if (braking)
        {
            speed = Mathf.Max(speed - brakeSpeed * Time.fixedDeltaTime, 0);
        }

        transform.Translate(Vector3.forward * speed);
    }

    public void HandleSteering()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, steeringInput, turnSmoothing);
            currentSteeringAngle = Mathf.Clamp(currentSteeringAngle, -maxSteeringAngle, maxSteeringAngle);

            targetlayingAngle = maxlayingAngle * -steeringInput / maxSteeringAngle;
        }
        else
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * horizontalInput, turnSmoothing);
            targetlayingAngle = maxlayingAngle * -horizontalInput;
        }

        transform.Rotate(Vector3.up * currentSteeringAngle * Time.fixedDeltaTime);
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
        if (serialPort.IsOpen)
            serialPort.Close();
    }
}
