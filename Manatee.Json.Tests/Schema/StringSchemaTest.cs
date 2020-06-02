﻿using System.Collections.Generic;
using Manatee.Json.Pointer;
using Manatee.Json.Schema;
using Manatee.Json.Serialization;
using NUnit.Framework;

namespace Manatee.Json.Tests.Schema
{
	[TestFixture]
	public class StringSchemaTest
	{
		[OneTimeSetUp]
		public void Setup()
		{
			JsonOptions.LogCategory = LogCategory.Schema;
		}

		[Test]
		public void ValidateReturnsErrorOnNonString()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String);
			var json = new JsonObject();

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsErrorOnTooShort()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).MinLength(5);
			var json = (JsonValue) "test";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsValidOnLengthEqualsMinLength()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).MinLength(5);
			var json = (JsonValue) "test1";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsValidOnLengthGreaterThanMinLength()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).MinLength(5);
			var json = (JsonValue) "test123";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsErrorOnTooLong()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).MaxLength(5);
			var json = (JsonValue) "test123";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsValidOnLengthEqualsMaxLength()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).MaxLength(5);
			var json = (JsonValue) "test1";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsValidOnLengthLessThanMaxLength()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).MaxLength(5);
			var json = (JsonValue) "test";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsErrorOnInvalidDateTimeFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.DateTime!);
			var json = (JsonValue) "test123";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[TestCaseSource(nameof(_dateTimeCases))]
		public void ValidateReturnsValidOnValidDateTimeFormat(string dateTimeString)
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.DateTime!);
			var json = (JsonValue) dateTimeString;

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsErrorOnInvalidEmailFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Email!);
			var json = (JsonValue) "test123";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsValidOnValidEmailFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Email!);
			var json = (JsonValue) "me@you.com";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsValidOnValidEmailFormat2()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Email!);
			var json = (JsonValue) "Me@You.net";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsErrorOnInvalidHostNameFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.HostName!);
			var json = (JsonValue) "$test123";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsValidOnValidHostNameFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.HostName!);
			var json = (JsonValue) "me.you.com";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsErrorOnInvalidIpv4Format()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Ipv4!);
			var json = (JsonValue) "test123";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsValidOnValidIpv4Format()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Ipv4!);
			var json = (JsonValue) "255.255.1.1";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsErrorOnInvalidIpv6Format()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Ipv6!);
			var json = (JsonValue) "test123";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsValidOnValidIpv6Format()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Ipv6!);
			var json = (JsonValue) "2001:0db8:85a3:0042:1000:8a2e:0370:7334";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsErrorOnInvalidUriFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Uri!);
			var json = (JsonValue) "test123^%";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsValidOnValidUriFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format(Formats.Uri!);
			var json = (JsonValue) "http://en.wikipedia.org/wiki/ISO_8601";

			var results = schema.Validate(json);

			results.AssertValid();
		}
		[Test]
		public void ValidateReturnsValidOnUnknownFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format("Int32");
			var json = (JsonValue) "32";
			var expected = new SchemaValidationResults
				{
					IsValid = true,
					RelativeLocation = JsonPointer.Parse("#/format"),
					InstanceLocation = JsonPointer.Parse("#"),
					Keyword = "format",
					AnnotationValue = "Int32",
					AdditionalInfo = new JsonObject
						{
							["actual"] = json,
							["format"] = "Int32",
							["isKnownFormat"] = false
						}
	};

			var results = schema.Validate(json, new JsonSchemaOptions { OutputFormat = SchemaValidationOutputFormat.Detailed });

			results.AssertValid(expected);
		}
		[Test]
		public void DeserializeDoesNotThrowOnUnknownFormat()
		{
			JsonSchemaOptions.Default.AllowUnknownFormats = false;
			var serializer = new JsonSerializer();
			var schemaJson = new JsonObject
				{
					["type"] = "string",
					["format"] = "Int32"
				};

			serializer.Deserialize<JsonSchema>(schemaJson);
		}
		[Test]
		public void ValidateReturnsInvalidOnUnknownFormat()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Format("Int32");
			var json = (JsonValue)"32";
			var expected = new SchemaValidationResults
				{
					IsValid = false,
					RelativeLocation = JsonPointer.Parse("#/format"),
					InstanceLocation = JsonPointer.Parse("#"),
					Keyword = "format",
					ErrorMessage = "\"32\" is not in an acceptable \"Int32\" format.",
					AnnotationValue = "Int32",
					AdditionalInfo = new JsonObject
						{
							["actual"] = "32",
							["format"] = "Int32",
							["isKnownFormat"] = false
						}
				};

			var results = schema.Validate(json, new JsonSchemaOptions
				{
					OutputFormat = SchemaValidationOutputFormat.Detailed,
					AllowUnknownFormats = false
				});

			results.AssertInvalid(expected);
		}
		[Test]
		public void ValidateReturnsErrorOnPatternNonMatch()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Pattern("^[0-9_]+$");
			var json = (JsonValue) "test123";

			var results = schema.Validate(json);

			results.AssertInvalid();
		}
		[Test]
		public void ValidateReturnsValidOnPatternMatch()
		{
			var schema = new JsonSchema().Type(JsonSchemaType.String).Pattern("^[0-9_]+$");
			var json = (JsonValue) "81681_1868";

			var results = schema.Validate(json);

			results.AssertValid();
		}

		private static string[] _dateTimeCases = {
			"2016-01-25T10:32:02Z",
			"2019-09-25T08:40:24.1383719Z",
			"2019-09-25T08:40:24.138371Z",
			"2019-09-25T08:40:24.13837Z",
			"2019-09-25T08:40:24.1383Z",
			"2019-09-25T08:40:24.138Z",
			"2019-09-25T08:40:24.13Z",
			"2019-09-25T08:40:24.1Z",
			"2019-09-26T15:46:18.4123654+01:00",
			"2019-09-26T13:46:43.4281740-01:00"
		};
	}
}
