using System;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace wormy.Database
{
    public class WormyDatabase
    {
        public ISessionFactory SessionFactory { get; set; }

        public WormyDatabase(string connectionString)
        {
            // TODO: Allow modules to customize this instead of generating it all here
            // We can probably accomplish this by stopping right before ExposeConfiguration and
            // then passing along the object to the modules.
            var config = PostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString);
            SessionFactory = Fluently.Configure()
                .Database(config)
                .Mappings(m => m.FluentMappings.Add<MoePost.Mapping>())
                .Mappings(m => m.FluentMappings.Add<ChannelUser.Mapping>())
                .Mappings(m => m.FluentMappings.Add<WormyChannel.Mapping>())
                .Mappings(m => m.FluentMappings.Add<IgnoredMask.Mapping>())
                .Mappings(m => m.FluentMappings.Add<Reminder.Mapping>())
                .Mappings(m => m.FluentMappings.Add<OsuAlias.Mapping>())
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }

        private void BuildSchema(NHibernate.Cfg.Configuration config)
        {
            new SchemaUpdate(config).Execute(false, true);
        }
    }
}