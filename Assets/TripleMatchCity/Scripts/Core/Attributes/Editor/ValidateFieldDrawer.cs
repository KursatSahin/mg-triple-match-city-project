using System;
using System.Collections;
using System.Reflection;
using TripleMatch.Core.Attributes;
using UnityEditor;
using UnityEngine;

namespace TripleMatch.Core.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(ValidateFieldAttribute))]
    public class ValidateFieldDrawer : PropertyDrawer
    {
        const BindingFlags MemberFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label, true);
            
            if (!EditorGUI.EndChangeCheck()) return;

            var attr = (ValidateFieldAttribute)attribute;

            property.serializedObject.ApplyModifiedProperties();

            var owner = ResolveOwner(property);
            
            if (owner == null)
            {
                Debug.LogError($"[ValidateField] Could not resolve owner for '{property.propertyPath}'.");
                return;
            }

            var field = owner.GetType().GetField(property.name, MemberFlags);
            
            if (field == null)
            {
                Debug.LogError($"[ValidateField] Field '{property.name}' not found on '{owner.GetType().Name}'.");
                return;
            }

            var callback = owner.GetType().GetMethod(attr.CallbackName, MemberFlags);
            
            if (callback == null)
            {
                Debug.LogError($"[ValidateField] Callback '{attr.CallbackName}' not found on '{owner.GetType().Name}'.");
                return;
            }

            var parameters = callback.GetParameters();
            
            if (parameters.Length != 1 || parameters[0].ParameterType != field.FieldType)
            {
                Debug.LogError($"[ValidateField] Callback '{attr.CallbackName}' must accept a single parameter of type '{field.FieldType.Name}'.");
                return;
            }

            if (callback.ReturnType != field.FieldType)
            {
                Debug.LogError($"[ValidateField] Callback '{attr.CallbackName}' must return '{field.FieldType.Name}'.");
                return;
            }

            var unityTarget = property.serializedObject.targetObject;
            
            Undo.RecordObject(unityTarget, "Validate Field");

            var currentValue = field.GetValue(owner);
            var result = callback.Invoke(owner, new[] { currentValue });
            
            field.SetValue(owner, result);

            EditorUtility.SetDirty(unityTarget);
            property.serializedObject.Update();
        }

        static object ResolveOwner(SerializedProperty property)
        {
            object current = property.serializedObject.targetObject;
            
            var path = property.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');

            for (int i = 0; i < elements.Length - 1; i++)
            {
                var element = elements[i];
                int bracketIndex = element.IndexOf('[');
                
                if (bracketIndex >= 0)
                {
                    var name = element.Substring(0, bracketIndex);
                    var index = int.Parse(element.Substring(bracketIndex + 1, element.Length - bracketIndex - 2));
                    current = GetIndexedValue(current, name, index);
                }
                else
                {
                    current = GetMemberValue(current, element);
                }

                if (current == null) return null;
            }

            return current;
        }

        static object GetMemberValue(object source, string name)
        {
            if (source == null) return null;
            
            var type = source.GetType();
            var field = type.GetField(name, MemberFlags);
            
            if (field != null) return field.GetValue(source);
            
            var prop = type.GetProperty(name, MemberFlags);
            
            return prop?.GetValue(source);
        }

        static object GetIndexedValue(object source, string name, int index)
        {
            if (GetMemberValue(source, name) is not IEnumerable enumerable) return null;
            
            var enumerator = enumerable.GetEnumerator();
            
            for (int i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext()) return null;
            }
            
            return enumerator.Current;
        }
    }
}