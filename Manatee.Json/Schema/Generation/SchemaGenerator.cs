using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Manatee.Json.Internal;
using Manatee.Json.Serialization;
using Manatee.Json.Serialization.Internal.Serializers;

namespace Manatee.Json.Schema.Generation
{
	internal class SchemaGenerator
	{
		private static readonly Dictionary<Type, JsonSchema> _rawSchemas = new Dictionary<Type, JsonSchema>();
		private bool _generateProperties;

		public JsonSchema Generate<T>(JsonSerializer serializer)
		{
			return Generate(typeof(T), serializer);
		}

		public JsonSchema Generate(Type type, JsonSerializer serializer)
		{
			if (_rawSchemas.TryGetValue(type, out var schema)) return schema;

			var properties = ReflectionCache.GetMembers(type, PropertySelectionStrategy.ReadWriteOnly, false)
			                                .Select(s => s.MemberInfo)
			                                .OfType<PropertyInfo>()
			                                .ToList();
			schema = new JsonSchema();
			_AssignType(schema, type, serializer);
			if (_generateProperties)
			{
				foreach (var propertyInfo in properties)
				{
					var propertySchema = new SchemaGenerator().Generate(propertyInfo.PropertyType, serializer);
					var attributes = propertyInfo.GetCustomAttributes()
					                             .OfType<ISchemaGenerationAttribute>()
					                             .ToList();
					JsonSchema schemaToUpdate;
					switch (propertySchema.Type())
					{
						case JsonSchemaType.Array:
							schemaToUpdate = propertySchema.Items()[0];
							break;
						case JsonSchemaType.Object:
							schemaToUpdate = propertySchema.AdditionalProperties();
							break;
						default:
							schemaToUpdate = propertySchema;
							break;
					}
					foreach (var attribute in attributes)
					{
						attribute.Update(schemaToUpdate);
					}
					var propertyName = serializer.Options.SerializationNameTransform(propertyInfo.Name);
					schema.Property(propertyName, propertySchema);
					if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null)
						schema.Required(propertyName);
				}
			}

			//var allSchemas = _CollectSchemas(schema);

			return Equals(schema, JsonSchema.Empty)
				       ? JsonSchema.True
				       : schema;
		}

		//private Dictionary<JsonSchema, int> _CollectSchemas(JsonSchema schema)
		//{
		//	var subschemas = new List<IJsonSchema>();
		//	if (schema.Items != null)
		//		subschemas.Add(schema.Items);
		//	if (schema.AdditionalProperties != null)
		//		subschemas.Add(schema.AdditionalProperties);
		//	if (schema.Properties != null)
		//		subschemas.AddRange(schema.Properties.Values);
		//	if (schema.Definitions != null)
		//		subschemas.AddRange(schema.Definitions.Values);

		//}

		public JsonSchema Generate2(Type type)
		{
			var properties = ReflectionCache.GetMembers(type, PropertySelectionStrategy.ReadWriteOnly, false)
				.Select(m => m.MemberInfo)
				.OfType<PropertyInfo>()
				.ToList();

			var allPropertyTypes = new List<Type>();
			_GetComplexPropertyTypes(type, allPropertyTypes);

			allPropertyTypes.Remove(type);

			// generate schemas for all property types
			// if I add them in reverse, it *should* be in an inverted dependency sequence
			var definitions = allPropertyTypes.Select()

			// create empty schema
			var schema = new JsonSchema();

			// get schema type
			_AssignType(schema, type, serializer);

			// add properties
			if (_generateProperties)
			{
				foreach (var propertyInfo in properties)
				{
					var propertySchema = new SchemaGenerator().Generate(propertyInfo.PropertyType, serializer);
					var attributes = propertyInfo.GetCustomAttributes()
						.OfType<ISchemaGenerationAttribute>()
						.ToList();
					JsonSchema schemaToUpdate;
					switch (propertySchema.Type())
					{
						case JsonSchemaType.Array:
							schemaToUpdate = propertySchema.Items()[0];
							break;
						case JsonSchemaType.Object:
							schemaToUpdate = propertySchema.AdditionalProperties();
							break;
						default:
							schemaToUpdate = propertySchema;
							break;
					}
					foreach (var attribute in attributes)
					{
						attribute.Update(schemaToUpdate);
					}
					var propertyName = serializer.Options.SerializationNameTransform(propertyInfo.Name);
					schema.Property(propertyName, propertySchema);
					if (propertyInfo.GetCustomAttribute<RequiredAttribute>() != null)
						schema.Required(propertyName);
				}
			}

			return Equals(schema, JsonSchema.Empty)
				? JsonSchema.True
				: schema;
		}

		private void _GetComplexPropertyTypes(Type type, List<Type> foundTypes)
		{
			var propertyTypes = ReflectionCache.GetMembers(type, PropertySelectionStrategy.ReadWriteOnly, false)
				.Select(m => ((PropertyInfo) m.MemberInfo).PropertyType)
				.Except(foundTypes)
				.ToList();

			foundTypes.AddRange(propertyTypes);

			foreach (var propertyType in propertyTypes)
			{
				_GetComplexPropertyTypes(propertyType, foundTypes);
			}
		}

		private void _AssignType(JsonSchema schema, Type type, JsonSerializer serializer)
		{
			var typeInfo = type.GetTypeInfo();

			if (typeInfo.IsGenericType &&
			    typeInfo.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
			    typeInfo.GenericTypeArguments[0] == typeof(string))
			{
				schema.Type(JsonSchemaType.Object);
				schema.AdditionalProperties(new SchemaGenerator().Generate(typeInfo.GenericTypeArguments[1], serializer));
			}
			else if (type == typeof(string))
				schema.Type(JsonSchemaType.String);
			else if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeInfo))
			{
				schema.Type(JsonSchemaType.Array);
				if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(List<>))
					schema.Items(new SchemaGenerator().Generate(typeInfo.GenericTypeArguments[0], serializer));
			}
			else if (type.IsFloat())
				schema.Type(JsonSchemaType.Number);
			else if (type.IsInteger() || (typeInfo.IsEnum && serializer.Options.EnumSerializationFormat == EnumSerializationFormat.AsInteger))
				schema.Type(JsonSchemaType.Integer);
			else if (type == typeof(bool))
				schema.Type(JsonSchemaType.Boolean);
			else if (typeInfo.IsEnum && serializer.Options.EnumSerializationFormat == EnumSerializationFormat.AsName)
			{
				var defaultOption = serializer.Options.EncodeDefaultValues;
				serializer.Options.EncodeDefaultValues = true;
				schema.Enum(Enum.GetValues(type).Cast<object>().Select(v => serializer.Serialize(type, v)).ToArray());
				serializer.Options.EncodeDefaultValues = defaultOption;
			}
			else
			{
				schema.Type(JsonSchemaType.Object | JsonSchemaType.Null);
				_generateProperties = true;
				_rawSchemas[type] = schema;
			}
		}
	}

	public interface ISchemaProvider
	{
		Type Type { get; }
		JsonSchema Schema { get; }
	}
}