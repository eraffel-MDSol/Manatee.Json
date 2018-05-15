using System;
using System.Collections;
using System.Collections.Generic;
using Manatee.Json.Schema;
using NUnit.Framework;

namespace Manatee.Json.Tests.Schema
{
	[TestFixture]
	public class MiscTests
	{
		// ReSharper disable once InconsistentNaming
		private static readonly IJsonSchema AjvIfThenElseExample1Schema = new JsonSchema07
			{
				If = new JsonSchema07
					{
						Properties = new Dictionary<string, IJsonSchema>
							{
								["power"] = new JsonSchema07 { Minimum = 9000 }
							}
					},
				Then = new JsonSchema07 { Required = new[] { "disbelief" } },
				Else = new JsonSchema07 { Required = new[] { "confidence" } }
			};

		public static IEnumerable AjvIfThenElseExample1Cases
		{
			get
			{
				yield return new TestCaseData(new JsonObject {["power"] = 10000, ["disbelief"] = true}, true);
				yield return new TestCaseData(new JsonObject(), true);
				yield return new TestCaseData(new JsonObject {["power"] = 1000, ["confidence"] = true}, true);
				yield return new TestCaseData(new JsonObject {["power"] = 10000}, false);
				yield return new TestCaseData(new JsonObject {["power"] = 10000, ["confidence"] = true}, false);
				yield return new TestCaseData(new JsonObject {["power"] = 1000}, false);
			}
		}

		[TestCaseSource(nameof(AjvIfThenElseExample1Cases))]
		public void AjvIfThenElseExample1(JsonObject instance, bool isValid)
		{
			var results = AjvIfThenElseExample1Schema.Validate(instance);

			foreach (var error in results.Errors)
			{
				Console.WriteLine(error);
			}
			Assert.AreEqual(isValid, results.Valid);
		}
	}
}
