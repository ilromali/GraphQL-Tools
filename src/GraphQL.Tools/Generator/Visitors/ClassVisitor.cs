using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL.Tools.Generator.Base;
using GraphQL.Tools.Generator.Extensions;
using GraphQL.Types;
using GraphQLParser.AST;

namespace GraphQL.Tools.Generator.Visitors
{
    /// <summary>
    /// This visitor will extract GraphQL fields as classes.
    /// </summary>
    /// <example>
    /// GraphQL schema:
    /// <code>
    ///  type Simple {
    ///      int32: Int!
    ///      float: Float
    ///      string: String!
    ///      bool: Boolean
    ///      id: ID!
    ///  }
    /// </code>
    ///
    /// Extracted class:
    /// <code>
    /// public class Simple
    /// {
    ///     public int Int32 { get; set; }
    ///     public float? Float { get; set; }
    ///     public string String { get; set; }
    ///     public bool? Bool { get; set; }
    ///     public Guid Id { get; set; }
    /// }
    /// </code>
    /// </example>
    public class ClassVisitor : IGeneratableTypeVisitor
    {
        public HashSet<IGeneratableType> Visit(IEnumerable<IGraphType> graphTypes)
        {
            var @classes = new HashSet<IGeneratableType>();

            foreach (ObjectGraphType objectGraphType in graphTypes.Where(type => type is ObjectGraphType))
            {
                var className = objectGraphType.Name;
                var @class = new Class(className);

                foreach (FieldType fieldType in objectGraphType.Fields)
                {
                    @class.Properties
                        .Add(new Property(fieldType.Name, 
                            fieldType.GetTypeName(),
                            fieldType.IsArray(),
                            fieldType.IsNullable()));
                }

                ExtractImplementedInterfaces(objectGraphType, @class);

                @classes.Add(@class);
            }

            return @classes;
        }

        private static void ExtractImplementedInterfaces(ObjectGraphType objectGraphType, Class @class)
        {
            if (objectGraphType.ResolvedInterfaces.Any())
            {
                foreach (var interfaceGraphType in objectGraphType.ResolvedInterfaces)
                {
                    var interfaceName = ((GraphQLTypeReference)interfaceGraphType).TypeName;
                    @class.Interfaces.Add(interfaceName);
                }
            }
        }
    }
}