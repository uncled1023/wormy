using System;
using FluentNHibernate.Mapping;

namespace wormy.Database
{
    public class Reminder
    {
        public virtual int Id { get; protected set; }
        public virtual string Action { get; set; }
        public virtual DateTime DueDate { get; set; }
        public virtual string Target { get; set; }
        public virtual string Source { get; set; }

        public class Mapping : ClassMap<Reminder>
        {
            public Mapping()
            {
                Id(mn => mn.Id);
                Map(mn => mn.Action);
                Map(mn => mn.DueDate);
                Map(mn => mn.Target);
                Map(mn => mn.Source);
            }
        }
    }
}