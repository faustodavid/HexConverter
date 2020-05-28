using System;
using System.Buffers;

namespace HexConverter
{
    public readonly struct RentedArraySegmentWrapper<T> : IDisposable
    {
        public ArraySegment<T> ArraySegment { get; }
        private readonly ArrayPool<T> _arrayPool;

        public RentedArraySegmentWrapper(ArraySegment<T> arraySegment, ArrayPool<T> arrayPool)
        {
            ArraySegment = arraySegment;
            _arrayPool = arrayPool;
        }

        public void Dispose()
        {
            _arrayPool.Return(ArraySegment.Array);
        }
    }
}