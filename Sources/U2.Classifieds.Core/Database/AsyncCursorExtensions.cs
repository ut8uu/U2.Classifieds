using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2.Classifieds.Core;

/// <summary>
/// Borrowed from http://blog.i3arnon.com/2015/12/16/async-linq-to-objects-over-mongodb/
/// </summary>
public static class AsyncCursorExtensions
{
    sealed class AsyncCursorSourceEnumerable<T> : IAsyncEnumerable<T>
    {
        readonly IAsyncCursorSource<T> _source;

        public AsyncCursorSourceEnumerable(IAsyncCursorSource<T> source)
        {
            _source = source;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new AsyncCursorSourceEnumerator<T>(_source, cancellationToken);
        }
    }

    sealed class AsyncCursorSourceEnumerator<T> : IAsyncEnumerator<T>
    {
        readonly IAsyncCursorSource<T> _source;
        readonly CancellationToken _cancellationToken;
        IAsyncEnumerator<T> _enumerator;

        public AsyncCursorSourceEnumerator(IAsyncCursorSource<T> source, CancellationToken cancellationToken)
        {
            _source = source;
            _cancellationToken = cancellationToken;
        }

        public async ValueTask DisposeAsync()
        {
            if (_enumerator != null)
            {
                await _enumerator.DisposeAsync();
                _enumerator = null;
            }
        }

        public T Current => _enumerator.Current;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_enumerator == null)
            {
                var enumerable = await _source.ToCursorAsync(_cancellationToken);
                _enumerator = enumerable.ToAsyncEnumerable().GetAsyncEnumerator(_cancellationToken);
            }
            return await _enumerator.MoveNextAsync();
        }
    }

    sealed class AsyncCursorEnumerable<T> : IAsyncEnumerable<T>
    {
        readonly IAsyncCursor<T> _source;

        public AsyncCursorEnumerable(IAsyncCursor<T> source)
        {
            _source = source;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken)
        {
            return new AsyncCursorEnumerator<T>(_source, cancellationToken);
        }
    }

    sealed class AsyncCursorEnumerator<T> : IAsyncEnumerator<T>
    {
        readonly IAsyncCursor<T> _source;
        readonly CancellationToken _cancellationToken;
        IEnumerator<T> _batch;

        public AsyncCursorEnumerator(IAsyncCursor<T> source, CancellationToken cancellationToken)
        {
            _source = source;
            _cancellationToken = cancellationToken;
        }

        public ValueTask DisposeAsync()
        {
            _batch?.Dispose();
            _batch = null;
            return default;
        }

        public T Current => _batch.Current;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_batch != null && _batch.MoveNext())
            {
                return true;
            }

            if (await _source.MoveNextAsync(_cancellationToken))
            {
                _batch?.Dispose();
                _batch = _source.Current.GetEnumerator();
                return _batch.MoveNext();
            }

            return false;
        }
    }

    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IAsyncCursorSource<T> source)
    {
        return new AsyncCursorSourceEnumerable<T>(source);
    }

    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IAsyncCursor<T> source)
    {
        return new AsyncCursorEnumerable<T>(source);
    }
}
