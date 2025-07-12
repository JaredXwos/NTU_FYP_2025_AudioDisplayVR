using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public abstract class Dispatch : MonoBehaviour
{
    [SerializeField] MonoBehaviour[] listeners;

    protected abstract Type HandlerType { get; set; }
    protected abstract Type PayloadType { get; set; }

    private Action<object>[] invokes;

    protected virtual void Awake()
    {
        if(!
            typeof(Handler<>).MakeGenericType(PayloadType)
            .IsAssignableFrom(HandlerType))
            throw new InvalidOperationException($"Handler type {HandlerType} is not compatible with Handler<{PayloadType}>");
    }

    protected virtual void Start()
    {
        if (listeners == null || listeners.Length < 1)
        {
            Debug.Log($"[Dispatch::{GetType().Name}] No listeners found. Assuming global broadcast");
            listeners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(m => typeof(IHas<>).MakeGenericType(HandlerType).IsInstanceOfType(m))
            .ToArray();
            if (listeners.Length == 0)
            {
                Debug.LogWarning($"[Dispatch::{GetType().Name}] No valid listeners found. Disabling.");
                enabled = false;
                return;
            }
        }
        else
        {
            List<MonoBehaviour> newlist = new();
            foreach(MonoBehaviour m in listeners)
            {
                if(typeof(IHas<>).MakeGenericType(HandlerType).IsInstanceOfType(m)) newlist.Add(m);
                else Debug.LogWarning($"[Dispatch::{GetType().Name}] The provided listener {m.name} on {m.gameObject.name} does not have the required event handler {HandlerType.Name}.");
            }

            if (newlist.Count > 0) listeners = newlist.ToArray();
            else
            {
                Debug.LogWarning($"[Dispatch::{GetType().Name}] There are no valid listeners provided. Disabling.");
                enabled = false;
                return;
            }
        }
        ParameterExpression argument = Expression.Parameter(typeof(object), "payload");
        UnaryExpression parameter = Expression.Convert(argument, PayloadType);
        invokes = listeners.Select(m =>
        {
            var handler = typeof(IHas<>).MakeGenericType(HandlerType).GetProperty("Handler").GetValue(m) ?? throw new InvalidOperationException("MonoBehaviour lacks the property Handler");
            var handleDelegate = HandlerType.GetProperty("Handle").GetValue(handler) ?? throw new InvalidOperationException("Handler lacks the property Handle");
            ConstantExpression delegateConst = Expression.Constant(handleDelegate);
            InvocationExpression invokeCall = Expression.Invoke(delegateConst, parameter);
            return Expression.Lambda<Action<object>>(invokeCall, argument).Compile();
        }).ToArray();

    }

    protected void Invoke(object payload)
    {
        if (PayloadType.IsInstanceOfType(payload))
            foreach (Action<object> action in invokes) action(payload);
        else throw new InvalidCastException($"[Dispatch::{GetType().Name}] Expected payload of type {PayloadType}, but got {payload?.GetType()}");
    }
}