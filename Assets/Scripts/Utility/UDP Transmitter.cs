using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public abstract class UdpTransmitter : MonoBehaviour
{
    [Header("UDP Transmitter Settings")]
    public string remoteAddress = "127.0.0.1"; // Destination IP
    public int remotePort = 9091;              // Destination Port

    private UdpClient udpClient;
    private IPEndPoint remoteEndPoint;

    protected virtual void Awake()
    {
        try
        {
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteAddress), remotePort);
            udpClient = new UdpClient();

            Debug.Log($"{GetType().Name} ready to send to {remoteAddress}:{remotePort}");
        }
        catch (Exception e)
        {
            Debug.LogError("UDP transmitter init failed: " + e.Message);
        }
    }

    /// <summary>
    /// Call this method (from subclass or other scripts) to send data.
    /// </summary>
    protected void Transmit(byte[] data)
    {
        if (udpClient == null) return;

        try
        {
            udpClient.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception e)
        {
            Debug.LogError("UDP send error: " + e.Message);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        udpClient?.Close();
        udpClient = null;
    }
}