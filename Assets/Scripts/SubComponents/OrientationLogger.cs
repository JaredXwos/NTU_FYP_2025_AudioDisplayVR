using System.Collections.Generic;
using UnityEngine;

public record TiltRecord(float Time, float X, float Y, float Z);

public class OrientationLogger : MonoBehaviour, ILogCreator
{
    [SerializeField] private string LogName = string.Empty;
    protected IHasOrientation Level;
    protected List<TiltRecord> Log = new();

    protected virtual void Awake()
    {
        Check.PropertyEnabledElseAssign<IHasOrientation>(this, "Level");
    }
    protected virtual void FixedUpdate()
    {
        Vector3 Orientation = Level.Orientation;
        Log.Add(new TiltRecord(Time.time, Orientation.x, Orientation.y, Orientation.z));
    }
    protected virtual void OnDestroy()
    {
        ExportData.WriteToFile($"{LogName} Tilt Log.csv", ExportData.ToCSV(Log));
    }

    public void SetLogName(string name) => LogName = name;
}