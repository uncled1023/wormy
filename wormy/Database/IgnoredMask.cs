using System;
using FluentNHibernate.Mapping;

namespace wormy.Database
{
    public class IgnoredMask
    {
        public virtual int Id { get; protected set; }
        public virtual string Mask { get; set; }
        public virtual Channel Channel { get; set; }

        public class Mapping : ClassMap<IgnoredMask>
        {
            public Mapping()
            {
                Id(mn => mn.Id);
                Map(mn => mn.Mask);
                References<Channel>(mn => mn.Channel);
            }
        }
    }
}