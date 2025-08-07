using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class World
{
    private static readonly Dictionary<Vector3Int, CoreComponent> registry = new();
    private static readonly Dictionary<CoreComponent, HashSet<Vector3Int>> inverseRegistry = new();

    public static bool CheckCollision(Vector3Int position)
        => registry.ContainsKey(position);

    public static bool CheckCollision(Vector3Int position, out CoreComponent collided)
        => registry.TryGetValue(position, out collided);

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
    public static void ForceRegister(Vector3Int position, CoreComponent component)
        => ForceRegister(new[] { position }, component);

    public static void ForceRegister(IEnumerable<Vector3Int> positions, CoreComponent component)
    {
        foreach (var pos in positions)
            registry[pos] = component;

        if (inverseRegistry.ContainsKey(component))
            inverseRegistry[component].UnionWith(positions);
        else inverseRegistry[component] = positions.ToHashSet();
    }

    public static bool TryRegister(Vector3Int position, CoreComponent component)
    {
        if (registry.ContainsKey(position)) return false;

        ForceRegister(position, component);

        return true;
    }

    public static bool TryRegister(IEnumerable<Vector3Int> positions, CoreComponent component)
    {
        if (CheckCollision(positions)) return false;

        ForceRegister(positions, component);

        return true;
    }
    public static bool TryRegister(IEnumerable<Vector3Int> positions, CoreComponent component, out Dictionary<Vector3Int, CoreComponent> collided)
    {
        if (CheckCollision(positions, out collided)) return false;

        ForceRegister(positions, component);

        return true;
    }

    public static void ForceDeregister(Vector3Int position)
    {
        if (!registry.TryGetValue(position, out CoreComponent component)) return;
        inverseRegistry[component].Remove(position);
        registry.Remove(position);
    }

    public static void ForceDeregister(IEnumerable<Vector3Int> positions)
    {
        foreach (var pos in positions)
            ForceDeregister(pos);
    }

    public static void ForceDeregister(CoreComponent component)
    {
        if (!inverseRegistry.TryGetValue(component, out HashSet<Vector3Int> positions)) return;
        foreach (var pos in positions)
            registry.Remove(pos);
        inverseRegistry.Remove(component);
    }
}