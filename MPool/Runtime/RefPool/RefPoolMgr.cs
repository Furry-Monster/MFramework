using System;
using System.Collections.Generic;

namespace MPool.Runtime.RefPool
{
    /// <summary>
    /// 引用对象池管理器（RefPoolMgr），按类型管理多个 RefPool
    /// </summary>
    public static class RefPoolMgr
    {
        private static readonly Dictionary<Type, RefPool> RefPoolDict = new();

        /// <summary>
        /// 当前管理的 RefPool 数量
        /// </summary>
        public static int Count
        {
            get
            {
                lock (RefPoolDict)
                {
                    return RefPoolDict.Count;
                }
            }
        }

        /// <summary>
        /// 从池中获取一个对象
        /// </summary>
        public static T Acquire<T>() where T : class, IRefPoolable, new()
        {
            var pool = GetPool(typeof(T));
            return pool.Acquire<T>();
        }

        /// <summary>
        /// 从池中获取一个对象（按类型）
        /// </summary>
        public static IRefPoolable Acquire(Type poolType)
        {
            var pool = GetPool(poolType);
            return pool.Acquire();
        }

        /// <summary>
        /// 将对象归还到池
        /// </summary>
        public static void Release(IRefPoolable poolable)
        {
            if (poolable == null)
                throw new ArgumentNullException(nameof(poolable));

            var pool = GetPool(poolable.GetType());
            pool.Release(poolable);
        }

        /// <summary>
        /// 预创建指定数量的对象到池
        /// </summary>
        public static void Expand<T>(int count) where T : class, IRefPoolable, new()
        {
            Expand(typeof(T), count);
        }

        /// <summary>
        /// 预创建指定数量的对象到池
        /// </summary>
        public static void Expand(Type poolType, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            var pool = GetPool(poolType);
            pool.Expand(count);
        }

        /// <summary>
        /// 从池中移除指定数量的空闲对象
        /// </summary>
        public static void Shrink<T>(int count) where T : class, IRefPoolable, new()
        {
            Shrink(typeof(T), count);
        }

        /// <summary>
        /// 从池中移除指定数量的空闲对象
        /// </summary>
        public static void Shrink(Type poolType, int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero");

            var pool = GetPool(poolType);
            pool.Shrink(count);
        }

        private static RefPool GetPool(Type poolType)
        {
            if (poolType == null)
                throw new ArgumentNullException(nameof(poolType));

            RefPool refPool;
            lock (RefPoolDict)
            {
                if (!RefPoolDict.TryGetValue(poolType, out refPool))
                {
                    refPool = new RefPool(poolType);
                    RefPoolDict.Add(poolType, refPool);
                }
            }

            return refPool;
        }

        /// <summary>
        /// 清空所有 RefPool
        /// </summary>
        public static void Clear()
        {
            lock (RefPoolDict)
            {
                foreach (var pool in RefPoolDict.Values) pool.ReleaseAll();

                RefPoolDict.Clear();
            }
        }

        /// <summary>
        /// 获取所有 RefPool 的统计信息
        /// </summary>
        public static RefPoolInfo[] GetAllPoolInfos()
        {
            var index = 0;
            RefPoolInfo[] result;

            lock (RefPoolDict)
            {
                result = new RefPoolInfo[RefPoolDict.Count];
                foreach (var item in RefPoolDict)
                    result[index++] = new RefPoolInfo(
                        item.Key, item.Value.UnusedPoolableCount,
                        item.Value.UsedPoolableCount, item.Value.AcquirePoolableCount,
                        item.Value.ReleasePoolableCount, item.Value.AddPoolableCount,
                        item.Value.RemovePoolableCount);
            }

            return result;
        }
    }
}