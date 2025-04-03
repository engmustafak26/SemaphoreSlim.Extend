using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace System.Threading
{
    internal class ExecuteStore
    {
        internal ExecuteStore(System.Threading.SemaphoreSlim semaphoreSlimInstance, object identifierId, Delegate code)
        {
            this.SemaphoreSlimInstance = new System.Threading.SemaphoreSlim(semaphoreSlimInstance.CurrentCount, semaphoreSlimInstance.GetMaxCount());
            CodeList = new ConcurrentQueue<Delegate>(new Delegate[] { code });
        }

        private System.Threading.SemaphoreSlim SemaphoreSlimInstance { get; set; }
        private ConcurrentQueue<Delegate> CodeList { get; set; }

        internal ExecuteStore AddToCodeList(Delegate code)
        {
            CodeList.Enqueue(code);
            return this;
        }

        internal async Task<object?> ExecuteAsync<T>()
        {
            try
            {
                await SemaphoreSlimInstance.WaitAsync();
                CodeList.TryDequeue(out var registeredCode);
                return await (Task<T>)registeredCode.DynamicInvoke()!;
            }
            finally
            {
                SemaphoreSlimInstance.Release();
            }
        }

        internal async Task ExecuteAsync()
        {
            try
            {
                await SemaphoreSlimInstance.WaitAsync();
                CodeList.TryDequeue(out var registeredCode);
                await (Task)registeredCode.DynamicInvoke()!;
            }
            finally
            {
                SemaphoreSlimInstance.Release();
            }
        }

        internal bool IsCodeListEmpty()
        {
            return CodeList.Count == 0;

        }


    }
}
