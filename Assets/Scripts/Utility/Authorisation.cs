using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public interface IRequireAuthorisation<T>
{
    public object Key { set; }
}

public class LimitedClass : MonoBehaviour
{
    private readonly object key = new();
    protected virtual void Awake()
    {
        HashSet<Type> compatibleRequestorTypes = Check.GetCompatibleTypes(
            typeof(IRequireAuthorisation<>)
                .MakeGenericType(GetType())
        );

        foreach (Type iface in GetType().GetInterfaces())
            compatibleRequestorTypes.UnionWith( Check.GetCompatibleTypes(
                typeof(IRequireAuthorisation<>)
                .MakeGenericType(iface))
            );
        

        foreach (MonoBehaviour monobehaviour 
            in GetComponents<MonoBehaviour>()
            .Where(
                t => t != null && t
                .GetType()
                .GetInterfaces()
                .Any(i => compatibleRequestorTypes.Contains(i))
            ))
        {
            PropertyInfo Key = monobehaviour.GetType().GetProperty("Key");
            if (Key != null && Key.CanWrite)
                Key.SetValue(monobehaviour, key);
            
        }
    }

    protected void Verify(object Key)
    {
        if (!ReferenceEquals(Key, key))
            throw new InvalidOperationException($"[{(!this ? "Unknown" : gameObject)}] Caller lacks required authentication");
    }
}