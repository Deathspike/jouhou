using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents the class providing extensions for the DbCommand class.
	/// </summary>
	public static class ExtensionForDbCommand {
		/// <summary>
		/// Add an arguments to the command.
		/// </summary>
		/// <param name="DbCommand">The command.</param>
		/// <param name="Argument">The argument.</param>
		public static void Add(this DbCommand DbCommand, object Argument) {
			// Create a parameter.
			DbParameter DbParameter = DbCommand.CreateParameter();
			// Set the name of the parameter.
			DbParameter.ParameterName = "@" + DbCommand.Parameters.Count;
			// Check if the object is null.
			if (Argument == null) {
				// Set the value of the parameter.
				DbParameter.Value = DBNull.Value;
			} else {
				// Retrieve the argument type.
				Type Type = Argument.GetType();
				// Check the object type.
				if (Type == typeof(Guid)) {
					// Set the database type of the value.
					DbParameter.DbType = DbType.String;
					// Set the maximum size of the data in the column.
					DbParameter.Size = 4000;
					// Set the value of the parameter.
					DbParameter.Value = Argument.ToString();
				} else if (Type == typeof(ExpandoObject)) {
					// Set the value of the parameter.
					DbParameter.Value = ((IDictionary<string, object>) Argument).Values.FirstOrDefault();
				} else if (Type == typeof(string)) {
					// Set the maximum size of the data in the column.
					DbParameter.Size = ((string) Argument).Length > 4000 ? -1 : 4000;
					// Set the value of the parameter.
					DbParameter.Value = Argument;
				} else {
					// Set the value of the parameter.
					DbParameter.Value = Argument;
				}
			}
			// Add the parameter to the command.
			DbCommand.Parameters.Add(DbParameter);
		}

		/// <summary>
		/// Add a number of arguments to the command.
		/// </summary>
		/// <param name="DbCommand">The command.</param>
		/// <param name="Arguments">The arguments.</param>
		public static void Add(this DbCommand DbCommand, params object[] Arguments) {
			// Check if an argument is available.
			if (Arguments != null) {
				// Iterate through each argument.
				foreach (object Argument in Arguments) {
					// Add an arguments to the command.
					DbCommand.Add(Argument);
				}
			}
		}

		/// <summary>
		/// Execute a command and retrieve a result.
		/// </summary>
		/// <param name="DbCommand">The command.</param>
		public static async Task<T> ToResult<T>(this DbCommand DbCommand) where T : class, new() {
			// Execute the reader and wait for the result.
			using (DbDataReader DataReader = await DbCommand.ExecuteReaderAsync()) {
				// Retrieve a result.
				return await DataReader.ToResult<T>();
			}
		}

		/// <summary>
		/// Execute a command and retrieve a result.
		/// </summary>
		/// <param name="DbCommand">The command.</param>
		public static async Task<object> ToResult(this DbCommand DbCommand) {
			// Execute the reader and wait for the result.
			using (DbDataReader DataReader = await DbCommand.ExecuteReaderAsync()) {
				// Retrieve a result.
				return await DataReader.ToResult();
			}
		}

		/// <summary>
		/// Execute a command and retrieve the result set.
		/// </summary>
		/// <param name="DbCommand">The command.</param>
		public static async Task<IList<T>> ToResultSet<T>(this DbCommand DbCommand) where T : class, new() {
			// Execute the reader and wait for the result.
			using (DbDataReader DataReader = await DbCommand.ExecuteReaderAsync()) {
				// Initialize a new instance of the List class.
				IList<T> Result = new List<T>();
				// Initialize the single variable.
				T Single;
				// Iterate while the result is valid.
				while ((Single = await DataReader.ToResult<T>()) != null) {
					// Add the result to the result set.
					Result.Add(Single);
				}
				// Return the result.
				return Result;
			}
		}

		/// <summary>
		/// Execute a command and retrieve the result set.
		/// </summary>
		/// <param name="DbCommand">The command.</param>
		public static async Task<IList<dynamic>> ToResultSet(this DbCommand DbCommand) {
			// Execute the reader and wait for the result.
			using (DbDataReader DataReader = await DbCommand.ExecuteReaderAsync()) {
				// Initialize a new instance of the List class.
				IList<dynamic> Result = new List<dynamic>();
				// Initialize the single variable.
				object Single;
				// Iterate while the result is valid.
				while ((Single = await DataReader.ToResult()) != null) {
					// Add the result to the result set.
					Result.Add(Single);
				}
				// Return the result.
				return Result;
			}
		}
	}
}