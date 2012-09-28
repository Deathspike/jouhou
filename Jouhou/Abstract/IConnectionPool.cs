// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
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