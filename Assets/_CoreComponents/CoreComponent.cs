using System.Linq;
using System.Reflection;
using UnityEngine;

public interface IRequireInfo<T> { }

public abstract class CoreComponent : MonoBehaviour
{
    protected virtual void Awake()
    {
        if(GetComponents<CoreComponent>().Where(c => c.enabled).Count() > 1)
        {
            Debug.LogWarning($"[{name}]: More than 1 enabled core component detected. Disabling.");
            enabled = false;
        }
        LinkComponents();
    }
    protected abstract (string name, System.Func<object> binding)[] Bindings { get; }

    private void LinkComponents()
    {
        System.Type componentType = typeof(IRequireInfo<>).MakeGenericType(GetType());
        foreach (var (name, binding) in Bindings)
        {
            foreach (var component in GetComponents<MonoBehaviour>())
                if (component.GetType().GetInterfaces().Any(i => Check.GetCompatibleTypes(componentType).Contains(i)))
            {
                PropertyInfo prop = component
                    .GetType()
                    .GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (prop != null && prop.CanWrite && prop.PropertyType.IsAssignableFrom(binding().GetType()))
                    prop.SetValue(component, binding());
                
            }
        }
    }
}