using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

            var compatibleTypes = Check.GetCompatibleTypes(typeof(IHas<>).MakeGenericType(HandlerType));
            listeners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(m => m.GetType()
                             .GetInterfaces()
                             .Any(i => compatibleTypes.Contains(i)))
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
                HashSet<Type> compatibleTypes = Check.GetCompatibleTypes(typeof(IHas<>).MakeGenericType(HandlerType));
                if (m.GetType().GetInterfaces().Any(i => compatibleTypes.Contains(i))) newlist.Add(m);
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
            object handler = null;

            // Try direct property on class hierarchy
            var prop = m.GetType().GetProperty("Handler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

            if (prop != null) handler = prop.GetValue(m);
            else
            {
                // Fallback: look for any IHas<...> explicit interface
                var iface = m.GetType()
                    .GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IHas<>)
                    ) ?? throw new InvalidOperationException($"{m} does not implement IHas<> or have a Handler property.");
                handler = iface.InvokeMember(
                    "Handler",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                    null,
                    m,
                    null
                );
            }

            if (handler == null)
                throw new InvalidOperationException($"Handler on {m} is null.");

            var handleDelegate = handler.GetType().GetProperty("Handle").GetValue(handler)
            ?? throw new InvalidOperationException("Handler lacks the property Handle");

            var delegateType = handleDelegate.GetType();
            var invokeMethod = delegateType.GetMethod("Invoke");
            var expectedParameterType = invokeMethod.GetParameters()[0].ParameterType;

            var delegateConst = Expression.Constant(handleDelegate);

            InvocationExpression invokeCall = Expression.Invoke(delegateConst, Check.BuildCompatibleNewInstance(parameter, parameter.Type, expectedParameterType));

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