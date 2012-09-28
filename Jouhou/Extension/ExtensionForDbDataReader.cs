// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;

namespace Jouhou {
	/// <summary>
	/// Represents the class providing extensions for the DbDataReader class.
	/// </summary>
	public static class ExtensionForDbDataReader {
		/// <summary>
		/// Retrieve the result.
		/// </summary>
		/// <param name="DataReader">The data reader.</param>
		public static T Single<T>(this DbDataReader DataReader) where T : class, new() {
			// Check if a column is available.
			if (DataReader.FieldCount > 0) {
				// Initialize a new instance of the T class.
				T Object = new T();
				// Map the object properties from a data reader.
				UtilityMapper<T>.Map(Object, DataReader);
				// Return the object.
				return Object;
			}
			// Return null.
			return null;
		}

		/// <summary>
		/// Retrieve the result.
		/// </summary>
		/// <param name="DataReader">The data reader.</param>
		public static object Single(this DbDataReader DataReader) {
			// Check if a column is available.
			if (DataReader.FieldCount > 0) {
				// Initialize a new instance of the ExpandoObject class.
				IDictionary<string, object> ExpandoObject = new UtilityExpandoObject();
				// Iterate through each column.
				for (int i = 0; i < DataReader.FieldCount; i++) {
					// Add the value to the ExpandoObject.
					ExpandoObject.Add(DataReader.GetName(i), DataReader[i]);
				}
				// Return the expando object.
				return ExpandoObject;
			}
			// Return null.
			return null;
		}
	}
}