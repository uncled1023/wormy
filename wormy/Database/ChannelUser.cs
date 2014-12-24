using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace wormy.Database
{
    public class ChannelUser
    {
        public ChannelUser()
        {
            Channels = new List<WormyChannel>();
        }

        public virtual int Id { get; protected set; }
        public virtual IList<WormyChannel> Channels { get; protected set; }
        public virtual string Nick { get; set; }
        public virtual DateTime LastSeen { get; set; }
        public virtual string LastSaid { get; set; }

        public virtual void AddChannel(WormyChannel channel)
        {
            Channels.Add(channel);
            channel.Users.Add(this);
        }

        public class Mapping : ClassMap<ChannelUser>
        {
            public Mapping()
            {
                Id(m => m.Id);
                Map(m => m.Nick);
                Map(m => m.LastSeen);
                Map(m => m.LastSaid);
                HasManyToMany(m => m.Channels)
                    .Cascade.All()
                    .Table("UserChannel");
            }
        }
    }
}