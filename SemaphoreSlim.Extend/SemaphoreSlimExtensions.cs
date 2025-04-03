using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Threading
{
    public static class SemaphoreSlimExtensions
    {


        private static ConcurrentDictionary<(SemaphoreSlim, object), ExecuteStore> ConcurrentExecuteStoreDictionary =
                                                               new ConcurrentDictionary<(SemaphoreSlim, object), ExecuteStore>();

        private static System.Threading.SemaphoreSlim ManageDictionaryDeletionSemaphoreSlim = new SemaphoreSlim(1, 1);
        public static async Task<T> WaitAsync<T>(this SemaphoreSlim semaphoreSlim, object identifierId, Func<Task<T>> codeAsync)
        {
            ExecuteStore store = await AddOrUpdateKeyWithValueAsync(semaphoreSlim, identifierId, codeAsync);
            var result = (T)await store.ExecuteAsync<T>()!;
            await CleanupAsync(semaphoreSlim, identifierId, store);
            return result;
        }



        public static async Task WaitAsync(this SemaphoreSlim semaphoreSlim, object identifierId, Func<Task> codeAsync)
        {
            ExecuteStore store = await AddOrUpdateKeyWithValueAsync(semaphoreSlim, identifierId, codeAsync);

            await store.ExecuteAsync()!;
            await CleanupAsync(semaphoreSlim, identifierId, store);

        }


        internal static int GetMaxCount(this SemaphoreSlim semaphoreSlim)
        {
            FieldInfo field = semaphoreSlim.GetType().GetField("m_maxCount", BindingFlags.NonPublic | BindingFlags.Instance)!;
            int maxCount = (int)field.GetValue(semaphoreSlim)!;
            return maxCount;
        }

        private static async Task<ExecuteStore> AddOrUpdateKeyWithValueAsync(SemaphoreSlim semaphoreSlim, object identifierId, Func<Task> codeAsync)
        {
            ExecuteStore store = null;
            try
            {
                await ManageDictionaryDeletionSemaphoreSlim.WaitAsync();
                store = ConcurrentExecuteStoreDictionary.AddOrUpdate((semaphoreSlim, identifierId),
                                                     new ExecuteStore(semaphoreSlim, identifierId, codeAsync),
                                                     (instance, currentStore) => currentStore.AddToCodeList(codeAsync));
            }
            finally
            {
                ManageDictionaryDeletionSemaphoreSlim.Release();
            }

            return store;
        }

        private static async Task CleanupAsync(SemaphoreSlim semaphoreSlim, object identifierId, ExecuteStore store)
        {
            try
            {
                await ManageDictionaryDeletionSemaphoreSlim.WaitAsync();
                if (store.IsCodeListEmpty())
                {
                    ConcurrentExecuteStoreDictionary.TryRemove((semaphoreSlim, identifierId), out _);
                }
            }
            finally
            {
                ManageDictionaryDeletionSemaphoreSlim.Release();
            }
        }
    }
}
