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
 
	File Name:		ObjectCaster.cs
	Namespace:		Manatee.Json.Helpers
	Class Name:		ObjectCaster
	Purpose:		Provides type-safe generic casting with additional functionality.

***************************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Manatee.Json.Helpers
{
	internal class ObjectCaster : IObjectCaster
	{
		private static readonly Dictionary<Type, MethodInfo> Cache = new Dictionary<Type, MethodInfo>();

		public bool TryCast<T>(object obj, out T result)
		{
			try
			{
				result = (T) obj;
				return true;
			}
			catch (InvalidCastException)
			{
				var type = typeof (T);
				result = default(T);
				MethodInfo parseMethod;
				if (Cache.ContainsKey(type))
					parseMethod = Cache[type];
				else
				{
					parseMethod = type.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
					                                   null, new[] {typeof (string), typeof (T).MakeByRefType()}, null);
					Cache.Add(type, parseMethod);
				}
				if (parseMethod == null) return false;
				var paramsList = new object[] {obj.ToString(), result};
				if ((bool) parseMethod.Invoke(null, paramsList))
				{
					result = (T) paramsList[1];
					return true;
				}
			}
			catch
			{
				result = default (T);
			}
			return false;
		}

		public T Cast<T>(object obj)
		{
			try
			{
				return (T) obj;
			}
			catch (InvalidCastException)
			{
				return default (T);
			}
		}
	}
}
