using System;
using FluentNHibernate.Mapping;
using System.Collections.Generic;

namespace wormy.Database
{
    public class OsuAlias
    {
        public virtual int Id { get; set; }
        public virtual WormyChannel Channel { get; set; }
        public virtual string IrcNick { get; set; }
        public virtual string OsuNick { get; set; }

        public class Mapping : ClassMap<OsuAlias>
        {
            public Mapping()
            {
                Id(m => m.Id);
                Map(m => m.IrcNick);
                Map(m => m.OsuNick);
                References<WormyChannel>(m => m.Channel).Nullable();
            }
        }
    }
}