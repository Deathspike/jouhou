// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Jouhou {
	/// <summary>
	/// Represents the object mapper.
	/// </summary>
	/// <typeparam name="T">The type</typeparam>
	public static class UtilityMapper<T> {
		/// <summary>
		/// Contains each property.
		/// </summary>
		private static readonly Dictionary<string, PropertyInfo> _Properties;

		/// <summary>
		/// Initialize the UtilityMapper class.
		/// </summary>
		static UtilityMapper() {
			// Set each property.
			_Properties = typeof(T).GetProperties().Where(x => x.CanRead && x.CanWrite).ToDictionary(p => p.Name.ToLower(), p => p);
		}

		/// <summary>
		/// Map the object properties from a data reader.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="DataReader">The data reader.</param>
		public static void Map(T Object, IDataReader DataReader) {
			// Initialize the property information.
			PropertyInfo PropertyInfo;
			// Iterate through each column.
			for (int i = 0; i < DataReader.FieldCount; i++) {
				// Retrieve the column name.
				string Name = DataReader.GetName(i);
				// Check if the property exists in the object.
				if (_Properties.TryGetValue(Name.ToLowerInvariant(), out PropertyInfo)) {
					// Retrieve the value.
					object Value = DataReader[i];
					// Check if the value is valid and equals the target type.
					if (Value != null && Value.GetType().Equals(PropertyInfo.PropertyType)) {
						// Set the value.
						PropertyInfo.SetValue(Object, Value, null);
					}
				}
			}
		}

		/// <summary>
		/// Map the object properties to an action.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Invoke">The action.</param>
		public static void Map(T Object, Action<string, object> Invoke) {
			// Iterate through each property.
			foreach (KeyValuePair<string, PropertyInfo> KeyValuePair in _Properties) {
				// Invoke the action.
				Invoke(KeyValuePair.Value.Name, KeyValuePair.Value.GetValue(Object));
			}
		}
	}
}