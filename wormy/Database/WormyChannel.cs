using System;
using FluentNHibernate.Mapping;
using System.Collections.Generic;

namespace wormy.Database
{
    public class WormyChannel
    {
        public WormyChannel()
        {
            Users = new List<ChannelUser>();
        }

        public virtual int Id { get; protected set; }
        public virtual string Network { get; set; }
        public virtual string Name { get; set; }
        public virtual string Key { get; set; }
        public virtual string CommandPrefix { get; set; }
        public virtual IList<ChannelUser> Users { get; protected set; }

        public class Mapping : ClassMap<WormyChannel>
        {
            public Mapping()
            {
                Id(m => m.Id);
                Map(m => m.Name);
                Map(m => m.Key);
                Map(m => m.Network);
                Map(m => m.CommandPrefix);
                HasManyToMany(m => m.Users)
                    .Inverse()
                    .Table("UserChannel");
            }
        }
    }
}