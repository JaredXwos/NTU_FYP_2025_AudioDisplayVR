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

    public Handler(Action<T> handler, string identifier = "Unknown Parent")
    {
        Handle = handler;
        Identifier = identifier;

        InterfaceRegistry.Register(this);

        foreach (Dispatch dispatcher in InterfaceRegistry<Dispatch>.All) 
            if(Check.GetCompatibleTypes(dispatcher.HandlerType).Contains(GetType())) 
                dispatcher.CompileInvoke(this);
    }

    public void Dispose()
    {
        InterfaceRegistry.Unregister(this);
        foreach (Dispatch dispatcher in InterfaceRegistry<Dispatch>.All) dispatcher.DeleteInvoke(this);
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