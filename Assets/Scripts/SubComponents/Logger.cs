using System.Collections.Generic;
using UnityEngine;

public record TiltRecord(float Time, float X, float Y, float Z);

public class Logger : MonoBehaviour
{
    [SerializeField] private string LogName;
    protected ScaleBalance ScaleBalance;
    protected List<TiltRecord> Log = new();

    protected virtual void Awake()
    {
        Check.PropertyEnabledElseAssign<ScaleBalance>(this, "ScaleBalance");
    }
    protected virtual void FixedUpdate()
    {
        Vector3 Orientation = ScaleBalance.Orientation;
        Log.Add(new TiltRecord(Time.time, Orientation.x, Orientation.y, Orientation.z));
    }
    protected virtual void OnDestroy()
    {
        ExportData.WriteToFile($"{LogName} Tilt Log.csv", ExportData.ToCSV(Log));
    }
}