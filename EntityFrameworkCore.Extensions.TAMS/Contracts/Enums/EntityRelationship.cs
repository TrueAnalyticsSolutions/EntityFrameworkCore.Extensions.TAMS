using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.Extensions.TAMS
{
    /// <summary>
    /// Defines the relationship from one entity to another.
    /// </summary>
    public enum EntityRelationship
    {
        /// <summary>
        /// A relationship exists between one entity and another entity.
        /// </summary>
        /// <example>1..1</example>
        OneWithOne,
        /// <summary>
        /// A relationship exists between one entity and many of another entity.
        /// </summary>
        /// <example>1..*</example>
        OneWithMany,
        /// <summary>
        /// A relatinoship exists between many of an entity and and one of another entity.
        /// </summary>
        /// <example>*..1</example>
        ManyWithOne,
        /// <summary>
        /// A relationship exists between many of an entity and many of another entity.
        /// </summary>
        /// <example>*..*</example>
        ManyWithMany
    }
}
