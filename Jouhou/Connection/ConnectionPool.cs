using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents a connection pool.
	/// </summary>
	public sealed class ConnectionPool : IConnectionPool {
		/// <summary>
		/// Contains the connection string.
		/// </summary>
		private readonly string _ConnectionString;

		/// <summary>
		/// Indicates whether the pool has been disposed.
		/// </summary>
		private bool _Disposed;

		/// <summary>
		/// Contains the factory.
		/// </summary>
		private readonly DbProviderFactory _Factory;

		/// <summary>
		/// Contains each connection.
		/// </summary>
		private readonly List<DbConnection> _Pool;

		/// <summary>
		/// Initialize a new instance of the ConnectionPool class.
		/// </summary>
		/// <param name="ConnectionString">The connection string.</param>
		/// <param name="ProviderName">The provider name.</param>
		public ConnectionPool(string ConnectionString, string ProviderName = "System.Data.SqlClient") {
			// Set the connection string.
			_ConnectionString = ConnectionString;
			// Set the factory.
			_Factory = DbProviderFactories.GetFactory(ProviderName);
			// Initialize a new instance of the List class.
			_Pool = new List<DbConnection>();
		}

		#region IDbConnectionPool
		/// <summary>
		/// Close a connection.
		/// </summary>
		/// <param name="Connection">The connection.</param>
		public void Close(DbConnection Connection) {
			// Check if the connection is open.
			if (Connection != null && Connection.State == ConnectionState.Open) {
				// Check if the pool has not been disposed.
				if (!_Disposed) {
					// Lock this object.
					lock (this) {
						// Check if the pool has not been disposed.
						if (!_Disposed) {
							// Add the connection to the pool.
							_Pool.Add(Connection);
							// Return from the method.
							return;
						}
					}
				}
				// Dispose of the connection.
				Connection.Dispose();
			}
		}

		/// <summary>
		/// Open a connection.
		/// </summary>
		public async Task<DbConnection> OpenAsync() {
			// Check if a connection is available.
			if (_Pool.Count != 0) {
				// Lock this object.
				lock (this) {
					// Check if a connection is available.
					if (_Pool.Count != 0) {
						// Retrieve the connection.
						DbConnection Connection = _Pool[0];
						// Remove the connection from the pool.
						_Pool.RemoveAt(0);
						// Return the connection.
						return Connection;
					}
				}
			}
			// Establish a connection.
			if (1 == 1) {
				// Create a connection.
				DbConnection Connection = _Factory.CreateConnection();
				// Set the string used to open the connection.
				Connection.ConnectionString = _ConnectionString;
				// Open the database connection.
				await Connection.OpenAsync();
				// Return the connection.
				return Connection;
			}
		}
		#endregion

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
						// Clear the pool.
						_Pool.Clear();
					}
				}
			}
		}
		#endregion
	}
}