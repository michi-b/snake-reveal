using System;
using JetBrains.Annotations;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property), MeansImplicitUse(ImplicitUseKindFlags.Access)]
    public class UnityEventTargetAttribute : Attribute
    {
        public UnityEventTargetAttribute(string note = null)
        {
            Note = note ?? string.Empty;
        }

        [PublicAPI] public string Note { get; set; }
    }
}