namespace GameFoundation.Scripts.Patterns.MVP.Attribute
{
    using System;
    [AttributeUsage(AttributeTargets.Class)]
    public class PresenterAttribute : System.Attribute
    {
        public bool IsSingleton { get; }
        public bool AutoInit { get; }
        
        public PresenterAttribute(bool isSingleton = false, bool autoInit = false)
        {
            this.IsSingleton = isSingleton;
            this.AutoInit = autoInit;
        }
    }
} 