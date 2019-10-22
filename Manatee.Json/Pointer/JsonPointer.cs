﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Manatee.Json.Internal;
using Manatee.Json.Serialization;

namespace Manatee.Json.Pointer
{
	/// <summary>
	/// Represents a JSON Pointer.
	/// </summary>
	[DebuggerDisplay("{ToString()}")]
	public class JsonPointer : List<string>, IJsonSerializable, IEquatable<JsonPointer>
	{
		private static readonly Regex _generalEscapePattern = new Regex("%(?<Value>[0-9A-F]{2})", RegexOptions.IgnoreCase);

		private bool _usesHash;

		/// <summary>
		/// Creates a new <see cref="JsonPointer"/> instance.
		/// </summary>
		public JsonPointer() { }
		/// <summary>
		/// Creates a new <see cref="JsonPointer"/> instance.
		/// </summary>
		/// <param name="source">A collection of strings representing the segments of the pointer.</param>
		public JsonPointer(params string[] source)
			: this((IEnumerable<string>) source) { }
		/// <summary>
		/// Creates a new <see cref="JsonPointer"/> instance.
		/// </summary>
		/// <param name="source">A collection of strings representing the segments of the pointer.</param>
		public JsonPointer(IEnumerable<string> source)
			: base(source.FirstOrDefault() == "#" ? source.SkipWhile(s => s == "#") : source)
		{
			_usesHash = source.FirstOrDefault() == "#";
		}

		/// <summary>
		/// Creates a new <see cref="JsonPointer"/> instance.
		/// </summary>
		/// <param name="source">A collection of strings representing the segments of the pointer.</param>
		/// /// <param name="capacity">An integer indicating the initial capacity for the underlying List.</param>
		private JsonPointer(JsonPointer source, int capacity)
			: base(capacity)
		{
			AddRange(source.FirstOrDefault() == "#" ? source.SkipWhile(s => s == "#") : source);
			_usesHash = source.FirstOrDefault() == "#";
		}

		/// <summary>
		/// Parses a string containing a JSON Pointer.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <returns>A <see cref="JsonPointer"/> instance.</returns>
		public static JsonPointer Parse(string source)
		{
			var pointer = new JsonPointer();

			var parts = source.Split('/');
			if (parts.Length == 0) return pointer;


			if (parts[0] == "#")
				pointer._usesHash = true;
			else if (string.IsNullOrEmpty(parts[0]))
				parts = parts.Skip(1).ToArray();
			else
				parts = parts.ToArray();

			pointer.AddRange(parts.SkipWhile(s => s == "#").Select(_Unescape));

			return pointer;
		}

		/// <summary>
		/// Evaluates the pointer against a JSON instance.
		/// </summary>
		/// <param name="root">The JSON instance.</param>
		/// <returns>The element the pointer references, if any.</returns>
		public PointerEvaluationResults Evaluate(JsonValue root)
		{
			var upTo = new JsonPointer();
			var current = root;
			foreach (var segment in this)
			{
				upTo.Add(segment);
				current = _EvaluateSegment(current, segment);
				if (current == null)
					return new PointerEvaluationResults($"No value found at '{upTo}'");
			}

			return new PointerEvaluationResults(current);
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		/// <filterpriority>2</filterpriority>
		public override string ToString()
		{
			var asString = _usesHash
				? $"#/{string.Join("/", this.Select(_Escape))}"
				: $"/{string.Join("/", this.Select(_Escape))}";

			return asString == "/" ? asString : asString.TrimEnd('/');
		}

		/// <summary>
		/// Creates a copy of the pointer.
		/// </summary>
		public JsonPointer Clone()
		{
			return new JsonPointer(this){_usesHash = _usesHash};
		}

		/// <summary>
		/// Creates a copy of the pointer and appends new segments.
		/// </summary>
		/// <param name="append">The segments to append.</param>
		public JsonPointer CloneAndAppend(params string[] append)
		{
			var clone = new JsonPointer(this, Count + append.Length) {_usesHash = _usesHash};
			clone.AddRange(append);

			return clone;
		}

		/// <summary>
		/// Determines whether the pointer is a child of another pointer (starts with the same segments).
		/// </summary>
		/// <param name="pointer">Another pointer.</param>
		/// <returns>`true` if this pointer starts with all of the segments of <paramref name="pointer"/>; `false` otherwise.</returns>
		public bool IsChildOf(JsonPointer pointer)
		{
			return this.Take(pointer.Count).SequenceEqual(pointer);
		}

		internal JsonPointer WithHash()
		{
			return new JsonPointer(this) {_usesHash = true};
		}

		private static JsonValue _EvaluateSegment(JsonValue current, string segment)
		{
			if (current.Type == JsonValueType.Array)
			{
				if (int.TryParse(segment, out var index))
				{
					return (segment != "0" && segment.StartsWith("0")) ||
					       (0 > index || index >= current.Array.Count)
						       ? null
						       : current.Array[index];
				}

				if (segment == "-")
					return current.Array[current.Array.Count-1];
			}

			return current.Type != JsonValueType.Object || !current.Object.TryGetValue(segment, out var value)
				       ? null
				       : value;
		}

		private static string _Escape(string reference)
		{
			return reference.Replace("~", "~0")
				.Replace("/", "~1");
		}

		private static string _Unescape(string reference)
		{
			var unescaped = reference.Replace("~1", "/")
				.Replace("~0", "~");
			var matches = _generalEscapePattern.Matches(unescaped);
			foreach (Match match in matches)
			{
				var value = int.Parse(match.Groups["Value"].Value, NumberStyles.HexNumber);
				var ch = (char)value;
				unescaped = Regex.Replace(unescaped, match.Value, new string(ch, 1));
			}
			return unescaped;
		}

		/// <summary>
		/// Builds an object from a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="json">The <see cref="JsonValue"/> representation of the object.</param>
		/// <param name="serializer">The <see cref="JsonSerializer"/> instance to use for additional
		/// serialization of values.</param>
		public void FromJson(JsonValue json, JsonSerializer serializer)
		{
			AddRange(json.String.Split('/').Skip(1).SkipWhile(s => s == "#").Select(_Unescape));
		}

		/// <summary>
		/// Converts an object to a <see cref="JsonValue"/>.
		/// </summary>
		/// <param name="serializer">The <see cref="JsonSerializer"/> instance to use for additional
		/// serialization of values.</param>
		/// <returns>The <see cref="JsonValue"/> representation of the object.</returns>
		public JsonValue ToJson(JsonSerializer serializer)
		{
			return ToString();
		}
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(JsonPointer other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _usesHash == other._usesHash &&
			       this.SequenceEqual(other);
		}
		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object obj)
		{
			return Equals(obj as JsonPointer);
		}
		/// <summary>Serves as the default hash function. </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = _usesHash.GetHashCode();
				hashCode = (hashCode * 397) ^ this.GetCollectionHashCode();
				return hashCode;
			}
		}
	}
}
