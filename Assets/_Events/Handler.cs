using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public interface IHas<out T>
{
    public T Handler { get; }
}

public class Handler<T> : IDisposable
{
    public Action<T> Handle { get; }
    public string Identifier { get; }
    
    private readonly Type handlerRegistryType;

    public Handler(Action<T> handler, string identifier = "Unknown Parent")
    {
        Handle = handler;
        Identifier = identifier;

        handlerRegistryType = typeof(InterfaceRegistry<>)
            .MakeGenericType(GetType());

        handlerRegistryType
            .GetMethod("Register", BindingFlags.Public | BindingFlags.Static)
            .Invoke(null, new object[] { this });

        IReadOnlyCollection<Dispatch> allDispatchers = InterfaceRegistry<Dispatch>.All;

        foreach (Dispatch dispatcher in allDispatchers) 
            if(Check.GetCompatibleTypes(dispatcher.HandlerType).Contains(GetType())) 
                dispatcher.CompileInvoke(this);
    }

    public void Dispose()
    {
        handlerRegistryType
            .GetMethod("Unregister", BindingFlags.Public | BindingFlags.Static)
            .Invoke(null, new object[] { this });

        // Now tell all dispatchers to remove this handler
        IReadOnlyCollection<Dispatch> allDispatchers = InterfaceRegistry<Dispatch>.All;

        foreach (Dispatch dispatcher in allDispatchers) dispatcher.DeleteInvoke(this);
    }
}

public abstract class EventCascade<HANDLER, Payload, Component> : MonoBehaviour, IHas<HANDLER>
    where HANDLER : Handler<Payload>
{
    public HANDLER handler;
    public HANDLER Handler => handler;

    protected virtual void Awake() => handler = (HANDLER) Activator.CreateInstance(
        typeof(HANDLER), 
        (Action<Payload>) (p => {
            if (Condition(p)) 
                foreach (Component component in GetComponentsInChildren<Component>()) 
                    Dispatch(component);
        }),
        $"{GetType()} on {gameObject.name}"
    );

    protected abstract void Dispatch(Component component);
    protected virtual bool Condition(Payload payload) => true;
}