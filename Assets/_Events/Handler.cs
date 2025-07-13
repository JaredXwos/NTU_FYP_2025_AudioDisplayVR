using System;
using System.Collections.Generic;
using UnityEngine;

public interface IHas<out T>
{
    public T Handler { get; }
}

public class Handler<T>
{
    public Action<T> Handle { get; }
    public Handler(Action<T> handler) => Handle = handler;
}

public abstract class EventCascade<HANDLER, Payload, Component> : MonoBehaviour, IHas<HANDLER>
    where HANDLER : Handler<Payload>
{
    public HANDLER Handler => (HANDLER)Activator.CreateInstance(typeof(HANDLER), (Action<Payload>)(p => {
        if (Condition(p)) foreach (Component component in GetComponentsInChildren<Component>()) Dispatch(component);
    }));

    protected abstract void Dispatch(Component component);
    protected virtual bool Condition(Payload payload) => true;
}