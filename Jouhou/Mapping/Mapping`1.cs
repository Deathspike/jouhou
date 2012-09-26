using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents a mapping.
	/// </summary>
	/// <typeparam name="T">The type.</typeparam>
	public class Mapping<T> : MappingAbstract<T> where T : class, new() {
		/// <summary>
		/// Contains the primary key.
		/// </summary>
		private readonly PropertyInfo _PrimaryKey;

		#region Abstract
		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		protected override async Task<List<T>> _AllAsync(DbConnection Connection, string Query, object[] Arguments) {
			// Execute a query and retrieve the result.
			return await Connection.AllAsync<T>(Query, Arguments);
		}

		/// <summary>
		/// Create insert values.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Action">The action.</param>
		protected override void _CreateInsertValues(T Object, Action<string, object> Action) {
			// Map the object properties to an action.
			UtilityMapper<T>.Map(Object, (Key, Value) => {
				// Check if this property is not the primary key.
				if (!_Identity || _PrimaryKey.Name != Key) {
					// Invoke the action.
					Action(Key, Value);
				}
			});
		}

		/// <summary>
		/// Create update values.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Action">The action.</param>
		protected override void _CreateUpdateValues(T Object, Action<string, object> Action) {
			// Map the object properties to an action.
			UtilityMapper<T>.Map(Object, (Key, Value) => {
				// Check if this property is not the primary key.
				if (_PrimaryKey.Name != Key) {
					// Invoke the action.
					Action(Key, Value);
				}
			});
		}

		/// <summary>
		/// Determines whether the object has a valid primary key and returns the value.
		/// </summary>
		/// <param name="Object">The object.</param>
		protected override KeyValuePair<string, object> _HasValidPrimaryKey(T Object) {
			// Check if the primary key is available.
			if (_PrimaryKey != null) {
				// Retrieve the value of the primary key.
				object PrimaryKeyValue = _PrimaryKey.GetValue(Object);
				// Check if the value is valid and indicates an update.
				if (PrimaryKeyValue != null && !PrimaryKeyValue.Equals(Activator.CreateInstance(_PrimaryKey.PropertyType))) {
					// Return the value.
					return new KeyValuePair<string, object>(_PrimaryKey.Name, PrimaryKeyValue);
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
		protected override void _MapPrimaryKey(T Object, object Value) {
			// Set the value.
			_PrimaryKey.SetValue(Object, Value);
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		protected override async Task<T> _SingleAsync(DbConnection Connection, string Query, object[] Arguments) {
			// Execute a query and retrieve the result.
			return await Connection.SingleAsync<T>(Query, Arguments);
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
				SelectColumns: string.Join(",", typeof(T).GetProperties().Where(x => x.CanRead && x.CanWrite).Select(x => x.Name)),
				// The table name.
				TableName: TableName) {
			// Set the primary key.
			_PrimaryKey = typeof(T).GetProperties().SingleOrDefault(x => x.CanRead && x.CanWrite && string.Compare(x.Name, PrimaryKey, StringComparison.InvariantCultureIgnoreCase) == 0);
		}
		#endregion
	}
}