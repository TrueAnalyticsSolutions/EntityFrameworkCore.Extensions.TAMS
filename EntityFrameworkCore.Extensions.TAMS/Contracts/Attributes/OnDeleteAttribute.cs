using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.Extensions.TAMS
{
    /// <summary>
    /// An attribute that defines the configuration of the <see cref="OnDelete"/> behavior to be set via Fluent configuration in <see cref="DbContext.OnModelCreating(ModelBuilder)"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OnDelete : Attribute
    {
        /// <summary>
        /// Property name of the Primary key of the other class.
        /// </summary>
        public string RemoteKey { get; set; }

        /// <summary>
        /// Flags whether or not the primary key of the remote class was identified.
        /// </summary>
        public bool HasRemoteProperty => !string.IsNullOrEmpty(RemoteKey);

        /// <summary>
        /// Defines the relationship from the attached property and the remote class.
        /// </summary>
        public EntityRelationship Type { get; set; }

        /// <summary>
        /// Defines the behavior for what happens to the remote class when the attached property's class is deleted.
        /// </summary>
        public DeleteBehavior Behavior { get; set; }

        /// <summary>
        /// Constructs the framework for configuring OnDelete behavior
        /// </summary>
        /// <param name="type">Defines the relationship from the attached property and the remote class.</param>
        /// <param name="behavior">Defines the behavior for what happens to the remote class when the attached property's class is deleted.</param>
        public OnDelete(EntityRelationship type, DeleteBehavior behavior)
        {
            RemoteKey = string.Empty;
            Type = type;
            Behavior = behavior;
        }

        public OnDelete(string remoteProperty, EntityRelationship type, DeleteBehavior behavior) : this(type, behavior)
        {
            RemoteKey = remoteProperty;
        }
    }
}
