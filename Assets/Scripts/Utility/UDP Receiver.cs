using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// Thread-safe base class for receiving UDP packets in Unity.
/// Subclass and implement OnReceive to process incoming data.
/// </summary>
public abstract class UdpReceiver : MonoBehaviour
{
    [Header("UDP Receiver Settings")]
    public int listenPort = 9090;

    private UdpClient udpClient;
    private Thread receiveThread;
    private CancellationTokenSource cts;

    protected virtual void Awake()
    {
        try
        {
            udpClient = new UdpClient(listenPort);
            cts = new CancellationTokenSource();

            receiveThread = new Thread(() => ReceiveLoop(cts.Token))
            {
                IsBackground = true
            };
            receiveThread.Start();

            Debug.Log($"{GetType().Name} listening on UDP port {listenPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP init failed: {e.Message}");
        }
    }

    private void ReceiveLoop(CancellationToken token)
    {
        IPEndPoint remoteEndPoint = new(IPAddress.Any, listenPort);

        try
        {
            while (!token.IsCancellationRequested)
            {
                // If socket was closed externally, break out cleanly
                if (udpClient == null) break;

                if (udpClient.Available > 0)
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    try
                    {
                        OnReceive(data);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error in OnReceive: {ex.Message}");
                    }
                }
                else
                {
                    // Avoid busy-waiting
                    Thread.Sleep(2);
                }
            }
        }
        catch (SocketException se)
        {
            if (!token.IsCancellationRequested)
                Debug.LogWarning($"UDP socket error: {se.Message}");
        }
        catch (ObjectDisposedException)
        {
            // Expected during shutdown
        }
        catch (Exception e)
        {
            if (!token.IsCancellationRequested)
                Debug.LogError($"UDP receive error: {e.Message}");
        }
        finally
        {
            udpClient?.Close();
        }
    }

    /// <summary>
    /// Override this in subclasses to handle received data.
    /// </summary>
    protected abstract void OnReceive(byte[] data);

    protected virtual void OnApplicationQuit()
    {
        StopReceiver();
    }

    protected virtual void OnDestroy()
    {
        StopReceiver();
    }

    private void StopReceiver()
    {
        if (cts == null) return;

        try
        {
            cts.Cancel();

            // Closing the client unblocks Receive()
            udpClient?.Close();

            if (receiveThread != null && receiveThread.IsAlive)
                receiveThread.Join(100);

            udpClient?.Dispose();
            udpClient = null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"UDP shutdown exception: {e.Message}");
        }
        finally
        {
            cts.Dispose();
            cts = null;
        }
    }
}