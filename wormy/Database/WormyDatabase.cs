using System;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using FluentNHibernate.Mapping;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;
using System.Linq;

namespace wormy.Database
{
    public class WormyDatabase
    {
        public ISessionFactory SessionFactory { get; set; }

        public WormyDatabase(string connectionString)
        {
            var config = PostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionString);
            var c = Fluently.Configure()
                .Database(config);
            var databaseMappings = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.BaseType != null
                        && type.BaseType.IsGenericType
                        && type.BaseType.GetGenericTypeDefinition() == typeof(ClassMap<>)
                        && !type.IsAbstract
                        && !type.IsInterface
                        && !type.IsGenericType)
                    {
                        databaseMappings.Add(type);
                    }
                }
            }
            databaseMappings.ToList().ForEach(t => c = c.Mappings(m => m.FluentMappings.Add(t)));
            SessionFactory = c
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }

        private void BuildSchema(NHibernate.Cfg.Configuration config)
        {
            new SchemaUpdate(config).Execute(true, true);
        }
    }
}