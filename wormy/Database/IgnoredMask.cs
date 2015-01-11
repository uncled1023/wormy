using System;
using FluentNHibernate.Mapping;

namespace wormy.Database
{
    public class IgnoredMask
    {
        public virtual int Id { get; protected set; }
        public virtual string Mask { get; set; }
        public virtual WormyChannel Channel { get; set; }

        public class Mapping : ClassMap<IgnoredMask>
        {
            public Mapping()
            {
                Id(mn => mn.Id);
                Map(mn => mn.Mask);
                References<WormyChannel>(mn => mn.Channel);
            }
        }
    }
}