// ManagedReferenceAttribute.cs
using System;
using UnityEngine;

/// Put this on a [SerializeReference] field to tell the drawer which base/interface to list.
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ManagedReferenceAttribute : PropertyAttribute
{
    public Type BaseType { get; }
    public ManagedReferenceAttribute(Type baseType) => BaseType = baseType;
}