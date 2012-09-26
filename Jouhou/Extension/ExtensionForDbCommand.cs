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
		/// Add an argument to the command.
		/// </summary>
		/// <param name="Command">The command.</param>
		/// <param name="Argument">The argument.</param>
		public static void Add(this DbCommand Command, object Argument) {
			// Create a parameter.
			DbParameter Parameter = Command.CreateParameter();
			// Set the name of the parameter.
			Parameter.ParameterName = "@" + Command.Parameters.Count;
			// Check if the object is null.
			if (Argument == null) {
				// Set the value of the parameter.
				Parameter.Value = DBNull.Value;
			} else {
				// Retrieve the argument type.
				Type Type = Argument.GetType();
				// Check the object type.
				if (Type == typeof(Guid)) {
					// Set the database type of the value.
					Parameter.DbType = DbType.String;
					// Set the maximum size of the data in the column.
					Parameter.Size = 4000;
					// Set the value of the parameter.
					Parameter.Value = Argument.ToString();
				} else if (Type == typeof(ExpandoObject)) {
					// Set the value of the parameter.
					Parameter.Value = ((IDictionary<string, object>) Argument).Values.FirstOrDefault();
				} else if (Type == typeof(string)) {
					// Set the maximum size of the data in the column.
					Parameter.Size = ((string) Argument).Length > 4000 ? -1 : 4000;
					// Set the value of the parameter.
					Parameter.Value = Argument;
				} else {
					// Set the value of the parameter.
					Parameter.Value = Argument;
				}
			}
			// Add the parameter to the command.
			Command.Parameters.Add(Parameter);
		}

		/// <summary>
		/// Add a number of arguments to the command.
		/// </summary>
		/// <param name="Command">The command.</param>
		/// <param name="Arguments">The arguments.</param>
		public static void Add(this DbCommand Command, params object[] Arguments) {
			// Check if an argument is available.
			if (Arguments != null) {
				// Iterate through each argument.
				foreach (object Argument in Arguments) {
					// Add an arguments to the command.
					Command.Add(Argument);
				}
			}
		}

		/// <summary>
		/// Execute a command and retrieve the result set.
		/// </summary>
		/// <param name="Command">The command.</param>
		public static async Task<List<T>> AllAsync<T>(this DbCommand Command) where T : class, new() {
			// Execute the reader and wait for the result.
			using (DbDataReader DataReader = await Command.ExecuteReaderAsync()) {
				// Initialize a new instance of the List class.
				List<T> Result = new List<T>();
				// Read values from the data reader.
				while (await DataReader.ReadAsync()) {
					// Add the result to the result set.
					Result.Add(DataReader.Single<T>());
				}
				// Return the result.
				return Result;
			}
		}

		/// <summary>
		/// Execute a command and retrieve the result set.
		/// </summary>
		/// <param name="Command">The command.</param>
		public static async Task<List<dynamic>> AllAsync(this DbCommand Command) {
			// Execute the reader and wait for the result.
			using (DbDataReader DataReader = await Command.ExecuteReaderAsync()) {
				// Initialize a new instance of the List class.
				List<object> Result = new List<object>();
				// Read values from the data reader.
				while (await DataReader.ReadAsync()) {
					// Add the result to the result set.
					Result.Add(DataReader.Single());
				}
				// Return the result.
				return Result;
			}
		}

		/// <summary>
		/// Execute a command and retrieve a result.
		/// </summary>
		/// <param name="Command">The command.</param>
		public static async Task<T> SingleAsync<T>(this DbCommand Command) where T : class, new() {
			// Execute the reader and wait for the result.
			using (DbDataReader DataReader = await Command.ExecuteReaderAsync()) {
				// Retrieve a result.
				return await DataReader.ReadAsync() ? DataReader.Single<T>() : null;
			}
		}

		/// <summary>
		/// Execute a command and retrieve a result.
		/// </summary>
		/// <param name="Command">The command.</param>
		public static async Task<object> SingleAsync(this DbCommand Command) {
			// Execute the reader and wait for the result.
			using (DbDataReader DataReader = await Command.ExecuteReaderAsync()) {
				// Retrieve a result.
				return await DataReader.ReadAsync() ? DataReader.Single() : null;
			}
		}
	}
}