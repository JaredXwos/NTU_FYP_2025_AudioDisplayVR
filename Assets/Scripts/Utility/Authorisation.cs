using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public interface IRequireAuthorisation<T>
{
    public object Key { set; }
}

public class Auth
{
    private readonly MonoBehaviour Host;
    private readonly HashSet<Type> CompatibleRequestorTypes;
    private readonly object key = new();
    public Auth(MonoBehaviour host)
    {
        Host = host; 
        CompatibleRequestorTypes = Check.GetCompatibleTypes(
            typeof(IRequireAuthorisation<>)
                .MakeGenericType(Host.GetType())
        );
        Authenticate();
    }
    public void Authenticate()
    {
        foreach (MonoBehaviour monobehaviour
            in Host.GetComponents<MonoBehaviour>()
            .Where(
                t => t != null && t
                .GetType()
                .GetInterfaces()
                .Any(i => CompatibleRequestorTypes.Contains(i))
            ))
        {
            PropertyInfo Key = monobehaviour.GetType().GetProperty("Key");
            if (Key != null && Key.CanWrite)
                Key.SetValue(monobehaviour, key);
        }
    }
    public void Verify(object Key)
    {
        if (!ReferenceEquals(Key, key))
            throw new InvalidOperationException($"[{(!Host ? "Unknown" : Host.gameObject.name)}] Caller lacks required authentication");
    }
}

public interface ILimitedAccess
{
    public void Authenticate();
    protected Auth Auth { get; }
}