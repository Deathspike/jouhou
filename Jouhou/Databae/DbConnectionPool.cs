using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents a connection pool.
	/// </summary>
	public class DbConnectionPool : IDisposable {
		/// <summary>
		/// Contains the connection string.
		/// </summary>
		private string _ConnectionString;

		/// <summary>
		/// Indicates whether the pool has been disposed.
		/// </summary>
		private bool _Disposed;

		/// <summary>
		/// Contains the factory.
		/// </summary>
		private DbProviderFactory _Factory;

		/// <summary>
		/// Contains each connection.
		/// </summary>
		private List<DbConnection> _Pool;

		/// <summary>
		/// Initialize a new instance of the DbConnectionPool class.
		/// </summary>
		/// <param name="ConnectionString">The connection string.</param>
		public DbConnectionPool(string ConnectionString)
			: this(ConnectionString, "System.Data.SqlClient") {
			return;
		}

		/// <summary>
		/// Initialize a new instance of the DbConnectionPool class.
		/// </summary>
		/// <param name="ConnectionString">The connection string.</param>
		/// <param name="ProviderName">The provider name.</param>
		public DbConnectionPool(string ConnectionString, string ProviderName) {
			// Set the connection string.
			_ConnectionString = ConnectionString;
			// Set the factory.
			_Factory = DbProviderFactories.GetFactory(ProviderName);
			// Initialize a new instance of the List class.
			_Pool = new List<DbConnection>();
		}

		/// <summary>
		/// Fetch a connection.
		/// </summary>
		public async Task<DbConnection> Fetch() {
			// Check if the pool has not been disposed.
			if (!_Disposed) {
				// Check if a connection is available.
				if (_Pool.Count != 0) {
					// Lock this object.
					lock (this) {
						// Check if a connection is available.
						if (_Pool.Count != 0) {
							// Retrieve the connection.
							DbConnection DbConnection = _Pool[0];
							// Remove the connection from the pool.
							_Pool.RemoveAt(0);
							// Return the connection.
							return DbConnection;
						}
					}
				}
				// Establish a connection.
				if (1 == 1) {
					// Create a connection.
					DbConnection DbConnection = _Factory.CreateConnection();
					// Set the string used to open the connection.
					DbConnection.ConnectionString = _ConnectionString;
					// Open the database connection.
					await DbConnection.OpenAsync();
					// Return the connection.
					return DbConnection;
				}
			} else {
				// Return null.
				return null;
			}
		}

		/// <summary>
		/// Recycle a connection.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		public void Recycle(DbConnection DbConnection) {
			// Check if the connection is open.
			if (DbConnection.State == ConnectionState.Open) {
				// Check if the pool has not been disposed.
				if (!_Disposed) {
					// Lock this object.
					lock (this) {
						// Check if the pool has not been disposed.
						if (!_Disposed) {
							// Add the connection to the pool.
							_Pool.Add(DbConnection);
							// Return from the method.
							return;
						}
					}
				}
				// Dispose of the connection.
				DbConnection.Dispose();
			}
		}

		#region IDisposable
		/// <summary>
		/// Dispose of the object.
		/// </summary>
		public void Dispose() {
			// Check if the pool has not been disposed.
			if (!_Disposed) {
				// Lock this object.
				lock (this) {
					// Check if the pool has not been disposed.
					if (!_Disposed) {
						// Set the value indicating the pool has been disposed.
						_Disposed = true;
						// Iterate through each connection.
						foreach (DbConnection DbConnection in _Pool) {
							// Dispose of the connection.
							DbConnection.Dispose();
						}
					}
				}
			}
		}
		#endregion
	}
}