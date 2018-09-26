using System;

namespace Manatee.Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class FormatAttribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly StringFormat _format;

		public FormatAttribute(string formatKey)
		{
			_format = StringFormat.GetFormat(formatKey);
		}

		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.Format(_format);
		}
	}
}