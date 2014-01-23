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
 
	File Name:		JsonCompatibleImplementationClass.cs
	Namespace:		Manatee.Tests.Test_References
	Class Name:		JsonCompatibleImplementationClass
	Purpose:		Basic class that implements IJsonCompatible to be used in
					testing the Manatee.Json library.

***************************************************************************************/
using Manatee.Json;
using Manatee.Json.Serialization;

namespace Manatee.Tests.Test_References
{
	public class JsonCompatibleImplementationClass : ImplementationClass, IJsonCompatible
	{
		public void FromJson(JsonValue json)
		{
			RequiredProp = json.Object["requiredProp"].String;
		}
		public JsonValue ToJson()
		{
			return new JsonObject { { "requiredProp", RequiredProp } };
		}
	}
}