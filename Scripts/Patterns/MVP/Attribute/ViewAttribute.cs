namespace GameFoundation.Scripts.Patterns.MVP.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class ViewAttribute : Attribute
    {
        public string PrefabPath  { get; }
        public int    MaxPoolSize { get; }

        public ViewAttribute(string prefabPath, int maxPoolSize = 5)
        {
            this.PrefabPath  = prefabPath;
            this.MaxPoolSize = maxPoolSize;
        }
    }
}
