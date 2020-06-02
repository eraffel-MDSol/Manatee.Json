﻿using System;
using Manatee.Json.Pointer;
using Manatee.Json.Schema;
using NUnit.Framework;

namespace Manatee.Json.Tests.Schema
{
	[TestFixture]
	public class AllOfSchemaTest
	{
		[OneTimeSetUp]
		public void Setup()
		{
			JsonOptions.LogCategory = LogCategory.Schema;
		}

		[Test]
		public void ValidateReturnsErrorOnAnyInvalid()
		{
			var schema = new JsonSchema()
				.AllOf(new JsonSchema().Type(JsonSchemaType.Array),
				       new JsonSchema().Type(JsonSchemaType.Number));

			var json = new JsonObject();

			var expected = new SchemaValidationResults
				{
					IsValid = false,
					RelativeLocation = JsonPointer.Parse("#"),
					InstanceLocation = JsonPointer.Parse("#"),
					NestedResults =
						{
							new SchemaValidationResults
								{
									IsValid = false,
									Keyword = "allOf",
									RelativeLocation = JsonPointer.Parse("#/allOf"),
									InstanceLocation = JsonPointer.Parse("#"),
									ErrorMessage = "2 of 2 subschemas failed validation.",
									AdditionalInfo =
										{
											["failed"] = 2,
											["total"] = 2
										},
									NestedResults =
										{
											new SchemaValidationResults
												{
													IsValid = false,
													RelativeLocation = JsonPointer.Parse("#/allOf/0"),
													InstanceLocation = JsonPointer.Parse("#"),
													NestedResults =
														{
															new SchemaValidationResults
																{
																	IsValid = false,
																	Keyword = "type",
																	RelativeLocation = JsonPointer.Parse("#/allOf/0/type"),
																	InstanceLocation = JsonPointer.Parse("#"),
																	ErrorMessage = "Values of type \"object\" are not one of the allowed types \"array\".",
																	AdditionalInfo =
																		{
																			["allowed"] = "array",
																			["actual"] = "object"
																		}
																}
														}
												},
											new SchemaValidationResults
												{
													IsValid = false,
													RelativeLocation = JsonPointer.Parse("#/allOf/1"),
													InstanceLocation = JsonPointer.Parse("#"),
													NestedResults =
														{
															new SchemaValidationResults
																{
																	IsValid = false,
																	Keyword = "type",
																	RelativeLocation = JsonPointer.Parse("#/allOf/1/type"),
																	InstanceLocation = JsonPointer.Parse("#"),
																	ErrorMessage = "Values of type \"object\" are not one of the allowed types \"number\".",
																	AdditionalInfo =
																		{
																			["allowed"] = "number",
																			["actual"] = "object"
																		}
																}
														}
												}
										}
								}
						}
				};

			var results = schema.Validate(json, new JsonSchemaOptions{OutputFormat = SchemaValidationOutputFormat.Verbose});

			results.AssertInvalid(expected);
		}

		[Test]
		public void ValidateReturnsValidOnAllValid()
		{
			var schema = new JsonSchema()
				.AllOf(new JsonSchema()
					       .Type(JsonSchemaType.Number)
					       .Minimum(10),
				       new JsonSchema()
					       .Type(JsonSchemaType.Number)
					       .Maximum(20));

			var json = (JsonValue) 15;

			var expected = new SchemaValidationResults
				{
					IsValid = true,
					RelativeLocation = JsonPointer.Parse("#"),
					InstanceLocation = JsonPointer.Parse("#"),
					NestedResults =
						{
							new SchemaValidationResults
								{
									IsValid = true,
									RelativeLocation = JsonPointer.Parse("#/allOf"),
									InstanceLocation = JsonPointer.Parse("#"),
									Keyword = "allOf",
									NestedResults =
										{
											new SchemaValidationResults
												{
													IsValid = true,
													RelativeLocation = JsonPointer.Parse("#/allOf/0"),
													InstanceLocation = JsonPointer.Parse("#"),
													NestedResults =
														{
															new SchemaValidationResults
																{
																	IsValid = true,
																	RelativeLocation = JsonPointer.Parse("#/allOf/0/type"),
																	InstanceLocation = JsonPointer.Parse("#"),
																	Keyword = "type"
																},
															new SchemaValidationResults
																{
																	IsValid = true,
																	RelativeLocation = JsonPointer.Parse("#/allOf/0/minimum"),
																	InstanceLocation = JsonPointer.Parse("#"),
																	Keyword = "minimum"
																}
														}
												},
											new SchemaValidationResults
												{
													IsValid = true,
													RelativeLocation = JsonPointer.Parse("#/allOf/1"),
													InstanceLocation = JsonPointer.Parse("#"),
													NestedResults =
														{
															new SchemaValidationResults
																{
																	IsValid = true,
																	RelativeLocation = JsonPointer.Parse("#/allOf/1/type"),
																	InstanceLocation = JsonPointer.Parse("#"),
																	Keyword = "type"
																},
															new SchemaValidationResults
																{
																	IsValid = true,
																	RelativeLocation = JsonPointer.Parse("#/allOf/1/maximum"),
																	InstanceLocation = JsonPointer.Parse("#"),
																	Keyword = "maximum"
																}
														}
												}
										}
								}
						}
				};

			var results = schema.Validate(json, new JsonSchemaOptions {OutputFormat = SchemaValidationOutputFormat.Verbose});

			results.AssertValid(expected);
		}

		[Test]
		public void AllOfOrderingUnimportant()
		{
			var schema = new JsonSchema().Schema(MetaSchemas.Draft2019_09.Schema)
				.AllOf(new JsonSchema().Property("foo", true),
					new JsonSchema().UnevaluatedProperties(false));
			JsonValue instance = new JsonObject{["foo"]  = 1};

			var results = schema.Validate(instance);
			
			results.AssertInvalid();
		}
	}
}