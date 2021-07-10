using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace EntityFrameworkCore.Extensions.TAMS
{
    /// <summary>
    /// A container for methods providing automatic configuration of the data model based on fluent attributes.
    /// </summary>
    public static class DbContextConfiguration
    {
        /// <summary>
        /// Configures the <paramref name="context"/>'s delete behavior for entities decorated with the [<see cref="OnDelete"/>()] attribute.
        /// </summary>
        /// <typeparam name="T">Type implementing EntityFramework's <see cref="DbContext"/></typeparam>.
        /// <param name="builder">Reference to the <see cref="DbContext.OnModelCreating(ModelBuilder)"/> method's <see cref="ModelBuilder"/>.</param>
        /// <param name="context">Reference to the implemented EntityFramework <see cref="DbContext"/>.</param>
        public static void Configure<T>(ModelBuilder builder, T context) where T : DbContext
        {
            Assembly assembly = Assembly.GetAssembly(typeof(T));
            Type[] coreTypes = assembly.GetTypes();

            ConfigureOnDelete(builder, context);
        }

        private static void ConfigureOnDelete<T>(ModelBuilder builder, T context) where T : DbContext
        {
            // Reflect on assembly to automatically apply keys and delete behavior based on Data Annotations (OnDelete Attribute).
            IEnumerable<IMutableEntityType> mutableEntities = builder.Model.GetEntityTypes().ToList();
            //System.Diagnostics.Debugger.Launch();
            foreach (var modelEntity in mutableEntities)
            {
                Type modelEntityType = modelEntity.ClrType;
                var modelEntityProperties = modelEntityType.GetProperties();
                foreach (var modelEntityProperty in modelEntityProperties)
                {
                    Type modelEntityPropertyType = modelEntityProperty.PropertyType;
                    var onDeleteProperty = modelEntityProperty.GetCustomAttribute(typeof(OnDelete), false) as OnDelete;
                    if (onDeleteProperty != null)
                    {
                        // Has [OnDelete()] attribute
                        var entityBuilder = builder.Entity(modelEntityType);
                        object behavior;
                        if (Enum.TryParse(typeof(DeleteBehavior), onDeleteProperty.Behavior.ToString(), out behavior))
                        {
                            DeleteBehavior deleteBehavior = (DeleteBehavior)behavior;

                            // Try to find the "remote" foreign key for the "with" statement
                            string remotePropertyName = null;
                            Type remotePropertyType = null;
                            if (onDeleteProperty.HasRemoteProperty)
                            {
                                remotePropertyName = onDeleteProperty.RemoteKey;
                                remotePropertyType = modelEntityPropertyType.GetProperties().Where(o => o.Name == remotePropertyName).FirstOrDefault().PropertyType;
                            }
                            else
                            {
                                PropertyInfo[] remoteProperties = modelEntityPropertyType.GetProperties()
                                    .Where(o => !o.GetCustomAttributes<NotMappedAttribute>().Any())
                                    .ToArray();
                                PropertyInfo remoteProperty = remoteProperties
                                    .FirstOrDefault(o =>
                                        (o.PropertyType.Name == modelEntityType.Name)
                                        || (
                                            o.PropertyType
                                            .GetInterfaces()
                                            .Any(i =>
                                                i.IsGenericType
                                                && i.GetGenericTypeDefinition()
                                                    .Equals(typeof(IEnumerable<>))
                                                && i.GetGenericArguments()[0].Name == modelEntityType.Name
                                            )
                                        )
                                    )
                                    ?? null;
                                if (remoteProperty != null)
                                {
                                    remotePropertyType = remoteProperty.PropertyType;
                                    remotePropertyName = remoteProperty.Name;
                                }
                            }

                            // Get "local" foreign key for "HasForeignKey" statement
                            string foreignKeyName = null;
                            ForeignKeyAttribute foreignKey = modelEntityProperty.GetCustomAttribute(typeof(ForeignKeyAttribute), false) as ForeignKeyAttribute;
                            if (foreignKey != null)
                            {
                                foreignKeyName = foreignKey.Name;
                            }
                            bool isRequired = false;
                            PropertyInfo foreignProperty = modelEntityProperties.FirstOrDefault(o => o.Name == foreignKeyName);

                            if (foreignProperty != null)
                            {
                                Type foreignPropertyType = foreignProperty.PropertyType;

                                // Base "IsRequired" based on the "foreign key property" and/or the Required attribute
                                isRequired = (foreignProperty?.GetCustomAttributes<RequiredAttribute>()?.Any() ?? false)
                                    || modelEntityProperty.GetCustomAttributes<RequiredAttribute>().Any();
                                if (!isRequired)
                                    isRequired = foreignPropertyType != null && Nullable.GetUnderlyingType(foreignPropertyType) == null;

                            }

                            switch (onDeleteProperty.Type)
                            {
                                case EntityRelationship.OneWithOne:
                                    var owo = entityBuilder
                                        .HasOne(modelEntityPropertyType, modelEntityProperty.Name)
                                        .WithOne(remotePropertyName)
                                        .IsRequired(isRequired)
                                        .OnDelete(deleteBehavior);
                                    //if (!string.IsNullOrEmpty(foreignKeyName))
                                    //    owo.HasForeignKey(foreignKeyName);
                                    break;
                                case EntityRelationship.OneWithMany:
                                    var owm = entityBuilder
                                        .HasOne(modelEntityPropertyType, modelEntityProperty.Name)
                                        .WithMany(remotePropertyName)
                                        .IsRequired(isRequired)
                                        .OnDelete(deleteBehavior);
                                    if (!string.IsNullOrEmpty(foreignKeyName))
                                        owm.HasForeignKey(foreignKeyName);
                                    break;
                                case EntityRelationship.ManyWithOne:
                                    var mwo = entityBuilder
                                        .HasMany(modelEntityPropertyType, modelEntityProperty.Name)
                                        .WithOne(remotePropertyName)
                                        .IsRequired(isRequired)
                                        .OnDelete(deleteBehavior);
                                    if (!string.IsNullOrEmpty(foreignKeyName))
                                        mwo.HasForeignKey(foreignKeyName);
                                    break;
                                case EntityRelationship.ManyWithMany:
                                    // This should be handled with a lookup object
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
