using System;
using FluentNHibernate.Mapping;

namespace wormy.Database
{
    public class Channel
    {
        public virtual int ID { get; set; }
        public virtual string Name { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual string Key { get; set; }
        public virtual string CommandPrefix { get; set; }
        public virtual Network Network { get; set; }

        protected Channel() { }

        public Channel(string name, Network network)
        {
            CommandPrefix = ".";
            Network = network;
            Name = name;
            Enabled = true;
        }

        public class Mapping : ClassMap<Channel>
        {
            public Mapping()
            {
                Id(mn => mn.ID);
                Map(mn => mn.Name);
                Map(mn => mn.Key);
                Map(mn => mn.CommandPrefix);
                Map(mn => mn.Enabled);
                References(mn => mn.Network);
            }
        }
    }
}