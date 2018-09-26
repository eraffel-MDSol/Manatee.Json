using System;
using Manatee.Json.Schema;
using Manatee.Json.Serialization;
using Manatee.Json.Tests.Test_References;
using NUnit.Framework;

namespace Manatee.Json.Tests.Schema
{
	[TestFixture]
	public class GenerationTest
	{
		[Test]
		public void GenerateSchemaFromClass()
		{
			var expected = new JsonSchema()
				.Definition("Manatee.Json.Tests.Test_References.Object.TestEnum", new JsonSchema()
					            .Enum("None", "BasicEnumValue", "enum_value_with_description"))
				.Definition("Manatee.Json.Tests.Test_References.Object.FlagsEnum", new JsonSchema()
					            .Enum("None", "BasicEnumValue", "enum_value_with_description"))
				.Definition("Manatee.Json.Tests.Test_References.Object.SchemaGenerationSimpleTarget", new JsonSchema()
					            .Property("Integer", new JsonSchema().Type(JsonSchemaType.Integer))
					            .Property("String", new JsonSchema().Type(JsonSchemaType.String)))
				.Property("StringProp", new JsonSchema()
					          .Type(JsonSchemaType.String)
					          .MinLength(10)
					          .MaxLength(20))
				.Property("IntProp", new JsonSchema()
					          .Type(JsonSchemaType.Integer)
					          .Minimum(0)
					          .Maximum(50))
				.Property("DoubleProp", new JsonSchema()
					          .Type(JsonSchemaType.Number)
					          .ExclusiveMinimum(0)
					          .ExclusiveMaximum(50))
				.Property("BoolProp", new JsonSchema().Type(JsonSchemaType.Boolean))
				.Property("EnumProp", new JsonSchema().Ref("#/definitions/Manatee.Json.Tests.Test_References.Object.TestEnum"))
				.Property("FlagsEnumProp", new JsonSchema().Ref("#/definitions/Manatee.Json.Tests.Test_References.Object.FlagsEnum"))
				.Property("MappedProp", new JsonSchema().Type(JsonSchemaType.Integer))
				.Property("ReadOnlyListProp", new JsonSchema()
					          .Type(JsonSchemaType.Array)
					          .Items(new JsonSchema()
						                 .Type(JsonSchemaType.Integer)
						                 .Minimum(10)))
				.Property("ReadOnlyDictionaryProp", new JsonSchema()
					          .Type(JsonSchemaType.Object)
					          .AdditionalProperties(new JsonSchema()
						                                .Type(JsonSchemaType.String)
						                                .MaxLength(10)))
				.Property("Email", new JsonSchema()
					          .Type(JsonSchemaType.String)
					          .Format(StringFormat.Email))
				.Property("Alphanumeric", new JsonSchema()
					          .Type(JsonSchemaType.String)
					          .Pattern("^[a-zA-Z0-9]*$")
					          .MinLength(10)
					          .MaxLength(20))
				.Property("Website", new JsonSchema()
					          .Type(JsonSchemaType.String)
					          .Format(StringFormat.Uri))
				.Property("Recurse", new JsonSchema().RefRoot())
				.Property("Simple", new JsonSchema().Ref("#/definitions/Manatee.Json.Tests.Test_References.Object.SchemaGenerationSimpleTarget"));

			var serializer = new JsonSerializer
				{
					Options =
						{
							EnumSerializationFormat = EnumSerializationFormat.AsName
						}
				};

			var actual = JsonSchema.GenerateFor<SchemaGenerationTarget>(serializer);

			Console.WriteLine(actual.ToJson(serializer));

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GeneratedSchemaValidatesModel()
		{
			var model = new SchemaGenerationTarget
				{
					Alphanumeric = "noon23tn4in2",
					BoolProp = true,
					DoubleProp = 15.4,
					Email = "me@you.com",
					EnumProp = TestEnum.BasicEnumValue,
					FlagsEnumProp = FlagsEnum.EnumValueWithDescription,
					IntProp = 25,
					MappedProp = -6,
					ReadOnlyDictionaryProp =
						{
							["one"] = "1",
							["two"] = "2"
						},
					ReadOnlyListProp =
						{
							11,
							12,
							13,
							14
						},
					StringProp = "1234567890123",
					Website = "http://site.com",
					Recurse = new SchemaGenerationTarget(),
					Simple = new SchemaGenerationSimpleTarget
						{
							Integer = 4,
							String = "string"
						}
				};
			var serializer = new JsonSerializer
				{
					Options =
						{
							EnumSerializationFormat = EnumSerializationFormat.AsName
						}
				};
			var json = serializer.Serialize(model);
			var schema = JsonSchema.GenerateFor<SchemaGenerationTarget>(serializer);

			var results = schema.Validate(json);

			Console.WriteLine(schema.ToJson(serializer));
			Console.WriteLine();
			Console.WriteLine(results.ToJson(serializer));

			Assert.IsTrue(results.IsValid);
		}
	}
}
