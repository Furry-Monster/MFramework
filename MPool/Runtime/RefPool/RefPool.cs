using System;
using System.Collections.Generic;

namespace MPool.Runtime.RefPool
{
    /// <summary>
    /// 单一类型的引用对象池（RefPool）
    /// </summary>
    public class RefPool
    {
        private readonly Queue<IRefPoolable> poolables;
        private readonly Type poolType;
        private int usedPoolableCount;
        private int acquirePoolableCount;
        private int releasePoolableCount;
        private int addPoolableCount;
        private int removePoolableCount;

        /// <summary>
        /// 初始化指定类型的引用对象池
        /// </summary>
        /// <param name="poolType">池化对象类型，须实现 IRefPoolable</param>
        public RefPool(Type poolType)
        {
            if (!typeof(IRefPoolable).IsAssignableFrom(poolType))
                throw new ArgumentException($"{poolType} is not an IRefPoolable");

            poolables = new Queue<IRefPoolable>();
            this.poolType = poolType;
            usedPoolableCount = 0;
            acquirePoolableCount = 0;
            releasePoolableCount = 0;
            addPoolableCount = 0;
            removePoolableCount = 0;
        }

        /// <summary>池化对象类型</summary>
        public Type PoolType => poolType;

        /// <summary>当前池中空闲对象数量</summary>
        public int UnusedPoolableCount => poolables.Count;

        /// <summary>当前正在使用的对象数量</summary>
        public int UsedPoolableCount => usedPoolableCount;

        /// <summary>累计获取对象次数</summary>
        public int AcquirePoolableCount => acquirePoolableCount;

        /// <summary>累计归还对象次数</summary>
        public int ReleasePoolableCount => releasePoolableCount;

        /// <summary>累计创建新对象次数</summary>
        public int AddPoolableCount => addPoolableCount;

        /// <summary>累计销毁对象次数</summary>
        public int RemovePoolableCount => removePoolableCount;

        /// <summary>
        /// 从池中获取一个指定类型的对象
        /// </summary>
        public T Acquire<T>() where T : class, IRefPoolable, new()
        {
            if (typeof(T) != poolType)
                throw new ArgumentException($"Type {typeof(T)} does not match the {poolType} type");

            usedPoolableCount++;
            acquirePoolableCount++;

            lock (poolables)
            {
                if (poolables.Count > 0)
                {
                    var poolable = (T)poolables.Dequeue();
                    poolable.OnAcquireFromPool();
                    return poolable;
                }
            }

            addPoolableCount++;
            return new T();
        }

        /// <summary>
        /// 从池中获取一个对象（按类型）
        /// </summary>
        public IRefPoolable Acquire()
        {
            usedPoolableCount++;
            acquirePoolableCount++;

            lock (poolables)
            {
                if (poolables.Count > 0)
                {
                    var poolable = poolables.Dequeue();
                    poolable.OnAcquireFromPool();
                    return poolable;
                }
            }

            addPoolableCount++;
            return (IRefPoolable)Activator.CreateInstance(poolType);
        }

        /// <summary>
        /// 将对象归还到池
        /// </summary>
        public void Release<T>(T poolable) where T : class, IRefPoolable
        {
            if (poolable == null)
                throw new ArgumentNullException(nameof(poolable));

            if (typeof(T) != poolType)
                throw new ArgumentException($"Type {typeof(T)} does not match the {poolType} type");

            poolable.OnReturnToPool();

            lock (poolables)
            {
                if (poolables.Contains(poolable))
                    throw new InvalidOperationException("Object already released to pool");

                poolables.Enqueue(poolable);
            }

            releasePoolableCount++;
            usedPoolableCount--;
        }

        /// <summary>
        /// 将对象归还到池
        /// </summary>
        public void Release(IRefPoolable poolable)
        {
            if (poolable == null)
                throw new ArgumentNullException(nameof(poolable));

            if (poolable.GetType() != poolType)
                throw new ArgumentException($"Type {poolable.GetType()} does not match the {poolType} type");

            poolable.OnReturnToPool();

            lock (poolables)
            {
                if (poolables.Contains(poolable))
                    throw new InvalidOperationException("Object already released to pool");

                poolables.Enqueue(poolable);
            }

            releasePoolableCount++;
            usedPoolableCount--;
        }

        /// <summary>
        /// 清空池中所有空闲对象
        /// </summary>
        public void ReleaseAll()
        {
            lock (poolables)
            {
                var clearedCount = poolables.Count;
                poolables.Clear();
                removePoolableCount += clearedCount;
            }
        }

        /// <summary>
        /// 预创建指定数量的对象到池中
        /// </summary>
        public void Expand(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            lock (poolables)
            {
                for (var i = 0; i < count; i++)
                {
                    var instance = (IRefPoolable)Activator.CreateInstance(poolType);
                    poolables.Enqueue(instance);
                    addPoolableCount++;
                }
            }
        }

        /// <summary>
        /// 从池中移除指定数量的空闲对象
        /// </summary>
        public void Shrink(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            lock (poolables)
            {
                var actualRemoveCount = Math.Min(count, poolables.Count);
                for (var i = 0; i < actualRemoveCount; i++)
                {
                    poolables.Dequeue();
                    removePoolableCount++;
                }
            }
        }
    }
}