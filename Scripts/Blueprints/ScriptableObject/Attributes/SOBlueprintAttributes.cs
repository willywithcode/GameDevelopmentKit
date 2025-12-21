namespace GameFoundation.Scripts.Blueprints.ScriptableObject.Attributes
{
    using System;
    [AttributeUsage(AttributeTargets.Class)]
    public class SOBlueprintAttributes : Attribute
    {
        public string BlueprintName { get; }
        public SOBlueprintAttributes(string blueprintName)
        {
            this.BlueprintName = blueprintName;
        }
    }

}