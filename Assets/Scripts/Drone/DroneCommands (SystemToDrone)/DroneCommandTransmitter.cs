using System;
using UnityEngine;

public class DroneCommandTransmitter : UdpTransmitter
{
    [Header("Drone Command Source")]
    private IDroneCommandInput commandInput;

    [Header("Transmission Settings")]
    public float sendRateHz = 20f; // how many times per second to send
    private float sendInterval;
    private float timer;

    protected override void Awake()
    {
        base.Awake();

        // Cache reference to input source
        Check.PropertyEnabledElseAssign<IDroneCommandInput>(this, "commandInput");

        sendInterval = 1f / Mathf.Max(1f, sendRateHz);
    }

    void Update()
    {
        if (commandInput == null || !commandInput.IsActive()) return;

        timer += Time.deltaTime;
        if (timer >= sendInterval)
        {
            timer -= sendInterval;
            DroneCommand cmd = commandInput.GetCommand();
            byte[] data = SerializeCommand(cmd);
            Transmit(data);
        }
    }

    /// <summary>
    /// Convert DroneCommand into 4 doubles (roll, pitch, yaw, altitude).
    /// Matches Simulink expectations.
    /// </summary>
    private byte[] SerializeCommand(DroneCommand cmd)
    {
        double[] values = { cmd.Roll, cmd.Pitch, cmd.Yaw, cmd.Altitude };
        byte[] data = new byte[values.Length * sizeof(double)];
        for (int i = 0; i < values.Length; i++)
        {
            Array.Copy(BitConverter.GetBytes(values[i]), 0, data, i * sizeof(double), sizeof(double));
        }
        return data;
    }
}