using System;
using JetBrains.Annotations;

namespace Manatee.Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class RegexAttribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly string _regex;

		public RegexAttribute([RegexPattern] string regex)
		{
			_regex = regex;
		}

		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.Pattern(_regex);
		}
	}
}