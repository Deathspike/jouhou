using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents the class providing extensions for the DbConnection class.
	/// </summary>
	public static class ExtensionForDbConnection {
		/// <summary>
		/// Create and return a command associated with the current connection.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static DbCommand CreateCommand(this DbConnection DbConnection, string Query, params object[] Arguments) {
			// Create a command associated with the connection.
			DbCommand DbCommand = DbConnection.CreateCommand();
			// Set the text command.
			DbCommand.CommandText = Query;
			// Add a number of arguments to the command.
			DbCommand.Add(Arguments);
			// Return the command.
			return DbCommand;
		}

		/// <summary>
		/// Execute a query and retrieve the number of affected rows.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<int> ToAffected(this DbConnection DbConnection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbConnection.CreateCommand(Query, Arguments)) {
				// Execute a query.
				return await Command.ExecuteNonQueryAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the scalar.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<T> ToScalar<T>(this DbConnection DbConnection, string Query, params object[] Arguments) {
			// Execute a query and retrieve the scalar.
			return (T) await DbConnection.ToScalar(Query, Arguments);
		}

		/// <summary>
		/// Execute a query and retrieve the scalar.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<object> ToScalar(this DbConnection DbConnection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbConnection.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.ExecuteScalarAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<T> ToResult<T>(this DbConnection DbConnection, string Query, params object[] Arguments) where T : class, new() {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbConnection.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.ToResult<T>();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<dynamic> ToResult(this DbConnection DbConnection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbConnection.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.ToResult();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<List<T>> ToResultSet<T>(this DbConnection DbConnection, string Query, params object[] Arguments) where T : class, new() {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbConnection.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result set.
				return await Command.ToResultSet<T>();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<List<dynamic>> ToResultSet(this DbConnection DbConnection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbConnection.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result set.
				return await Command.ToResultSet();
			}
		}
	}
}