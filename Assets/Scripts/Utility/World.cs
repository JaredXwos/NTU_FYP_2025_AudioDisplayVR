using System.Collections.Generic;
using UnityEngine;

public static class World
{
    private static readonly Dictionary<Vector3Int, CoreComponent> registry = new();
    public static bool TryRegister(Vector3Int position, CoreComponent component)
        => registry.TryAdd(position, component);
    public static bool TryRegister(IEnumerable<Vector3Int> positions, CoreComponent component)
    {
        if (CheckCollision(positions)) return false;
        foreach (var pos in positions)
            registry[pos] = component;
        return true;
    }
    public static bool TryRegister(IEnumerable<Vector3Int> positions, CoreComponent component, out Dictionary<Vector3Int, CoreComponent> collided)
    {
        if (CheckCollision(positions, out collided)) return false;
        foreach (var pos in positions)
            registry[pos] = component;
        return true;
    }
    public static void Deregister(Vector3Int position)
    {
        if (!registry.Remove(position))
            Debug.LogWarning($"Attempted to deregister at position {position} but no component was found.");
    }
    public static bool TryDeregister(IEnumerable<Vector3Int> positions, CoreComponent component)
    {
        foreach (var pos in positions)
        {
            if(!registry.TryGetValue(pos, out CoreComponent existingComponent))
            {
                Debug.LogWarning($"Attempted to deregister at position {pos} but no component was found.");
                return false;
            }
            if (existingComponent != component)
            {
                Debug.LogWarning($"Attempted to deregister {component.name} at position {pos}, but found {existingComponent} instead.");
                return false;
            }
        }
        foreach (var pos in positions)
            registry.Remove(pos);
        return true;
    }
    public static bool CheckCollision(IEnumerable<Vector3Int> positions)
    {
        foreach (var pos in positions)
            if (registry.ContainsKey(pos)) return true;
        return false;
    }
    public static bool CheckCollision(IEnumerable<Vector3Int> positions, out Dictionary<Vector3Int, CoreComponent> collided)
    {
        collided = new();
        foreach (var pos in positions)
            if (registry.TryGetValue(pos, out CoreComponent component)) collided[pos] = component;
        return collided.Count != 0;
    }
}