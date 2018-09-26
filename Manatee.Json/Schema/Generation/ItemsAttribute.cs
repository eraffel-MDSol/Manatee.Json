using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Manatee.Json.Schema.Generation
{
	public class ItemsAttribute : ValidationAttribute
	{
		private readonly Type[] _attributeTypes;
		private string _errorMessage;
		private int _index;

		public ItemsAttribute(params Type[] attributes)
		{
			if (attributes == null)
				throw new ArgumentNullException(nameof(attributes));

			var validationAttributeType = typeof(ValidationAttribute).GetTypeInfo();
			if (attributes.Any(x => !validationAttributeType.IsAssignableFrom(x.GetTypeInfo())))
				throw new ArgumentException($"Validation types must derive from {nameof(ValidationAttribute)}");

			_attributeTypes = attributes;
		}

		public override bool IsValid(object value)
		{
			if (!(value is IEnumerable enumerable) || value is string)
				return true;

			var index = 0;
			foreach (var item in enumerable)
			{
				foreach (var type in _attributeTypes)
				{
					var validator = Activator.CreateInstance(type) as ValidationAttribute;

					// ReSharper disable once PossibleNullReferenceException
					var validationResult = validator.GetValidationResult(item, new ValidationContext(item, null, null));
					if (validationResult != null)
					{
						_errorMessage = validationResult.ErrorMessage;
						_index = index;
						return false;
					}
				}
				index++;
			}
			return true;
		}

		public override string FormatErrorMessage(string name)
		{
			return $"Error at {name}[{_index}]: {_errorMessage}";
		}
	}
}
