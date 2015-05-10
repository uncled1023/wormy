using System;

namespace wormy.Database
{
    /// <summary>
    /// Manages what's considered a single "identity", or a single real person that
    /// has connected to IRC. Keeps track of all known nicks, users, hosts, real names,
    /// nickserv accounts, etc in an effort to isolate one person.
    /// </summary>
    public class Identity
    {
        public class Alias
        {
            public virtual int ID { get; set; }
            public virtual TimeSpan Duration { get; set; }
            public virtual bool Confirmed { get; set; }
        }
        
        public class KnownNick : Alias
        {
            public virtual string Nick { get; set; }
        }

        public class KnownUser : Alias
        {
            public virtual string User { get; set; }
        }

        public class KnownHost : Alias
        {
            public virtual string Host { get; set; }
        }

        public class KnownRealName : Alias
        {
            public virtual string RealName { get; set; }
        }

        public class UserNetwork
        {
            public virtual Network Network { get; set; }
            public virtual string NickservAccount { get; set; }
        }

        public virtual int ID { get; set; }
        public virtual Guid Guid { get; set; }
        public virtual DateTime LastSeen { get; set; }
    }
}