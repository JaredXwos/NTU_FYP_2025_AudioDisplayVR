#if UNITY_EDITOR
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomPropertyDrawer(typeof(ManagedReferenceAttribute))]
public sealed class ManagedReferenceDrawer : PropertyDrawer
{
    private static readonly Dictionary<Type, Type[]> Cache = new();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Let Unity calculate height for the nested object
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        var attr = (ManagedReferenceAttribute)attribute;
        var baseType = attr.BaseType;

        // Header with type dropdown
        var line = new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight);
        var body = new Rect(pos.x, line.yMax + 2f, pos.width, pos.height - line.height - 2f);

        EditorGUI.LabelField(line, label);

        // Current type
        var currentType = GetCurrentType(prop);
        var displayName = currentType != null ? currentType.Name : "(None)";
        var btnRect = new Rect(line.xMax - 180f, line.y, 180f, line.height);
        if (GUI.Button(btnRect, $"Type: {displayName}", EditorStyles.popup))
        {
            var menu = new GenericMenu();
            foreach (var t in GetImplementations(baseType))
            {
                var isOn = t == currentType;
                menu.AddItem(new GUIContent(t.FullName), isOn, () =>
                {
                    SetNewInstance(prop, t);
                });
            }
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("(None)"), currentType == null, () => SetNull(prop));
            menu.ShowAsContext();
        }

        // Draw nested fields for the managed reference
        if (prop.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(body, prop, GUIContent.none, true);
            EditorGUI.indentLevel--;
        }
    }

    private static Type GetCurrentType(SerializedProperty prop)
    {
        var full = prop.managedReferenceFullTypename; // "Assembly Name TypeName"
        if (string.IsNullOrEmpty(full)) return null;
        // Unity gives "AssemblyName TypeFullName"
        var parts = full.Split(' ');
        if (parts.Length != 2) return null;
        var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == parts[0]);
        return asm?.GetType(parts[1]);
    }

    private static void SetNewInstance(SerializedProperty prop, Type t)
    {
        prop.serializedObject.Update();
        prop.managedReferenceValue = Activator.CreateInstance(t);
        prop.serializedObject.ApplyModifiedProperties();
    }

    private static void SetNull(SerializedProperty prop)
    {
        prop.serializedObject.Update();
        prop.managedReferenceValue = null;
        prop.serializedObject.ApplyModifiedProperties();
    }

    private static IEnumerable<Type> GetImplementations(Type baseType)
    {
        if (!Cache.TryGetValue(baseType, out var types))
        {
            types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                {
                    // Ignore Unity editor/runtime internals to speed up
                    var n = a.GetName().Name;
                    return !(n.StartsWith("UnityEngine") || n.StartsWith("UnityEditor") || n.StartsWith("System") || n.StartsWith("mscorlib") || n.StartsWith("netstandard"));
                })
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(tt => tt != null)!; }
                })
                .Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.FullName)
                .ToArray();
            Cache[baseType] = types;
        }
        return types!;
    }
}
#endif