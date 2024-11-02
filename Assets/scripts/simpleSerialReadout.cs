using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class SimpleSerialReader : MonoBehaviour
{
    public string portName = "COM3";
    public int baudRate = 115200; 
    public int readTimeout = 1000;
    private SerialPort serialPort;

    private string buffer = "";
    private string lastReceivedData = ""; // Store the last received value.

    void Start()
    {
        // Initialize the serial port.
        serialPort = new SerialPort(portName, baudRate);
        serialPort.ReadTimeout = readTimeout; // Timeout in milliseconds.

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

    void Update()
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                // Read all available data without blocking.
                string data = serialPort.ReadExisting();
                buffer += data; // Add the new data to the buffer.

                // Check if we have a complete line (end with newline character).
                int newlineIndex = buffer.IndexOf('\n');
                if (newlineIndex >= 0)
                {
                    // Extract the complete line.
                    string completeLine = buffer.Substring(0, newlineIndex).Trim();
                    buffer = buffer.Substring(newlineIndex + 1); // Remove the processed line from the buffer.

                    // Only print if the data is different from the last received value.
                    if (completeLine != lastReceivedData)
                    {
                        Debug.Log($"Received: {completeLine}");
                        lastReceivedData = completeLine;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Serial read error: {ex.Message}");
            }
        }
    }

    void OnApplicationQuit()
    {
        // Close the serial port when the application quits.
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }
    }
}
