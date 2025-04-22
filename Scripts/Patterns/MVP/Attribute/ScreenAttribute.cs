namespace GameFoundation.Scripts.Patterns.MVP.Attribute
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class ScreenAttribute : Attribute
    {
        public string nameScreen;

        public ScreenAttribute(string nameScreen)
        {
            this.nameScreen = nameScreen;
        }
    }
}