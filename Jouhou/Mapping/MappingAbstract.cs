// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents a database mapping abstraction.
	/// </summary>
	/// <typeparam name="T">The type.</typeparam>
	public abstract class MappingAbstract<T> where T : class, new() {
		/// <summary>
		/// Contains a connection pool.
		/// </summary>
		protected readonly IConnectionPool _ConnectionPool;

		/// <summary>
		/// Indicates whether the primary key is an identity field.
		/// </summary>
		protected readonly bool _Identity;

		/// <summary>
		/// Contains the columns selected on SELECT statement.
		/// </summary>
		private readonly string _SelectColumns;

		/// <summary>
		/// Contains the table name.
		/// </summary>
		protected readonly string _TableName;

		#region Abstract
		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		protected abstract Task<List<T>> _AllAsync(DbConnection Connection, string Query, object[] Arguments);

		/// <summary>
		/// Create insert values.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Action">The action.</param>
		protected abstract void _CreateInsertValues(T Object, Action<string, object> Action);

		/// <summary>
		/// Create update values.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Action">The action.</param>
		protected abstract void _CreateUpdateValues(T Object, Action<string, object> Action);

		/// <summary>
		/// Determines whether the object has a valid primary key and returns the value.
		/// </summary>
		/// <param name="Object">The object.</param>
		protected abstract KeyValuePair<string, object> _HasValidPrimaryKey(T Object);

		/// <summary>
		/// Map a primary key value to the object.
		/// </summary>
		/// <param name="Object">The object.</param>
		/// <param name="Value">The value.</param>
		protected abstract void _MapPrimaryKey(T Object, object Value);

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		protected abstract Task<T> _SingleAsync(DbConnection Connection, string Query, object[] Arguments);
		#endregion

		#region Batch
		/// <summary>
		/// Execute a command.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Command">The command.</param>
		/// <param name="Object">The object.</param>
		private async Task _BatchAsync(DbConnection Connection, DbCommand Command, T Object) {
			// Execute the query.
			await Command.ExecuteNonQueryAsync();
			// Check of the object is valid and a primary key has been set.
			if (Object != null) {
				// Map a primary key value to the object.
				_MapPrimaryKey(Object, await Connection.ScalarAsync("SELECT @@IDENTITY"));
			}
		}

		/// <summary>
		/// Execute a batch of commands.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="CommandSet">The command.</param>
		private async Task _BatchAsync(DbConnection Connection, List<KeyValuePair<DbCommand, T>> CommandSet) {
			// Check if the command set is valid and check the connection.
			if (CommandSet.Count != 0) {
				// Check if this is a single statement.
				if (CommandSet.Count == 1) {
					// Execute a command and map an identity.
					await _BatchAsync(Connection, CommandSet[0].Key, CommandSet[0].Value);
				} else {
					// Start a database transaction.
					using (DbTransaction DbTransaction = Connection.BeginTransaction()) {
						// Iterate through each command.
						foreach (KeyValuePair<DbCommand, T> KeyValuePair in CommandSet) {
							// Set the transaction.
							KeyValuePair.Key.Transaction = DbTransaction;
							// Execute a command and map an identity.
							await _BatchAsync(Connection, KeyValuePair.Key, KeyValuePair.Value);
						}
						// Commit the database transaction.
						DbTransaction.Commit();
					}
				}
			}
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initialize a new instance of the MappingAbstract class.
		/// </summary>
		/// <param name="ConnectionPool">Contains the connection pool.</param>
		/// <param name="Identity">Indicates whether the primary key is an identity field.</param>
		/// <param name="SelectColumns">The columns selected on SELECT statement.</param>
		/// <param name="TableName">The table name.</param>
		public MappingAbstract(IConnectionPool ConnectionPool, bool Identity, string SelectColumns, string TableName) {
			// Set the connection pool.
			_ConnectionPool = ConnectionPool;
			// Set the value indicating whether the primary key is an identity field.
			_Identity = Identity;
			// Set the columns selected on a SELECT statement.
			_SelectColumns = SelectColumns;
			// Set the table name.
			_TableName = string.IsNullOrWhiteSpace(TableName) ? typeof(T).Name : TableName;
		}
		#endregion

		#region Queries
		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<List<T>> AllAsync(DbConnection Connection, DbTransaction Transaction, string Query, params object[] Arguments) {
			// Check if the query is invalid.
			if (string.IsNullOrWhiteSpace(Query)) {
				// Execute a query and retrieve the result set.
				return await _AllAsync(Connection, string.Format("SELECT {0} FROM {1}", _SelectColumns, _TableName), Arguments);
			} else if (!Query.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)) {
				// Execute a query and retrieve the result set.
				return await _AllAsync(Connection, string.Format("SELECT {0} FROM {1} {2}", _SelectColumns, _TableName, Query), Arguments);
			} else {
				// Execute a query and retrieve the result set.
				return await _AllAsync(Connection, Query, Arguments);
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<List<T>> AllAsync(DbConnection Connection, string Query, params object[] Arguments) {
			// Execute a query and retrieve the result set.
			return await AllAsync(Connection, null, Query, Arguments);
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<List<T>> AllAsync(DbTransaction Transaction, string Query, params object[] Arguments) {
			// Execute a query and retrieve the result set.
			return await AllAsync(Transaction.Connection, Transaction, Query, Arguments);
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<List<T>> AllAsync(string Query, params object[] Arguments) {
			DbConnection Connection = await _ConnectionPool.OpenAsync();
			// Attempt the following code.
			try {
				// Execute a query and retrieve the result set.
				return await AllAsync(Query, Arguments);
			} finally {
				// Close a connection.
				_ConnectionPool.Close(Connection);
			}
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Objects">The objects.</param>
		public async Task DeleteAsync(DbConnection Connection, DbTransaction Transaction, IEnumerable<T> Objects) {
			// Initialize a new instance of the List class.
			List<KeyValuePair<DbCommand, T>> CommandSet = new List<KeyValuePair<DbCommand, T>>();
			// Iterate through each object.
			foreach (T Object in Objects) {
				// Check if the object is valid.
				if (Object != null) {
					// Retrieve the primary key.
					KeyValuePair<string, object> PrimaryKey = _HasValidPrimaryKey(Object);
					// Check whether the object has a valid primary key.
					if (PrimaryKey.Value != null) {
						// Create and return a command associated with the current connection.
						DbCommand Command = Connection.CreateCommand();
						// Set the command.
						Command.CommandText = string.Format("DELETE FROM {0} WHERE {1} = @{2}",
							// ... with the table name ...
							_TableName,
							// ... with the primary key ...
							PrimaryKey.Key,
							// ... and with the parameter count.
							Command.Parameters.Count
						);
						// Set the transaction.
						Command.Transaction = Transaction;
						// Add the argument to the command.
						Command.Add(PrimaryKey.Value);
						// Add the command to the command set.
						CommandSet.Add(new KeyValuePair<DbCommand, T>(Command, null));
					}
				}
			}
			// Execute a batch of commands and map an identity.
			await _BatchAsync(Connection, CommandSet);
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Objects">The objects.</param>
		public async Task DeleteAsync(DbConnection Connection, DbTransaction Transaction, params T[] Objects) {
			// Check if an object is available.
			if (Objects != null) {
				// Delete each object.
				await DeleteAsync(Connection, Transaction, Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Objects">The objects.</param>
		public async Task DeleteAsync(DbConnection Connection, IEnumerable<T> Objects) {
			// Delete each object.
			await DeleteAsync(Connection, null, Objects);
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Objects">The objects.</param>
		public async Task DeleteAsync(DbConnection Connection, params T[] Objects) {
			// Check if an object is available.
			if (Objects != null) {
				// Delete each object.
				await DeleteAsync(Connection, Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Objects">The objects.</param>
		public async Task DeleteAsync(DbTransaction Transaction, IEnumerable<T> Objects) {
			// Delete each object.
			await DeleteAsync(Transaction.Connection, Transaction, Objects);
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Objects">The objects.</param>
		public async Task DeleteAsync(DbTransaction Transaction, params T[] Objects) {
			// Check if an object is available.
			if (Objects != null) {
				// Delete each object.
				await DeleteAsync(Transaction, Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Objects">The objects.</param>
		public async Task DeleteAsync(IEnumerable<T> Objects) {
			// Open a connection.
			DbConnection Connection = await _ConnectionPool.OpenAsync();
			// Attempt the following code.
			try {
				// Delete each object.
				await DeleteAsync(Connection, Objects);
			} finally {
				// Close a connection.
				_ConnectionPool.Close(Connection);
			}
		}

		/// <summary>
		/// Delete each object.
		/// </summary>
		/// <param name="Objects">The objects.</param>
		public async Task DeleteAsync(params T[] Objects) {
			// Check if an object is available.
			if (Objects != null) {
				// Save each object.
				await DeleteAsync(Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Objects">The objects.</param>
		public async Task SaveAsync(DbConnection Connection, DbTransaction Transaction, IEnumerable<T> Objects) {
			// Initialize a new instance of the List class.
			List<KeyValuePair<DbCommand, T>> CommandSet = new List<KeyValuePair<DbCommand, T>>();
			// Iterate through each object.
			foreach (T Object in Objects) {
				// Check if the object is valid.
				if (Object != null) {
					// Create a command associated with the connection.
					DbCommand Command = Connection.CreateCommand();
					// Initialize the validation boolean.
					bool IsValid = false;
					// Retrieve the primary key.
					KeyValuePair<string, object> PrimaryKey = _HasValidPrimaryKey(Object);
					// Check whether the object has a valid primary key.
					if (PrimaryKey.Value != null) {
						// Initialize a new instance of the StringBuilder class.
						StringBuilder StringBuilder = new StringBuilder();
						// Create an update command.
						_CreateUpdateValues(Object, (Key, Value) => {
							// Set the validation boolean.
							IsValid = true;
							// Append the set command.
							StringBuilder.AppendFormat("{0} = @{1},", Key, Command.Parameters.Count);
							// Add the argument to the command.
							Command.Add(Value);
						});
						// Check if the validation boolean has been set.
						if (IsValid) {
							// Set the command.
							Command.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2} = @{3}",
								// ... with the table name ...
								_TableName,
								// ... with the values ...
								StringBuilder.ToString().Substring(0, StringBuilder.Length - 1),
								// ... with the primary key ...
								PrimaryKey.Key,
								// ... and with the parameter count.
								Command.Parameters.Count
							);
							// Add the argument to the command.
							Command.Add(PrimaryKey.Value);
							// Add the command to the command set.
							CommandSet.Add(new KeyValuePair<DbCommand, T>(Command, null));
						}
					} else {
						// Initialize a new instance of the StringBuilder class.
						StringBuilder StringBuilderColumns = new StringBuilder();
						// Initialize a new instance of the StringBuilder class.
						StringBuilder StringBuilderValues = new StringBuilder();
						// Create an insert command.
						_CreateInsertValues(Object, (Key, Value) => {
							// Set the validation boolean.
							IsValid = true;
							// Append the column to the command.
							StringBuilderColumns.AppendFormat("{0},", Key);
							// Append the value to the command.
							StringBuilderValues.AppendFormat("@{0},", Command.Parameters.Count);
							// Add the argument to the command.
							Command.Add(Value);
						});
						// Check if the validation boolean has been set.
						if (IsValid) {
							// Set the command ...
							Command.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
								// ... with the table name ...
								_TableName,
								// ... with the columns ...
								StringBuilderColumns.ToString().Substring(0, StringBuilderColumns.Length - 1),
								// ... and with the values.
								StringBuilderValues.ToString().Substring(0, StringBuilderValues.Length - 1)
							);
							// Add the command to the command set.
							CommandSet.Add(new KeyValuePair<DbCommand, T>(Command, _Identity ? Object : null));
						}
					}
				}
			}
			// Execute a batch of commands.
			await _BatchAsync(Connection, CommandSet);
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Objects">The objects.</param>
		public async Task SaveAsync(DbConnection Connection, DbTransaction Transaction, params T[] Objects) {
			// Check if an object is available.
			if (Objects != null) {
				// Save each object.
				await SaveAsync(Connection, Transaction, Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Objects">The objects.</param>
		public async Task SaveAsync(DbConnection Connection, IEnumerable<T> Objects) {
			// Save each object.
			await SaveAsync(Connection, null, Objects);
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Objects">The objects.</param>
		public async Task SaveAsync(DbConnection Connection, params T[] Objects) {
			// Check if an object is available.
			if (Objects != null) {
				// Save each object.
				await SaveAsync(Connection, Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Objects">The objects.</param>
		public async Task SaveAsync(DbTransaction Transaction, IEnumerable<T> Objects) {
			// Save each object.
			await SaveAsync(Transaction.Connection, Transaction, Objects);
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Objects">The objects.</param>
		public async Task SaveAsync(DbTransaction Transaction, params T[] Objects) {
			// Check if an object is available.
			if (Objects != null) {
				// Save each object.
				await SaveAsync(Transaction, Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Objects">The objects.</param>
		public async Task SaveAsync(IEnumerable<T> Objects) {
			// Open a connection.
			DbConnection Connection = await _ConnectionPool.OpenAsync();
			// Attempt the following code.
			try {
				// Save each object.
				await SaveAsync(Connection, Objects);
			} finally {
				// Close a connection.
				_ConnectionPool.Close(Connection);
			}
		}

		/// <summary>
		/// Save each object.
		/// </summary>
		/// <param name="Objects">The objects.</param>
		public async Task SaveAsync(params T[] Objects) {
			// Check if an object is available.
			if (Objects != null) {
				// Save each object.
				await SaveAsync(Objects as IEnumerable<T>);
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<T> SingleAsync(DbConnection Connection, DbTransaction Transaction, string Query, params object[] Arguments) {
			// Check if the query is invalid.
			if (string.IsNullOrWhiteSpace(Query)) {
				// Execute a query and retrieve the result.
				return await _SingleAsync(Connection, string.Format("SELECT {0} FROM {1} LIMIT 1", _SelectColumns, _TableName), Arguments);
			} else if (!Query.StartsWith("SELECT", StringComparison.InvariantCultureIgnoreCase)) {
				// Retrieve a value indicating whether a limit does not exist.
				bool DoesNotLimit = !Regex.Match(Query, @"LIMIT\s?([0-9]+),?\s?([0-9]+)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled).Success;
				// Execute a query and retrieve the result.
				return await _SingleAsync(Connection, string.Format("SELECT {0} FROM {1} {2} {3}", _SelectColumns, _TableName, Query, DoesNotLimit ? "LIMIT 1" : null), Arguments);
			} else {
				// Execute a query and retrieve the result.
				return await _SingleAsync(Connection, Query, Arguments);
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<T> SingleAsync(DbConnection Connection, string Query, params object[] Arguments) {
			// Execute a query and retrieve the result.
			return await SingleAsync(Connection, null, Query, Arguments);
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<T> SingleAsync(DbTransaction Transaction, string Query, params object[] Arguments) {
			// Execute a query and retrieve the result.
			return await SingleAsync(Transaction.Connection, Transaction, Query, Arguments);
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public async Task<T> SingleAsync(string Query, params object[] Arguments) {
			DbConnection Connection = await _ConnectionPool.OpenAsync();
			// Attempt the following code.
			try {
				// Execute a query and retrieve the result.
				return await SingleAsync(Query, Arguments);
			} finally {
				// Close a connection.
				_ConnectionPool.Close(Connection);
			}
		}
		#endregion
	}
}