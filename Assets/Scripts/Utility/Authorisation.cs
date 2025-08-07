using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public interface IRequireAuthorisation<T>
{
    public object Key { set; }
}

public interface ILimitedAccess
{
    public void Authenticate();
    protected Auth Auth { get; }
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
        if (!ReferenceEquals(Key, key) && Key is not Admin.RightsHolder)
            throw new InvalidOperationException($"[{(!Host ? "Unknown" : Host.gameObject.name)}] Caller lacks required authentication");
    }
}
    
public abstract class Admin : MonoBehaviour
{
    private static readonly object Rights = new();
    protected static RightsHolder Key => new ExclusiveRightsHolder();
    public abstract class RightsHolder
    {
        protected readonly object HeldRights;
        protected RightsHolder() => throw new InvalidOperationException("No default constructor");
        protected RightsHolder(object HeldRights)
        {
            this.HeldRights = HeldRights;
            if (HeldRights != Rights) throw new InvalidOperationException("Not the valid rights holder");
        }
    }
    private sealed class ExclusiveRightsHolder : RightsHolder
    {
        public ExclusiveRightsHolder() : base(Rights) { }
    }
}