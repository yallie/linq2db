﻿using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace LinqToDB.Async
{
	/// <summary>
	/// Provides reflection-based <see cref="DbConnection"/> wrapper with async operations support.
	/// </summary>
	internal class ReflectedAsyncDbConnection : AsyncDbConnection
	{
		private readonly Func<IDbConnection, CancellationToken, Task>?                                           _openAsync;
		private readonly Func<IDbConnection, Task>?                                                              _closeAsync;
#if NATIVE_ASYNC
		private readonly Func<IDbConnection, CancellationToken, ValueTask<IAsyncDbTransaction>>?                 _beginTransactionAsync;
		private readonly Func<IDbConnection, IsolationLevel, CancellationToken, ValueTask<IAsyncDbTransaction>>? _beginTransactionIlAsync;
		private readonly Func<IDbConnection, ValueTask>?                                                         _disposeAsync;
#else
		private readonly Func<IDbConnection, CancellationToken, Task<IAsyncDbTransaction>>?                      _beginTransactionAsync;
		private readonly Func<IDbConnection, IsolationLevel, CancellationToken, Task<IAsyncDbTransaction>>?      _beginTransactionIlAsync;
		private readonly Func<IDbConnection, Task>?                                                              _disposeAsync;
#endif

		public ReflectedAsyncDbConnection(
			IDbConnection connection,
#if NATIVE_ASYNC
			Func<IDbConnection, CancellationToken, ValueTask<IAsyncDbTransaction>>?                 beginTransactionAsync,
			Func<IDbConnection, IsolationLevel, CancellationToken, ValueTask<IAsyncDbTransaction>>? beginTransactionIlAsync,
#else
			Func<IDbConnection, CancellationToken, Task<IAsyncDbTransaction>>?                      beginTransactionAsync,
			Func<IDbConnection, IsolationLevel, CancellationToken, Task<IAsyncDbTransaction>>?      beginTransactionIlAsync,
#endif
			Func<IDbConnection, CancellationToken, Task>?                                           openAsync,
			Func<IDbConnection, Task>?                                                              closeAsync,
#if NATIVE_ASYNC
			Func<IDbConnection, ValueTask>?                                                         disposeAsync)
#else
			Func<IDbConnection, Task>?                                                              disposeAsync)
#endif
			: base(connection)
		{
			_beginTransactionAsync   = beginTransactionAsync;
			_beginTransactionIlAsync = beginTransactionIlAsync;
			_openAsync               = openAsync;
			_closeAsync              = closeAsync;
			_disposeAsync            = disposeAsync;
		}

#if NATIVE_ASYNC
		public override ValueTask<IAsyncDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
#else
		public override Task<IAsyncDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
#endif
		{
			return _beginTransactionAsync?.Invoke(Connection, cancellationToken) ?? base.BeginTransactionAsync(cancellationToken);
		}

#if NATIVE_ASYNC
		public override ValueTask<IAsyncDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
#else
		public override Task<IAsyncDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
#endif
		{
			return _beginTransactionIlAsync?.Invoke(Connection, isolationLevel, cancellationToken) ?? base.BeginTransactionAsync(isolationLevel, cancellationToken);
		}

		public override Task OpenAsync(CancellationToken cancellationToken = default)
		{
			return _openAsync?.Invoke(Connection, cancellationToken) ?? base.OpenAsync(cancellationToken);
		}

		public override Task CloseAsync()
		{
			return _closeAsync?.Invoke(Connection) ?? base.CloseAsync();
		}

#if !NATIVE_ASYNC
		public override Task DisposeAsync()
#else
		public override ValueTask DisposeAsync()
#endif
		{
			return _disposeAsync?.Invoke(Connection) ?? base.DisposeAsync();
		}
	}
}
