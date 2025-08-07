using System;
using System.Linq;
using UnityEngine;

public interface ILogCreator
{
    void SetLogName(string name);
}

public class LogNameSetter : MonoBehaviour
{
    [SerializeField] private string LogName = string.Empty;

    private void Awake()
    {
        if(LogName == string.Empty) LogName = DateTime.UtcNow.ToString("D_HHmmss");
        foreach(ILogCreator creator in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(Mb => Mb is ILogCreator).Cast<ILogCreator>())
            creator.SetLogName(LogName);
    }

}