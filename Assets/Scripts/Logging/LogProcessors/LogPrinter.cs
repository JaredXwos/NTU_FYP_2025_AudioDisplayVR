using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class LogPrinter : MonoBehaviour
{
    [SerializeField] private Logger[] loggers;

    private void OnApplicationQuit()
    {
        foreach (Logger logger in loggers)
        if (logger.LogBook.Count > 0)
            PrintToFile(
                Path.Combine(
                    Application.persistentDataPath,
                    $"../Logs/{logger.GetType().Name}-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{UpdateTrialName.TrialName}.csv"),
                logger.LogBook
            );
        else Debug.Log($"No log entries to print for {logger.GetType().Name}");
    }

    public void PrintToFile(string path, IReadOnlyList<Entry> logbook)
    {
        if (logbook == null || logbook.Count == 0)
        {
            File.WriteAllText(path, "# Empty log\n");
            return;
        }
        Type EntryType = logbook[0].GetType();
        IOrderedEnumerable<FieldInfo> fields = EntryType
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(f => f.MetadataToken);

        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory); 
        using StreamWriter sw = new(path);
        sw.WriteLine($"# {EntryType.Name} log generated {DateTime.Now}");
        sw.WriteLine(string.Join(",", fields.Select(p => p.Name)));

        foreach (Entry entry in logbook)
            sw.WriteLine(string.Join(",", fields.Select(p => ToCsvSafe(p.GetValue(entry)))));
    }

    private static string ToCsvSafe(object val)
    {
        if (val == null) return "";
        string s = val.ToString() ?? "";
        if (s.Contains(',') || s.Contains('"'))
            s = $"\"{s.Replace("\"", "\"\"")}\"";
        return s;
    }
}