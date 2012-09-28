using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Jouhou {
	/// <summary>
	/// Represents a connection pool.
	/// </summary>
	public interface IConnectionPool : IDisposable {
		/// <summary>
		/// Close a connection.
		/// </summary>
		/// <param name="DbConnection">The connection.</param>
		void Close(DbConnection DbConnection);

		/// <summary>
		/// Open a connection.
		/// </summary>
		Task<DbConnection> OpenAsync();
	}
}