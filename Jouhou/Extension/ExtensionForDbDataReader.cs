using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Dynamic;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents the class providing extensions for the DbDataReader class.
	/// </summary>
	public static class ExtensionForDbDataReader {
		/// <summary>
		/// Retrieve the result.
		/// </summary>
		/// <param name="DbDataReader">The data reader.</param>
		public static T ToResult<T>(this DbDataReader DbDataReader) where T : class, new() {
			// Check if a column is available.
			if (DbDataReader.FieldCount > 0) {
				// Initialize a new instance of the T class.
				T Object = new T();
				// Map the object properties from a data reader.
				UtilityMapper<T>.Map(Object, DbDataReader);
				// Return the object.
				return Object;
			}
			// Return null.
			return null;
		}

		/// <summary>
		/// Retrieve the result.
		/// </summary>
		/// <param name="DbDataReader">The data reader.</param>
		public static object ToResult(this DbDataReader DbDataReader) {
			// Check if a column is available.
			if (DbDataReader.FieldCount > 0) {
				// Initialize a new instance of the ExpandoObject class.
				IDictionary<string, object> ExpandoObject = new ExpandoObject();
				// Iterate through each column.
				for (int i = 0; i < DbDataReader.FieldCount; i++) {
					// Add the value to the ExpandoObject.
					ExpandoObject.Add(DbDataReader.GetName(i), DbDataReader[i]);
				}
				// Return the expando object.
				return ExpandoObject;
			}
			// Return null.
			return null;
		}
	}
}