using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents a database mapper (requires an instance per thread).
	/// </summary>
	/// <typeparam name="T">The type.</typeparam>
	public class DbMap<T> : IDisposable where T : class, new() {
		/// <summary>
		/// Contains a connection pool.
		/// </summary>
		private DbConnectionPool _DbConnectionPool;

		/// <summary>
		/// Contains the primary key.
		/// </summary>
		private PropertyInfo _PrimaryKey;

		/// <summary>
		/// Contains the table name.
		/// </summary>
		private string _TableName;

		/// <summary>
		/// Check the connection.
		/// </summary>
		private async Task<bool> _CheckConnection() {
			// Check if the connection is invalid.
			if (Connection == null) {
				// Check if the connection pool is invalid.
				if (_DbConnectionPool == null) {
					// Return false.
					return false;
				} else {
					// Fetch a connection.
					Connection = await _DbConnectionPool.Fetch();
				}
			}
			// Check if the connection state is invalid.
			if (Connection.State != ConnectionState.Open) {
				// Dispose of the connection.
				Connection.Dispose();
				// Remove the connection.
				Connection = null;
				// Check the connection.
				return await _CheckConnection();
			}
			// Return true.
			return true;
		}

		/// <summary>
		/// Initialize a new instance of the DbMap class.
		/// </summary>
		/// <param name="DbConnection">Contains the connection.</param>
		public DbMap(DbConnection DbConnection) {
			// Set the connection.
			Connection = DbConnection;
			// Set the table name.
			TableName = typeof(T).Name;
		}

		/// <summary>
		/// Initialize a new instance of the DbMap class.
		/// </summary>
		/// <param name="DbConnectionPool">Contains the connection pool.</param>
		public DbMap(DbConnectionPool DbConnectionPool) {
			// Set the connection pool.
			_DbConnectionPool = DbConnectionPool;
			// Set the table name.
			TableName = typeof(T).Name;
		}

		#region Map
		/// <summary>
		/// Execute a command and map an identity.
		/// </summary>
		/// <param name="DbCommand">The command.</param>
		/// <param name="Object">The object.</param>
		private async Task _Map(DbCommand DbCommand, T Object) {
			// Execute the query.
			await DbCommand.ExecuteNonQueryAsync();
			// Check if the value is assigned and is an identity field.
			if (Object != null && _PrimaryKey != null) {
				// Set the primary key.
				_PrimaryKey.SetValue(Object, await Connection.ToScalar("SELECT @@IDENTITY"));
			}
		}

		/// <summary>
		/// Execute a batch of commands and map an identity.
		/// </summary>
		/// <param name="CommandSet">The command set.</param>
		private async Task _MapBatch(List<KeyValuePair<DbCommand, T>> CommandSet) {
			// Check if the command set is valid and check the connection.
			if (CommandSet.Count != 0) {
				// Check if this is a single statement.
				if (CommandSet.Count == 1) {
					// Execute a command and map an identity.
					await _Map(CommandSet[0].Key, CommandSet[0].Value);
				} else {
					// Start a database transaction.
					using (DbTransaction DbTransaction = Connection.BeginTransaction()) {
						// Iterate through each command.
						foreach (KeyValuePair<DbCommand, T> KeyValuePair in CommandSet) {
							// Set the transaction.
							KeyValuePair.Key.Transaction = DbTransaction;
							// Execute a command and map an identity.
							await _Map(KeyValuePair.Key, KeyValuePair.Value);
						}
						// Commit the database transaction.
						DbTransaction.Commit();
					}
				}
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Objects">The objects.</param>
		public async Task Delete(IEnumerable<T> Objects) {
			// Check if the connection is valid.
			if (await _CheckConnection()) {
				// Initialize a new instance of the List class.
				List<KeyValuePair<DbCommand, T>> CommandSet = new List<KeyValuePair<DbCommand, T>>();
				// Iterate through each object.
				foreach (T Object in Objects) {
					// Check if the object is valid.
					if (Object != null) {
						// Create and return a command associated with the current connection.
						DbCommand DbCommand = Connection.CreateCommand();
						// Check if the primary key is available.
						if (_PrimaryKey != null) {
							// Retrieve the value of the primary key.
							object PrimaryKeyValue = _PrimaryKey.GetValue(Object);
							// Check if the value is valid and indicates an update.
							if (PrimaryKeyValue != null && !PrimaryKeyValue.Equals(Activator.CreateInstance(_PrimaryKey.PropertyType))) {
								// Set the command.
								DbCommand.CommandText = string.Format("DELETE FROM {0} WHERE {1} = @{2}",
									// ... with the table name ...
									TableName,
									// ... with the primary key ...
									_PrimaryKey.Name,
									// ... and with the parameter count.
									DbCommand.Parameters.Count
								);
								// Add the argument to the command.
								DbCommand.Add(PrimaryKeyValue);
								// Add the command to the command set.
								CommandSet.Add(new KeyValuePair<DbCommand, T>(DbCommand, null));
								// Continue iteration.
								continue;
							}
						}
					}
				}
				// Execute a batch of commands and map an identity.
				await _MapBatch(CommandSet);
			}
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Objects">The objects.</param>
		public async Task Delete(params T[] Objects) {
			// Check if an object is available.
			if (Objects != null && Objects.Length != 0) {
				// Save each object.
				await Delete(Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Objects">The objects.</param>
		public async Task Save(IEnumerable<T> Objects) {
			// Check if the connection is valid.
			if (await _CheckConnection()) {
				// Initialize a new instance of the List class.
				List<KeyValuePair<DbCommand, T>> CommandSet = new List<KeyValuePair<DbCommand, T>>();
				// Iterate through each object.
				foreach (T Object in Objects) {
					// Check if the object is valid.
					if (Object != null) {
						// Create and return a command associated with the current connection.
						DbCommand DbCommand = Connection.CreateCommand();
						// Check if the primary key is available.
						if (_PrimaryKey != null) {
							// Retrieve the value of the primary key.
							object PrimaryKeyValue = _PrimaryKey.GetValue(Object);
							// Check if the value is valid and indicates an update.
							if (PrimaryKeyValue != null && !PrimaryKeyValue.Equals(Activator.CreateInstance(_PrimaryKey.PropertyType))) {
								// Initialize a new instance of the StringBuilder class.
								StringBuilder StringBuilder = new StringBuilder();
								// Map the object properties to an action.
								UtilityMapper<T>.Map(Object, (Key, Value) => {
									// Check if this property is not the primary key.
									if (_PrimaryKey.Name != Key) {
										// Append the set command.
										StringBuilder.AppendFormat("{0} = @{1},", Key, DbCommand.Parameters.Count);
										// Add the argument to the command.
										DbCommand.Add(Value);
									}
								});
								// Set the command.
								DbCommand.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2} = @{3}",
									// ... with the table name ...
									TableName,
									// ... with the values ...
									StringBuilder.ToString().Substring(0, StringBuilder.Length - 1),
									// ... with the primary key ...
									_PrimaryKey.Name,
									// ... and with the parameter count.
									DbCommand.Parameters.Count
								);
								// Add the argument to the command.
								DbCommand.Add(PrimaryKeyValue);
								// Add the command to the command set.
								CommandSet.Add(new KeyValuePair<DbCommand, T>(DbCommand, null));
								// Continue iteration.
								continue;
							}
						}
						// It seems that this was not an update, but an insert ...
						if (1 == 1) {
							// Initialize a new instance of the StringBuilder class.
							StringBuilder StringBuilderColumns = new StringBuilder();
							// Initialize a new instance of the StringBuilder class.
							StringBuilder StringBuilderValues = new StringBuilder();
							// Map the object properties to an action.
							UtilityMapper<T>.Map(Object, (Key, Value) => {
								// Check if this property is not the primary key.
								if (!Identity || _PrimaryKey.Name != Key) {
									// Append the column to the command.
									StringBuilderColumns.AppendFormat("{0},", Key);
									// Append the value to the command.
									StringBuilderValues.AppendFormat("@{0},", DbCommand.Parameters.Count);
									// Add the argument to the command.
									DbCommand.Add(Value);
								}
							});
							// Set the command ...
							DbCommand.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
								// ... with the table name ...
								TableName,
								// ... with the columns ...
								StringBuilderColumns.ToString().Substring(0, StringBuilderColumns.Length - 1),
								// ... and with the values.
								StringBuilderValues.ToString().Substring(0, StringBuilderValues.Length - 1)
							);
							// Add the command to the command set.
							CommandSet.Add(new KeyValuePair<DbCommand, T>(DbCommand, Identity ? Object : null));
						}
					}
				}
				// Execute a batch of commands and map an identity.
				await _MapBatch(CommandSet);
			}
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Objects">The objects.</param>
		public async Task Save(params T[] Objects) {
			// Check if an object is available.
			if (Objects != null && Objects.Length != 0) {
				// Save each object.
				await Save(Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		public async Task<T> ToResult() {
			// Execute a query and retrieve the result.
			return await ToResult(null);
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<T> ToResult(string Query, params object[] Arguments) {
			// Check if the connection is valid.
			if (await _CheckConnection()) {
				// Check if the query is invalid.
				if (string.IsNullOrWhiteSpace(Query)) {
					// Execute a query and retrieve the result.
					return await Connection.ToResult<T>(string.Format("SELECT * FROM {0} LIMIT 1", TableName));
				} else if (!Query.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)) {
					// Retrieve a value indicating whether a limit does not exist.
					bool DoesNotLimit = !Regex.Match(Query, @"LIMIT\s?([0-9]+),?\s?([0-9]+)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled).Success;
					// Execute a query and retrieve the result.
					return await Connection.ToResult<T>(string.Format("SELECT * FROM {0} {1} {2}", TableName, Query, DoesNotLimit ? "LIMIT 1" : null), Arguments);
				} else {
					// Execute a query and retrieve the result.
					return await Connection.ToResult<T>(Query, Arguments);
				}
			}
			// Return null.
			return null;
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		public async Task<IList<T>> ToResultSet() {
			// Execute a query and retrieve the result set.
			return await ToResultSet(null);
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<IList<T>> ToResultSet(string Query, params object[] Arguments) {
			// Check if the connection is valid.
			if (await _CheckConnection()) {
				// Check if the query is invalid.
				if (string.IsNullOrWhiteSpace(Query)) {
					// Execute a query and retrieve the result set.
					return await Connection.ToResultSet<T>(string.Format("SELECT * FROM {0}", TableName), Arguments);
				} else if (!Query.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)) {
					// Execute a query and retrieve the result set.
					return await Connection.ToResultSet<T>(string.Format("SELECT * FROM {0} {1}", TableName, Query), Arguments);
				} else {
					// Execute a query and retrieve the result set.
					return await Connection.ToResultSet<T>(Query, Arguments);
				}
			}
			// Return null.
			return null;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Contains the connection.
		/// </summary>
		public DbConnection Connection { get; set; }

		/// <summary>
		/// Indicates whether the primary key is an identity field.
		/// </summary>
		public bool Identity { get; set; }

		/// <summary>
		/// Contains the primary key.
		/// </summary>
		public string PrimaryKey {
			get {
				// Return the name of the primary key.
				return _PrimaryKey == null ? null : _PrimaryKey.Name;
			}
			set {
				// Check if the value is valid.
				if (!string.IsNullOrWhiteSpace(value)) {
					// Set the primary key.
					_PrimaryKey = typeof(T).GetProperties().SingleOrDefault(x => x.CanRead && x.CanWrite && string.Compare(x.Name, value, StringComparison.InvariantCultureIgnoreCase) == 0);
				}
			}
		}

		/// <summary>
		/// Contains the table name.
		/// </summary>
		public string TableName {
			get {
				// Get the table name.
				return _TableName;
			}
			set {
				// Check if the value is valid.
				if (!string.IsNullOrWhiteSpace(value)) {
					// Set the table name.
					_TableName = value;
				}
			}
		}
		#endregion

		#region IDisposable
		/// <summary>
		/// Dispose of the object.
		/// </summary>
		public void Dispose() {
			// Check if the connection is valid.
			if (Connection != null) {
				// Check if the connection pool is valid.
				if (_DbConnectionPool != null) {
					// Recycle the connection.
					_DbConnectionPool.Recycle(Connection);
				} else {
					// Dispose of the connection.
					Connection.Dispose();
				}
				// Remove the connection.
				Connection = null;
			}
		}
		#endregion
	}
}