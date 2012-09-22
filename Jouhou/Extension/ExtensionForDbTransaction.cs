using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents the class providing extensions for the DbTransaction class.
	/// </summary>
	public static class ExtensionForDbTransaction {
		/// <summary>
		/// Create and return a command associated with the current connection.
		/// </summary>
		/// <param name="DbTransaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static DbCommand CreateCommand(this DbTransaction DbTransaction, string Query, object[] Arguments) {
			// Create a command associated with the connection.
			DbCommand DbCommand = DbTransaction.Connection.CreateCommand();
			// Set the text command.
			DbCommand.CommandText = Query;
			// Set the transaction.
			DbCommand.Transaction = DbTransaction;
			// Add a number of arguments to the command.
			DbCommand.Add(Arguments);
			// Return the command.
			return DbCommand;
		}

		/// <summary>
		/// Execute a query and retrieve the number of affected rows.
		/// </summary>
		/// <param name="DbTransaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<int> ToAffected(this DbTransaction DbTransaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbTransaction.CreateCommand(Query, Arguments)) {
				// Execute a query.
				return await Command.ExecuteNonQueryAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the scalar.
		/// </summary>
		/// <param name="DbTransaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<T> ToScalar<T>(this DbTransaction DbTransaction, string Query, params object[] Arguments) {
			// Execute a query and retrieve the scalar.
			return (T) await DbTransaction.ToScalar(Query, Arguments);
		}

		/// <summary>
		/// Execute a query and retrieve the scalar.
		/// </summary>
		/// <param name="DbTransaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<object> ToScalar(this DbTransaction DbTransaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbTransaction.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.ExecuteScalarAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="DbTransaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<T> ToResult<T>(this DbTransaction DbTransaction, string Query, params object[] Arguments) where T : class, new() {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbTransaction.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.ToResult<T>();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="DbTransaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<dynamic> ToResult(this DbTransaction DbTransaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbTransaction.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.ToResult();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="DbTransaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<IList<T>> ToResultSet<T>(this DbTransaction DbTransaction, string Query, params object[] Arguments) where T : class, new() {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbTransaction.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result set.
				return await Command.ToResultSet<T>();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="DbTransaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<IList<dynamic>> ToResultSet(this DbTransaction DbTransaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = DbTransaction.CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result set.
				return await Command.ToResultSet();
			}
		}
	}
}