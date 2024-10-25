using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class BicycleVehicle : MonoBehaviour
{
    [Tooltip("Port name with which the SerialPort object will be created.")]
    public string portName = "COM3";

    [Tooltip("Baud rate that the serial device is using to transmit data.")]
    public int baudRate = 115200;

    [Tooltip("Reference to an scene object that will receive the events of connection, " +
             "disconnection and the messages from the serial device.")]
    public GameObject messageListener;

    [Tooltip("After an error in the serial communication, or an unsuccessful " +
             "connect, how many milliseconds we should wait.")]
    public int reconnectionDelay = 1000;

    [Tooltip("Maximum number of unread data messages in the queue. " +
             "New messages will be discarded.")]
    public int maxUnreadMessages = 1;


    float horizontalInput;
    float vereticallInput;
    SerialPort serialPort = new SerialPort("COM3", 9600);
    float steeringInput;

    public Transform handle;
    bool braking;
    Rigidbody rb;

    public Vector3 COG;

    [SerializeField] float motorforce;
    [SerializeField] float brakeForce;
    float currentbrakeForce;

    float steeringAngle;
    [SerializeField] float currentSteeringAngle;
    [Range(0f, 0.1f)][SerializeField] float speedteercontrolTime;
    [SerializeField] float maxSteeringAngle;
    [Range(0.000001f, 1)][SerializeField] float turnSmoothing;

    [SerializeField] float maxlayingAngle = 45f;
    public float targetlayingAngle;
    [Range(-40, 40)] public float layingammount;
    [Range(0.000001f, 1)][SerializeField] float leanSmoothing;

    [SerializeField] WheelCollider frontWheel;
    [SerializeField] WheelCollider backWheel;

    [SerializeField] Transform frontWheeltransform;
    [SerializeField] Transform backWheeltransform;

    [SerializeField] TrailRenderer fronttrail;
    [SerializeField] TrailRenderer rearttrail;

    public bool frontGrounded;
    public bool rearGrounded;

    // Friction variables
    [SerializeField] float brakeFriction = 1.0f; // Starting friction value
    [SerializeField] float frictionDecayRate = 0.1f; // Rate at which friction decreases
    [SerializeField] float minFriction = 0.1f; // Minimum friction value
    [SerializeField] float frictionRecoveryRate = 0.05f; // Rate at which friction recovers

    // Start is called before the first frame update
    void Start()
    {
        StopEmitTrail();
        rb = GetComponent<Rigidbody>();
        try
        {
            // Check if the specified port exists
            if (System.IO.Ports.SerialPort.GetPortNames().Length > 0)
            {
                serialPort = new SerialPort(portName, baudRate);

                // Ensure the specified port is in the list of available ports
                if (System.Array.Exists(System.IO.Ports.SerialPort.GetPortNames(), port => port == portName))
                {
                    if (!serialPort.IsOpen)
                    {
                        serialPort.Open();
                        serialPort.ReadTimeout = 100;
                        Debug.Log($"Successfully opened port: {portName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Port {portName} not found. Please check the connection.");
                }
            }
            else
            {
                Debug.LogWarning("No available COM ports found. Please check your device connection.");
            }
            }
    catch (System.Exception ex)
    {
        Debug.LogError($"Serial port error on start: {ex.Message}");
    }
        }

    // Update is called once per frame
    void FixedUpdate()
    {
        GetInput();
        HandleEngine();
        HandleSteering();
        UpdateWheels();
        UpdateHandle();
        LayOnTurn();
        DownPresureOnSpeed();
        EmitTrail();
    }

    public void GetInput()
    {
        try
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                string arduino = serialPort.ReadLine();
                steeringInput = float.Parse(arduino);
            }
            else
            {
                steeringInput = Input.GetAxis("Horizontal"); 
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Serial read error: {ex.Message}");
        }

        horizontalInput = Input.GetAxis("Horizontal");
        vereticallInput = Input.GetAxis("Vertical");
        braking = Input.GetKey(KeyCode.Space);
    }

    public void HandleEngine()
    {
        backWheel.motorTorque = vereticallInput * motorforce;
        currentbrakeForce = braking ? brakeForce : 0f;
        if (braking)
        {
            ApplyBraking();
        }
        else
        {
            ReleaseBraking();
        }
    }

    public void DownPresureOnSpeed()
    {
        Vector3 downforce = Vector3.down;
        float downpressure;
        if (rb.velocity.magnitude > 5)
        {
            downpressure = rb.velocity.magnitude;
            rb.AddForce(downforce * downpressure, ForceMode.Force);
        }
    }

    public void ApplyBraking()
    {
        // Reduce the brake friction while braking
        if (brakeFriction > minFriction)
        {
            brakeFriction -= frictionDecayRate * Time.fixedDeltaTime;
        }

        frontWheel.brakeTorque = currentbrakeForce * brakeFriction;
        backWheel.brakeTorque = currentbrakeForce * brakeFriction;
    }

    public void ReleaseBraking()
    {
        frontWheel.brakeTorque = 0;
        backWheel.brakeTorque = 0;

        // Gradually recover the brake friction when not braking
        if (brakeFriction < 1.0f)
        {
            brakeFriction += frictionRecoveryRate * Time.fixedDeltaTime;
        }
        brakeFriction = Mathf.Clamp(brakeFriction, minFriction, 1.0f);
    }

    public void SpeedSteerinReductor()
    {
        if (rb.velocity.magnitude < 5) // We set the limiting factor for the steering
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 50, speedteercontrolTime);
        }
        if (rb.velocity.magnitude > 5 && rb.velocity.magnitude < 10)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 30, speedteercontrolTime);
        }
        if (rb.velocity.magnitude > 10 && rb.velocity.magnitude < 15)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 15, speedteercontrolTime);
        }
        if (rb.velocity.magnitude > 15 && rb.velocity.magnitude < 20)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 10, speedteercontrolTime);
        }
        if (rb.velocity.magnitude > 20)
        {
            maxSteeringAngle = Mathf.LerpAngle(maxSteeringAngle, 5, speedteercontrolTime);
        }
    }

    public void HandleSteering()
    {

        SpeedSteerinReductor();
        if (serialPort != null && serialPort.IsOpen)
        {
            // Directly map the steeringInput to the steering angle
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, steeringInput, turnSmoothing);

            // Clamp the steering angle to ensure it doesn't exceed the bike's max steering capabilities
            currentSteeringAngle = Mathf.Clamp(currentSteeringAngle, -maxSteeringAngle, maxSteeringAngle);

            frontWheel.steerAngle = currentSteeringAngle;
            targetlayingAngle = maxlayingAngle * -steeringInput / maxSteeringAngle;
        }
        else
        {
            currentSteeringAngle = Mathf.Lerp(currentSteeringAngle, maxSteeringAngle * horizontalInput, turnSmoothing);
            frontWheel.steerAngle = currentSteeringAngle;

            targetlayingAngle = maxlayingAngle * -horizontalInput;
        }
    }

    private void LayOnTurn()
    {
        Vector3 currentRot = transform.rotation.eulerAngles;

        if (rb.velocity.magnitude < 1)
        {
            layingammount = Mathf.LerpAngle(layingammount, 0f, 0.05f);
            transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, layingammount);
            return;
        }

        if (currentSteeringAngle < 0.5f && currentSteeringAngle > -0.5) // We're straight
        {
            layingammount = Mathf.LerpAngle(layingammount, 0f, leanSmoothing);
        }
        else // We're turning
        {
            layingammount = Mathf.LerpAngle(layingammount, targetlayingAngle, leanSmoothing);
            rb.centerOfMass = new Vector3(rb.centerOfMass.x, COG.y, rb.centerOfMass.z);
        }

        transform.rotation = Quaternion.Euler(currentRot.x, currentRot.y, layingammount);
    }

    public void UpdateWheels()
    {
        UpdateSingleWheel(frontWheel, frontWheeltransform);
        UpdateSingleWheel(backWheel, backWheeltransform);
    }

    public void UpdateHandle()
    {
        Quaternion sethandleRot;
        sethandleRot = frontWheeltransform.rotation;
        handle.localRotation = Quaternion.Euler(handle.localRotation.eulerAngles.x, currentSteeringAngle, handle.localRotation.eulerAngles.z);
    }

    private void EmitTrail()
    {
        frontGrounded = frontWheel.GetGroundHit(out WheelHit Fhit);
        rearGrounded = backWheel.GetGroundHit(out WheelHit Rhit);

        fronttrail.emitting = frontGrounded;
        rearttrail.emitting = rearGrounded;
    }

    private void StopEmitTrail()
    {
        fronttrail.emitting = false;
        rearttrail.emitting = false;
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    void OnApplicationQuit()
    {
        if (serialPort.IsOpen)
            serialPort.Close();
    }
}
