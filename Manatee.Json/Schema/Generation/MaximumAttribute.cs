using System;

namespace Manatee.Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MaximumAttribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly double _value;

		public MaximumAttribute(double value)
		{
			_value = value;
		}
		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.Maximum(_value);
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMaximumAttribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly double _value;

		public ExclusiveMaximumAttribute(double value)
		{
			_value = value;
		}
		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.ExclusiveMaximum(_value);
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMaximumDraft04Attribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly bool _value;

		public ExclusiveMaximumDraft04Attribute(bool value)
		{
			_value = value;
		}
		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.ExclusiveMaximumDraft04(_value);
		}
	}
}