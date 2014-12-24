using System;

namespace wormy
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependsAttribute : Attribute
    {
        public Type DependsOn { get; set; }

        public DependsAttribute(Type dependsOn)
        {
            DependsOn = dependsOn;
        }
    }
}