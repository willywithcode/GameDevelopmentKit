namespace GameFoundation.Scripts.Patterns.MVP.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class ViewAttribute : Attribute
    {
        public string PrefabPath { get; }

        public ViewAttribute(string prefabPath)
        {
            this.PrefabPath = prefabPath;
        }
    }
}