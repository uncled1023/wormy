using System;

namespace wormy
{
    public class DependsAttribute : Attribute
    {
        public Type DependsOn { get; set; }

        public DependsAttribute(Type dependsOn)
        {
            DependsOn = dependsOn;
        }
    }
}