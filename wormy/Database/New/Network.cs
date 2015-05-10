using System;
using FluentNHibernate.Mapping;
using System.Collections.Generic;

namespace wormy.Database
{
    public class Network
    {
        public virtual int ID { get; set; }
        public virtual string Name { get; set; }
        public virtual string Address { get; set; }

        public virtual string User { get; set; }
        public virtual string Nick { get; set; }
        public virtual string Password { get; set; }
        public virtual string RealName { get; set; }

        public virtual bool NickServ { get; set; }

        public virtual bool Enabled { get; set; }

        public virtual IList<Channel> Channels { get; set; }

        public Network()
        {
            Channels = new List<Channel>();
            Enabled = true;
        }

        public virtual void AddChannel(Channel channel)
        {
            channel.Network = this;
            Channels.Add(channel);
        }

        public class Mapping : ClassMap<Network>
        {
            public Mapping()
            {
                Id(mn => mn.ID);
                Map(mn => mn.Name);
                Map(mn => mn.Address);
                Map(mn => mn.User, "IrcUser");
                Map(mn => mn.Nick);
                Map(mn => mn.Password);
                Map(mn => mn.RealName);
                Map(mn => mn.NickServ);
                Map(mn => mn.Enabled);
                HasMany(mn => mn.Channels).Inverse().Not.LazyLoad();
            }
        }
    }
}