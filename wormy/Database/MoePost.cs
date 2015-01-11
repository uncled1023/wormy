using System;
using FluentNHibernate.Mapping;
using System.Collections.Generic;

namespace wormy.Database
{
    public class MoePost
    {
        public MoePost()
        {
            SavedUsers = new List<ChannelUser>();
        }

        public virtual int Id { get; set; }
        public virtual WormyChannel Channel { get; set; }
        public virtual string RedditId { get; set; }
        public virtual string SearchTerms { get; set; }
        public virtual IList<ChannelUser> SavedUsers { get; protected set; }

        public class Mapping : ClassMap<MoePost>
        {
            public Mapping()
            {
                Id(m => m.Id);
                Map(m => m.RedditId);
                Map(m => m.SearchTerms).Nullable();
                References<WormyChannel>(m => m.Channel).Nullable();
                HasManyToMany(m => m.SavedUsers)
                    .Inverse()
                    .Table("UserMoe");
            }
        }
    }
}