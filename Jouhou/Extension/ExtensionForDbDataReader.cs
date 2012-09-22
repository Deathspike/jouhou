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
		public static async Task<T> ToResult<T>(this DbDataReader DbDataReader) where T : class, new() {
			// Read a value from the data reader.
			if (await DbDataReader.ReadAsync()) {
				// Check if a column is available.
				if (DbDataReader.FieldCount > 0) {
					// Initialize a new instance of the T class.
					T Object = new T();
					// Map the object properties from a data reader.
					UtilityMapper<T>.Map(Object, DbDataReader);
					// Return the object.
					return Object;
				}
			}
			// Return null.
			return null;
		}

		/// <summary>
		/// Retrieve the result.
		/// </summary>
		/// <param name="DbDataReader">The data reader.</param>
		public static async Task<object> ToResult(this DbDataReader DbDataReader) {
			// Read a value from the data reader.
			if (await DbDataReader.ReadAsync()) {
				// Check if a column is available.
				if (DbDataReader.FieldCount > 0) {
					// Initialize a new instance of the ExpandoObject class.
					ExpandoObject ExpandoObject = new ExpandoObject();
					// Retrieve the ExpandoObject as a dictionary.
					IDictionary<string, object> Dictionary = ExpandoObject as IDictionary<string, object>;
					// Iterate through each column.
					for (int i = 0; i < DbDataReader.FieldCount; i++) {
						// Retrieve the value.
						object Value = DbDataReader[i];
						// Add the value to the ExpandoObject.
						Dictionary.Add(DbDataReader.GetName(i), DBNull.Value.Equals(Value) ? null : Value);
					}
					// Return the expando object.
					return ExpandoObject;
				}
			}
			// Return null.
			return null;
		}
	}
}