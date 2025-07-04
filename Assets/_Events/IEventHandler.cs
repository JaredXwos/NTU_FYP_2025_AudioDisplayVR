using System.Collections.Generic;
using UnityEngine;

public interface IHandleEvent<T>
{
    void HandleEvent(T payload);
}

public class EventBroadcaster<Handler, Payload> where Handler : IHandleEvent<Payload>
{
    private readonly List<Handler> handlers = new();

    public EventBroadcaster(MonoBehaviour[] listeners)
    {
        if (listeners == null) return;

        foreach (MonoBehaviour listener in listeners)
            if (listener is Handler handler) handlers.Add(handler);
            else Debug.LogWarning($"{listener} does not implement IHandleEvent<{typeof(Payload).Name}> and will be ignored.");
    }

    public void InvokeEvent(Payload payload)
    {
        foreach (Handler handler in handlers)
            handler.HandleEvent(payload);
    }
}