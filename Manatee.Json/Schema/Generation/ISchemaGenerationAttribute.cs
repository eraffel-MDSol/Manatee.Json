namespace Manatee.Json.Schema.Generation
{
	internal interface ISchemaGenerationAttribute
	{
		void Update(JsonSchema schema);
	}
}