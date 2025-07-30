using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class ExportData
{
    public static readonly StringBuilder StringBuilder = new();
    public static string ToCSV<T>(IEnumerable<T> log) where T : class
    {
        if (log == null) return string.Empty;

        IEnumerable<PropertyInfo> props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        if (props.Count() == 0) return string.Empty;

        StringBuilder.Clear();
        StringBuilder.AppendLine(string.Join(",", props.Select(p => p.Name)));
        foreach (var entry in log)
            StringBuilder.AppendLine(string.Join(",", props.Select(p => p.GetValue(entry))));

        return StringBuilder.ToString();
    }

    public static void WriteToFile(string filename, string data)
    {
        string path = Path.Combine(Application.dataPath, $"../Logs/{filename}");
        File.WriteAllText(path, data);
        Debug.Log($"Written to: {Path.GetFullPath(path)}");
    }
}