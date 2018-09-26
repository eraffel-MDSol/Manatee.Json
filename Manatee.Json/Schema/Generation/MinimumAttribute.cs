using System;

namespace Manatee.Json.Schema.Generation
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinimumAttribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly double _value;

		public MinimumAttribute(double value)
		{
			_value = value;
		}
		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.Minimum(_value);
		}
	}
	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMinimumAttribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly double _value;

		public ExclusiveMinimumAttribute(double value)
		{
			_value = value;
		}
		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.ExclusiveMinimum(_value);
		}
	}
	[AttributeUsage(AttributeTargets.Property)]
	public class ExclusiveMinimumDraft04Attribute : Attribute, ISchemaGenerationAttribute
	{
		private readonly bool _value;

		public ExclusiveMinimumDraft04Attribute(bool value)
		{
			_value = value;
		}
		void ISchemaGenerationAttribute.Update(JsonSchema schema)
		{
			schema.ExclusiveMinimumDraft04(_value);
		}
	}
}