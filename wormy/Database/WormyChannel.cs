using System;
using FluentNHibernate.Mapping;

namespace wormy.Database
{
    public class WormyChannel
    {
        public virtual int Id { get; set; }
        public virtual string Network { get; set; }
        public virtual string Name { get; set; }
        public virtual string Key { get; set; }
        public virtual string CommandPrefix { get; set; }

        public class Mapping : ClassMap<WormyChannel>
        {
            public Mapping()
            {
                Id(m => m.Id);
                Map(m => m.Name);
                Map(m => m.Key);
                Map(m => m.Network);
                Map(m => m.CommandPrefix);
            }
        }
    }
}