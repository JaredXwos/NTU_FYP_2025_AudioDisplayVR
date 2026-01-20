using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entry { }
public interface IHasDummy<T> where T : Entry
{
    static T Dummy { get; }
}
public class LogBook
{
    private readonly List<Entry> entries = new();
    public readonly Type EntryType;

    public LogBook(Entry init)
        => EntryType = init.GetType();

    public void AddEntry(Entry entry)
    {
        if(entry.GetType() != EntryType)
            throw new ArgumentException($"LogBook only accepts entries of type {EntryType.Name}, but got {entry.GetType().Name}.");
        entries.Add(entry);
    }

    public IReadOnlyList<Entry> Get => entries.AsReadOnly();
    public int Count => entries.Count;
}

public abstract class Logger : MonoBehaviour
{
    public abstract IReadOnlyList<Entry> LogBook { get; }
}