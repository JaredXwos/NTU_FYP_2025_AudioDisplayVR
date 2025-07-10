using System;
using System.IO;
using System.Linq;
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
}