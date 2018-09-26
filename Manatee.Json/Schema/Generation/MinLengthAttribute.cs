using System;

namespace Manatee.Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinLengthAttribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly uint _value;

		public MinLengthAttribute(uint value)
		{
			_value = value;
		}
		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.MinLength(_value);
		}
	}
}