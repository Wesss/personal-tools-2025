namespace DomainUtils.Disposable
{
    /// <summary>
    /// Manages reference counts to a shared disposable object, where the underlying object
    /// is only truly disposed after all references to said object are disposed.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/39532476/shared-ownership-of-idisposable-objects-in-c-sharp
    /// 
    /// This is not a threadsafe solution; probably works fine enough for single threaded applications
    /// </remarks>
    public sealed class SharedDisposable<T> where T : IDisposable
    {
        private readonly T value;
        private readonly object refLock = new object();
        private int refCount;

        public SharedDisposable(T value)
        {
            this.value = value;
        }

        public Reference Acquire()
        {
            lock (refLock)
            {
                if (refCount < 0) throw new ObjectDisposedException(typeof(T).FullName);
                refCount++;
                return new Reference(this);
            }
        }


        /// <summary>
        /// Returns true if no references are held by this shared dispoable anymore and the object has been disposed.
        /// </summary>
        /// <remarks>
        /// This was added in addition to stack overflow solution.
        /// </remarks>
        public bool IsDisposed()
        {
            lock (refLock)
            {
                return refCount < 0;
            }
        }

        private void Release()
        {
            lock (refLock)
            {
                refCount--;
                if (refCount <= 0)
                {
                    value.Dispose();
                    refCount = -1;
                }
            }
        }

        public sealed class Reference : IDisposable
        {
            private readonly SharedDisposable<T> owner;
            private bool isDisposed;
            public T Value => owner.value;

            public Reference(SharedDisposable<T> owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                if (isDisposed) return;
                isDisposed = true;

                owner.Release();
            }
        }
    }
}