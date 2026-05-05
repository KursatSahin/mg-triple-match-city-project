using System;
using UnityEngine;

namespace TripleMatch.Core.Attributes
{
    /// <summary>
    /// Routes Inspector edits of the marked field through a callback method. The callback
    /// receives the new value, may mutate sibling fields on the owning instance, and
    /// returns the value that gets written back to this field. Editor-only; runtime
    /// behavior is unaffected.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ValidateFieldAttribute : PropertyAttribute
    {
        public string CallbackName { get; }

        public ValidateFieldAttribute(string callbackName)
        {
            CallbackName = callbackName;
        }
    }
}
