using System;

namespace Manatee.Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MaxLengthAttribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly uint _value;

		public MaxLengthAttribute(uint value)
		{
			_value = value;
		}
		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.MaxLength(_value);
		}
	}
}