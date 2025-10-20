using System;
using UnityEngine;

public class UDPDroneInput : UdpReceiver, IDroneInputInterface
{
    private Volatile<double> phi = new(), theta = new(), psi = new(), x = new(), y = new(), z = new();

    public double Phi => phi.Value;
    public double Theta => theta.Value;
    public double Psi => psi.Value;
    public double X => x.Value;
    public double Y => y.Value;
    public double Z => z.Value;

    protected override void OnReceive(byte[] data)
    {
        if (data.Length == 48) // 6 doubles
        {
            phi.Value = BitConverter.ToDouble(data, 0);
            theta.Value = BitConverter.ToDouble(data, 8);
            psi.Value = BitConverter.ToDouble(data, 16);
            x.Value = BitConverter.ToDouble(data, 24);
            y.Value = BitConverter.ToDouble(data, 32);
            z.Value = BitConverter.ToDouble(data, 40);
        }
        else
        {
            Debug.LogWarning($"Unexpected packet size: {data.Length}");
        }
    }
}