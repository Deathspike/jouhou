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
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		private static DbCommand _CreateCommand(this DbConnection Connection, string Query, params object[] Arguments) {
			// Create a command associated with the connection.
			DbCommand Command = Connection.CreateCommand();
			// Set the text command.
			Command.CommandText = Query;
			// Add a number of arguments to the command.
			Command.Add(Arguments);
			// Return the command.
			return Command;
		}

		/// <summary>
		/// Execute a query and retrieve the number of affected rows.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<int> AffectedAsync(this DbConnection Connection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Connection._CreateCommand(Query, Arguments)) {
				// Execute a query.
				return await Command.ExecuteNonQueryAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<List<T>> AllAsync<T>(this DbConnection Connection, string Query, params object[] Arguments) where T : class, new() {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Connection._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result set.
				return await Command.AllAsync<T>();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<List<dynamic>> AllAsync(this DbConnection Connection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Connection._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result set.
				return await Command.AllAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the scalar.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<T> ScalarAsync<T>(this DbConnection Connection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Connection._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return (T) await Command.ExecuteScalarAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the scalar.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<object> ScalarAsync(this DbConnection Connection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Connection._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.ExecuteScalarAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<T> SingleAsync<T>(this DbConnection Connection, string Query, params object[] Arguments) where T : class, new() {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Connection._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.SingleAsync<T>();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<dynamic> SingleAsync(this DbConnection Connection, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Connection._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.SingleAsync();
			}
		}
	}
}