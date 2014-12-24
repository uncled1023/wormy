using System;
using FluentNHibernate.Mapping;

namespace wormy.Database
{
    public class MoePost
    {
        public virtual int Id { get; set; }
        public virtual WormyChannel Channel { get; set; }
        public virtual string RedditId { get; set; }
        public virtual string SearchTerms { get; set; }

        public class Mapping : ClassMap<MoePost>
        {
            public Mapping()
            {
                Id(m => m.Id);
                Map(m => m.RedditId);
                Map(m => m.SearchTerms).Nullable();
                References<WormyChannel>(m => m.Channel).Nullable();
            }
        }
    }
}