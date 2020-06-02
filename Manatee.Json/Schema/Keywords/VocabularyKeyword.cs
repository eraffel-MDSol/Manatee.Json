﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Manatee.Json.Internal;
using Manatee.Json.Pointer;
using Manatee.Json.Serialization;

namespace Manatee.Json.Schema
{
	/// <summary>
	/// Defines the `$vocabulary` JSON Schema keyword.
	/// </summary>
	[DebuggerDisplay("Name={Name} Value={Vocabulary.Id}")]
	public class VocabularyKeyword : Dictionary<SchemaVocabulary, bool>, IJsonSchemaKeyword, IEquatable<VocabularyKeyword>
	{
		/// <summary>
		/// Gets the name of the keyword.
		/// </summary>
		public string Name => "$vocabulary";
		/// <summary>
		/// Gets the versions (drafts) of JSON Schema which support this keyword.
		/// </summary>
		public JsonSchemaVersion SupportedVersions => JsonSchemaVersion.Draft2019_09;
		/// <summary>
		/// Gets the a value indicating the sequence in which this keyword will be evaluated.
		/// </summary>
		/// <implementationNotes>
		/// May be duplicated across different keywords.  This property comes into play when there
		/// are several keywords which must be evaluated in a specific order.
		/// </implementationNotes>
		public int ValidationSequence => 0;
		/// <summary>
		/// Gets the vocabulary that defines this keyword.
		/// </summary>
		public SchemaVocabulary Vocabulary => SchemaVocabularies.Core;

		/// <summary>
		/// Builds an object from a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="json">The <see cref="JsonValue"/> representation of the object.</param>
		/// <param name="serializer">The <see cref="JsonSerializer"/> instance to use for additional
		/// serialization of values.</param>
		public void FromJson(JsonValue json, JsonSerializer serializer)
		{
			foreach (var kvp in json.Object)
			{
				var vocabulary = SchemaKeywordCatalog.GetVocabulary(kvp.Key) ?? new SchemaVocabulary(kvp.Key);
				this[vocabulary] = kvp.Value.Boolean;
			}
		}
		/// <summary>
		/// Converts an object to a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="serializer">The <see cref="JsonSerializer"/> instance to use for additional
		/// serialization of values.</param>
		/// <returns>The <see cref="JsonValue"/> representation of the object.</returns>
		public JsonValue ToJson(JsonSerializer serializer)
		{
			var json = new JsonObject();
			foreach (var kvp in this)
			{
				json[kvp.Key.Id] = kvp.Value;
			}

			return json;
		}
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(VocabularyKeyword? other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;

			return other.ContentsEqual(this);
		}
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(IJsonSchemaKeyword? other)
		{
			return Equals(other as VocabularyKeyword);
		}
		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object? obj)
		{
			return Equals(obj as VocabularyKeyword);
		}
		/// <summary>Serves as the default hash function. </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.GetCollectionHashCode();
		}
		/// <summary>
		/// Provides the validation logic for this keyword.
		/// </summary>
		/// <param name="context">The context object.</param>
		/// <returns>Results object containing a final result and any errors that may have been found.</returns>
		public SchemaValidationResults Validate(SchemaValidationContext context)
		{
			if (!context.IsMetaSchemaValidation) return SchemaValidationResults.Null;

			var nestedResults = new List<SchemaValidationResults>();

			var allVocabularies = this.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			allVocabularies[SchemaVocabularies.Core] = true;

			foreach (var kvp in allVocabularies)
			{
				var vocabulary = kvp.Key;
				if (vocabulary.MetaSchemaId == context.Local.Id) continue;

				var required = kvp.Value;
				if (vocabulary.MetaSchemaId != null)
				{
					var newContext = new SchemaValidationContext(context)
						{
							BaseRelativeLocation = context.BaseRelativeLocation?.CloneAndAppend(Name, vocabulary.Id),
							RelativeLocation = context.RelativeLocation.CloneAndAppend(Name, vocabulary.Id),
						};
					var metaSchema = JsonSchemaRegistry.Get(vocabulary.MetaSchemaId);
					if (metaSchema != null)
						metaSchema.Validate(newContext);
					else if (required)
						nestedResults.Add(new SchemaValidationResults(Name, newContext));
				}
			}

			var results = new SchemaValidationResults(Name, context)
				{
					NestedResults = nestedResults,
					IsValid = nestedResults.All(r => r.IsValid)
				};

			return results;
		}
		/// <summary>
		/// Used register any subschemas during validation.  Enables look-forward compatibility with `$ref` keywords.
		/// </summary>
		/// <param name="context">The context object.</param>
		public void RegisterSubschemas(SchemaValidationContext context) { }
		/// <summary>
		/// Resolves any subschemas during resolution of a `$ref` during validation.
		/// </summary>
		/// <param name="pointer">A <see cref="JsonPointer"/> to the target schema.</param>
		/// <param name="baseUri">The current base URI.</param>
		/// <param name="supportedVersions">Indicates the root schema's supported versions.</param>
		/// <returns>The referenced schema, if it exists; otherwise null.</returns>
		public JsonSchema? ResolveSubschema(JsonPointer pointer, Uri baseUri, JsonSchemaVersion supportedVersions)
		{
			return null;
		}
	}
}
