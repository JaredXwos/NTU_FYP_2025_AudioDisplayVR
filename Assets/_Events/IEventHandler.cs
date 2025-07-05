using System;
using System.Collections.Generic;
using UnityEngine;

public interface IHas<out T>
{
    public T Handler { get; }
}

public abstract class Handler<T>
{
    public Action<T> Handle { get; }
    public Handler(Action<T> handler) => Handle = handler;
}

public class EventBroadcaster<Caller, Payload> where Caller : IHas<Handler<Payload>>
{
    private readonly List<Caller> handlers = new();

    public EventBroadcaster(MonoBehaviour[] listeners)
    {
        if (listeners == null) return;

        foreach (MonoBehaviour listener in listeners)
            if (listener is Caller handler) handlers.Add(handler);
            else Debug.LogWarning($"{listener} does not implement IHas<Handler<{typeof(Payload).Name}>> and will be ignored.");
    }

    public void InvokeEvent(Payload payload)
    {
        foreach (Caller caller in handlers)
            caller.Handler.Handle(payload);
    }
}