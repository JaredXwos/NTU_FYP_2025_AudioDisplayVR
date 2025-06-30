using System.Collections.Generic;

public static class InterfaceRegistry<T>
{
    private static readonly HashSet<T> register = new();

    public static void Register(T obj) => register.Add(obj);
    public static void Unregister(T obj) => register.Remove(obj);
    public static IReadOnlyCollection<T> All => register;
    public static bool IsRegistered(T obj) => register.Contains(obj);
}