// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
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
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		private static DbCommand _CreateCommand(this DbTransaction Transaction, string Query, object[] Arguments) {
			// Create a command associated with the connection.
			DbCommand Command = Transaction.Connection.CreateCommand();
			// Set the text command.
			Command.CommandText = Query;
			// Set the transaction.
			Command.Transaction = Transaction;
			// Add a number of arguments to the command.
			Command.Add(Arguments);
			// Return the command.
			return Command;
		}

		/// <summary>
		/// Execute a query and retrieve the number of affected rows.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<int> AffectedAsync(this DbTransaction Transaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Transaction._CreateCommand(Query, Arguments)) {
				// Execute a query.
				return await Command.ExecuteNonQueryAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<List<T>> AllAsync<T>(this DbTransaction Transaction, string Query, params object[] Arguments) where T : class, new() {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Transaction._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result set.
				return await Command.AllAsync<T>();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result set.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<List<dynamic>> AllAsync(this DbTransaction Transaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Transaction._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result set.
				return await Command.AllAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the scalar.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<T> ScalarAsync<T>(this DbTransaction Transaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Transaction._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return (T) await Command.ExecuteScalarAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the scalar.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<object> ScalarAsync(this DbTransaction Transaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Transaction._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.ExecuteScalarAsync();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<T> SingleAsync<T>(this DbTransaction Transaction, string Query, params object[] Arguments) where T : class, new() {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Transaction._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.SingleAsync<T>();
			}
		}

		/// <summary>
		/// Execute a query and retrieve the result.
		/// </summary>
		/// <param name="Transaction">The transaction.</param>
		/// <param name="Query">The query.</param>
		/// <param name="Arguments">The arguments.</param>
		public static async Task<dynamic> SingleAsync(this DbTransaction Transaction, string Query, params object[] Arguments) {
			// Create and return a command associated with the current connection.
			using (DbCommand Command = Transaction._CreateCommand(Query, Arguments)) {
				// Execute a command and retrieve the result.
				return await Command.SingleAsync();
			}
		}
	}
}