using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public static class Check
{
    /// <summary>
    /// Checks if the GameObject has any components of type T.
    /// If it does, logs a warning and disables the caller.
    /// Returns true if it found any (i.e. if it disabled the caller).
    /// </summary>
    public static bool ForLocalComponentAndDisable<T>(MonoBehaviour caller)
    {
        if (caller.GetComponents<T>().Where(t => !ReferenceEquals(t, caller) && t is MonoBehaviour mb && mb.enabled).Count() > 0)
        {
            Debug.LogWarning($"[Check] {caller.GetType().Name} on '{caller.gameObject.name}' found other instance(s) of {typeof(T).Name}, disabling itself.", caller);
            caller.enabled = false;
            return true;
        }
        return false;
    }

    public static bool PropertyEnabledElseAssign<T>(MonoBehaviour caller, string propertyName)
    {
        Type callerType = caller.GetType();
        FieldInfo field = callerType.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        PropertyInfo prop = callerType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        bool isField;
        T currentValue;
        if (field != null)
        {
            if (!typeof(T).IsAssignableFrom(field.FieldType))
            {
                Debug.LogError($"[Check] Field '{propertyName}' on {callerType.Name} is of type {field.FieldType}, which does not implement or inherit {typeof(T)}");
                return false;
            }
            else
            {
                currentValue = (T)field.GetValue(caller);
                isField = true;
            }
        }
        else if (prop != null && prop.CanRead && prop.CanWrite)
        {
            if (!typeof(T).IsAssignableFrom(prop.PropertyType))
            {
                Debug.LogError($"[Check] Property '{propertyName}' on {callerType.Name} is of type {prop.PropertyType}, which does not implement or inherit {typeof(T)}");
                return false;
            }
            else
            {
                currentValue = (T)prop.GetValue(caller);
                isField = false;
            }
        }
        else
        {
            Debug.LogError($"[Check] Could not find field or property '{propertyName}' on {callerType.Name}");
            return false;
        }

        if (currentValue is MonoBehaviour mb && mb.enabled) return true;

        MonoBehaviour localEnabledValue = caller
            .GetComponents<MonoBehaviour>()
            .FirstOrDefault(c => c is T && c.enabled);

        if (localEnabledValue != null)
        {
            if(isField) field.SetValue(caller, localEnabledValue);
            else prop.SetValue(caller, localEnabledValue);
            return true;
        }

        if(currentValue != null)
        {
            ((MonoBehaviour)(object) currentValue).enabled = true;
            return true;
        }

        localEnabledValue = caller
            .GetComponents<MonoBehaviour>()
            .FirstOrDefault(c => c is T);

        if (localEnabledValue != null)
        {
            localEnabledValue.enabled = true;
            if (isField) field.SetValue(caller, localEnabledValue);
            else prop.SetValue(caller, localEnabledValue);
            return true;
        }

        MonoBehaviour globalEnabledValue = UnityEngine.Object
            .FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .FirstOrDefault(c => c is T && c.enabled);

        if (globalEnabledValue != null)
        {
            if (isField) field.SetValue(caller, globalEnabledValue);
            else prop.SetValue(caller, globalEnabledValue);
            return true;
        }

        globalEnabledValue = caller
            .GetComponents<MonoBehaviour>()
            .FirstOrDefault(c => c is T);

        if (globalEnabledValue != null)
        {
            globalEnabledValue.enabled = true;
            if (isField) field.SetValue(caller, globalEnabledValue);
            else prop.SetValue(caller, globalEnabledValue);
            return true;
        }

        Debug.LogError($"[Check] Could not find any {callerType.Name}");
        return false;
    }

    private static readonly Dictionary<Type, HashSet<Type>> SubsetTypes = new()
    {
        [typeof(object)] = new()
    };
    private static readonly HashSet<Type> Viewed = new();

    public static HashSet<Type> GetCompatibleTypes(Type type)
    {
        if (type == null) return new();

        HashSet<Type> result = new();

        if (SubsetTypes.TryGetValue(type, out var cached))
        {
            cached.Add(type);
            return cached;
        }
        if (Viewed.Contains(type)) return new();
        Viewed.Add(type);
        // --- GENERIC HANDLING ---
        if (type.IsGenericType)
        {
            // First, compute independent sets without mutation
            HashSet<Type>[] argSets = type.GetGenericArguments()
                .Where(arg => arg != null)
                .Select(arg => GetCompatibleTypes(arg))
                .ToArray();

            // Then compute cartesian product and mutate safely
            foreach (IEnumerable<Type> combo in CartesianProduct(argSets))
            {
                try
                {
                    var constructed = type.GetGenericTypeDefinition().MakeGenericType(combo.ToArray());
                    result.Add(constructed);
                }
                catch { }
            }
        }

        // --- BASE TYPE ---
            result.UnionWith(GetCompatibleTypes(type.BaseType));

        // --- INTERFACES ---
        foreach (var i in type.GetInterfaces())
            if (i != type)
                result.UnionWith(GetCompatibleTypes(i));
            
        SubsetTypes[type] = result;

        result.Add(type);
        return result;
    }

    // Helper: computes cartesian product of sets
    private static IEnumerable<IEnumerable<Type>> CartesianProduct(IEnumerable<HashSet<Type>> sequences)
    {
        IEnumerable<IEnumerable<Type>> result = new[] { Enumerable.Empty<Type>() };
        foreach (var sequence in sequences)
        {
            result = from accseq in result
                     from item in sequence
                     select accseq.Concat(new[] { item });
        }
        return result;
    }

    public static Expression BuildCompatibleNewInstance(Expression parameter, Type sourceType, Type targetType)
    {
        // If they are directly the same, no rebuild needed
        if (sourceType == targetType)
            return parameter;

        // If they are assignable directly (upcasting, interfaces)
        if (targetType.IsAssignableFrom(sourceType))
            return Expression.Convert(parameter, targetType);

        // If target type is generic, try to rebuild recursively
        if (targetType.IsGenericType && sourceType.IsGenericType)
        {
            var sourceArgs = sourceType.GetGenericArguments();
            var targetArgs = targetType.GetGenericArguments();

            if (sourceArgs.Length != targetArgs.Length)
                throw new InvalidOperationException($"Cannot convert: {sourceType} and {targetType} have different generic arity.");

            var rebuiltArgs = new List<Expression>();

            for (int i = 0; i < sourceArgs.Length; i++)
            {
                var fieldOrPropName = $"Item{i + 1}";
                MemberExpression innerSource;

                // Try to access as field first (like ValueTuple), then as property fallback
                var field = sourceType.GetField(fieldOrPropName);
                if (field != null)
                    innerSource = Expression.Field(parameter, fieldOrPropName);
                else
                {
                    var prop = sourceType.GetProperty(fieldOrPropName);
                    if (prop != null)
                        innerSource = Expression.Property(parameter, fieldOrPropName);
                    else
                        throw new InvalidOperationException($"No Item{i + 1} found on {sourceType}");
                }

                // Recurse for each generic argument
                rebuiltArgs.Add(BuildCompatibleNewInstance(innerSource, sourceArgs[i], targetArgs[i]));
            }

            var ctor = targetType.GetConstructor(targetArgs)
                ?? throw new InvalidOperationException($"No suitable constructor for {targetType}.");

            return Expression.New(ctor, rebuiltArgs);
        }

        // Last fallback: try direct convert for primitive or non-generic
        var compatibleTypes = GetCompatibleTypes(sourceType);
        if (compatibleTypes.Contains(targetType) || targetType.IsAssignableFrom(sourceType))
            return Expression.Convert(parameter, targetType);

        throw new InvalidOperationException($"Cannot build expression to convert from {sourceType} to {targetType}.");
    }
}