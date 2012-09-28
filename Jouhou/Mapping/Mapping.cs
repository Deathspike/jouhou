// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents a mapping.
	/// </summary>
	public class Mapping : MappingAbstract<dynamic> {
		/// <summary>
		/// Contains the primary key.
		/// </summary>
		private readonly string _PrimaryKey;

		#region Abstract
		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		protected override async Task<List<dynamic>> _AllAsync(DbConnection Connection, string Query, object[] Arguments) {
			// Execute a query and retrieve the result.
			return await Connection.AllAsync(Query, Arguments);
		}

		/// <summary>
		/// Create insert values.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Action">The action.</param>
		protected override void _CreateInsertValues(dynamic Object, Action<string, object> Action) {
			// Retrieve the object as a dictionary.
			IDictionary<string, object> Dictionary = Object as IDictionary<string, object>;
			// Check if the dictionary is valid.
			if (Dictionary != null) {
				// Iterate through each value in the dictionary.
				foreach (KeyValuePair<string, object> KeyValuePair in Dictionary) {
					// Check if this property is not the primary key.
					if (!_Identity || string.Compare(KeyValuePair.Key, _PrimaryKey, StringComparison.InvariantCultureIgnoreCase) != 0) {
						// Invoke the action.
						Action(KeyValuePair.Key, KeyValuePair.Value);
					}
				}
			}
		}

		/// <summary>
		/// Create update values.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Action">The action.</param>
		protected override void _CreateUpdateValues(dynamic Object, Action<string, object> Action) {
			// Retrieve the object as a dictionary.
			IDictionary<string, object> Dictionary = Object as IDictionary<string, object>;
			// Check if the dictionary is valid.
			if (Dictionary != null) {
				// Iterate through each value in the dictionary.
				foreach (KeyValuePair<string, object> KeyValuePair in Dictionary) {
					// Check if this property is not the primary key.
					if (string.Compare(KeyValuePair.Key, _PrimaryKey, StringComparison.InvariantCultureIgnoreCase) != 0) {
						// Invoke the action.
						Action(KeyValuePair.Key, KeyValuePair.Value);
					}
				}
			}
		}

		/// <summary>
		/// Determines whether the object has a valid primary key.
		/// </summary>
		/// <param name="Object">The object.</param>
		protected override KeyValuePair<string, object> _HasValidPrimaryKey(dynamic Object) {
			// Check if the primary key is valid.
			if (_PrimaryKey != null) {
				// Retrieve the object as a dictionary.
				IDictionary<string, object> Dictionary = Object as IDictionary<string, object>;
				// Check if the dictionary is valid.
				if (Dictionary != null) {
					// Initialize the value.
					object Value;
					// Check if the primary key is available and valid.
					if (Dictionary.TryGetValue(_PrimaryKey, out Value) && Value != null) {
						// Return the value.
						return new KeyValuePair<string, object>(_PrimaryKey, Value);
					}
				}
			}
			// Return null.
			return default(KeyValuePair<string, object>);
		}

		/// <summary>
		/// Map a primary key value to the object.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Value">The value.</param>
		protected override void _MapPrimaryKey(dynamic Object, object Value) {
			// Retrieve the object as a dictionary.
			IDictionary<string, object> Dictionary = Object as IDictionary<string, object>;
			// Check if the dictionary is valid.
			if (Dictionary != null) {
				// Set the value.
				Dictionary[_PrimaryKey] = Value;
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		protected override async Task<dynamic> _SingleAsync(DbConnection Connection, string Query, object[] Arguments) {
			// Execute a query and retrieve the result.
			return await Connection.SingleAsync(Query, Arguments);
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initialize a new instance of the Mapping class.
		/// </summary>
		/// <param name="ConnectionPool">Contains the connection pool.</param>
		/// <param name="Identity">Indicates whether the primary key is an identity field.</param>
		/// <param name="PrimaryKey">The primary key.</param>
		/// <param name="TableName">The table name.</param>
		public Mapping(IConnectionPool ConnectionPool = null, string TableName = null, string PrimaryKey = null, bool Identity = false)
			: base(
				// The connection pool.
				ConnectionPool: ConnectionPool,
				// The value indicating whether the primary key is an identity field.
				Identity: Identity,
				// The columns selected on SELECT statement.
				SelectColumns: "*",
				// The table name.
				TableName: TableName) {
			// Set the primary key.
			_PrimaryKey = string.IsNullOrWhiteSpace(PrimaryKey) ? null : PrimaryKey;
		}
		#endregion
	}
}