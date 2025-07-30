using System;
using System.Collections.Generic;
using UnityEngine;

public record EventRecord(Type EventType, Type PayloadType, float Timestamp);
public class EveryEventLogger : EventManager
{
    [SerializeField] private string LogName = string.Empty;
    private readonly List<EventRecord> Log = new();
    protected override HashSet<Type> ValidHandlerSignatures => new() { typeof(EventHandler<object, object>)};
    protected override void Manage(Type eventtype, object payload) => 
        Log.Add(new EventRecord(eventtype, payload.GetType(), Time.time));

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ExportData.WriteToFile($"{LogName} Event Log.csv", ExportData.ToCSV(Log));
    }
}