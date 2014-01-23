﻿/***************************************************************************************

	Copyright 2012 Greg Dennis

	   Licensed under the Apache License, Version 2.0 (the "License");
	   you may not use this file except in compliance with the License.
	   You may obtain a copy of the License at

		 http://www.apache.org/licenses/LICENSE-2.0

	   Unless required by applicable law or agreed to in writing, software
	   distributed under the License is distributed on an "AS IS" BASIS,
	   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	   See the License for the specific language governing permissions and
	   limitations under the License.
 
	File Name:		OneOfSchema.cs
	Namespace:		Manatee.Json.Schema
	Class Name:		OneOfSchema
	Purpose:		Used to define a collection of schema conditions, one of
					which must be satisfied.

***************************************************************************************/
using System.Collections.Generic;
using System.Linq;

namespace Manatee.Json.Schema
{
	/// <summary>
	/// Used to define a collection of schema conditions, one of which may
	/// be satisfied.
	/// </summary>
	public class OneOfSchema : IJsonSchema
	{
		/// <summary>
		/// A collection of schema options.
		/// </summary>
		public IEnumerable<IJsonSchema> Options { get; set; }
		/// <summary>
		/// The default value for this schema.
		/// </summary>
		/// <remarks>
		/// The default value is defined as a JSON value which may need to be deserialized
		/// to a .Net data structure.
		/// </remarks>
		public JsonValue Default { get; set; }

		/// <summary>
		/// Validates a <see cref="JsonValue"/> against the schema.
		/// </summary>
		/// <param name="json">A <see cref="JsonValue"/></param>
		/// <param name="root">The root schema serialized to a <see cref="JsonValue"/>.  Used internally for resolving references.</param>
		/// <returns>True if the <see cref="JsonValue"/> passes validation; otherwise false.</returns>
		public SchemaValidationResults Validate(JsonValue json, JsonValue root = null)
		{
			var jValue = root ?? ToJson();
			var errors = Options.Select(s => s.Validate(json, jValue)).ToList();
			if (errors.Count(r => r.Valid) == 1) return new SchemaValidationResults();
			if (errors.Count(r => r.Valid) == 0) return new SchemaValidationResults(errors);
			return new SchemaValidationResults(string.Empty, "More than one option was valid.");
		}
		/// <summary>
		/// Builds an object from a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="json">The <see cref="JsonValue"/> representation of the object.</param>
		public void FromJson(JsonValue json)
		{
			var obj = json.Object;
			Options = obj["oneOf"].Array.Select(JsonSchemaFactory.FromJson);
			if (obj.ContainsKey("default")) Default = obj["default"];
		}
		/// <summary>
		/// Converts an object to a <see cref="JsonValue"/>.
		/// </summary>
		/// <returns>The <see cref="JsonValue"/> representation of the object.</returns>
		public JsonValue ToJson()
		{
			var json = new JsonObject {{"oneOf", Options.ToJson()}};
			if (Default != null) json["default"] = Default;
			return json;
		}
	}
}