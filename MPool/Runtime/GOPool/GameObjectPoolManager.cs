using System;
using System.Collections.Generic;
using System.Linq;
using MUtils.Singleton;
using UnityEngine;

namespace MPool.Runtime.GOPool
{
    /// <summary>
    /// GameObject对象池管理器
    /// 管理多个不同类型的GameObject池
    /// </summary>
    public class GameObjectPoolManager : PersistentSingleton<GameObjectPoolManager>
    {
        private readonly Dictionary<GameObject, GameObjectPool> pools = new();
        private readonly Dictionary<GameObject, GameObjectPoolConfig> poolConfigs = new();

        // 事件
        public event Action<GameObject, GameObject> OnObjectSpawned;
        public event Action<GameObject, GameObject> OnObjectReturned;
        public event Action<GameObject> OnPoolCreated;
        public event Action<GameObject> OnPoolDestroyed;

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <returns>对象实例</returns>
        public GameObject Get(GameObject prefab, Vector3 position = default, Quaternion rotation = default)
        {
            if (prefab == null) return null;

            var pool = GetOrCreatePool(prefab);
            var obj = pool.Get(position, rotation);

            if (obj != null) OnObjectSpawned?.Invoke(prefab, obj);

            return obj;
        }

        /// <summary>
        /// 归还对象
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="obj">要归还的对象</param>
        /// <returns>是否成功归还</returns>
        public bool Return(GameObject prefab, GameObject obj)
        {
            if (prefab == null || obj == null) return false;

            if (!pools.TryGetValue(prefab, out var pool)) return false;

            var success = pool.Return(obj);
            if (success) OnObjectReturned?.Invoke(prefab, obj);

            return success;
        }

        /// <summary>
        /// 预热池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="count">预热数量</param>
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null) return;

            var pool = GetOrCreatePool(prefab);
            pool.Prewarm(count);
        }

        /// <summary>
        /// 设置池配置
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="config">配置</param>
        public void SetPoolConfig(GameObject prefab, GameObjectPoolConfig config)
        {
            if (prefab == null) return;

            poolConfigs[prefab] = config;
        }

        /// <summary>
        /// 销毁指定池
        /// </summary>
        /// <param name="prefab">预制体</param>
        public void DestroyPool(GameObject prefab)
        {
            if (prefab == null) return;

            if (pools.TryGetValue(prefab, out var pool))
            {
                pool.Clear();
                pools.Remove(prefab);
                poolConfigs.Remove(prefab);
                OnPoolDestroyed?.Invoke(prefab);
            }
        }

        /// <summary>
        /// 清空所有池
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values) pool.Clear();

            pools.Clear();
            poolConfigs.Clear();
        }

        /// <summary>
        /// 获取池信息
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <returns>池信息</returns>
        public GameObjectPoolInfo GetPoolInfo(GameObject prefab)
        {
            if (prefab == null || !pools.TryGetValue(prefab, out var pool)) return null;

            return new GameObjectPoolInfo
            {
                prefab = prefab,
                poolName = pool.PoolName,
                availableCount = pool.AvailableCount,
                inUseCount = pool.InUseCount,
                totalCreated = pool.TotalCreated,
                totalSpawned = pool.TotalSpawned,
                totalReturned = pool.TotalReturned
            };
        }

        /// <summary>
        /// 获取所有池信息
        /// </summary>
        /// <returns>所有池信息</returns>
        public GameObjectPoolInfo[] GetAllPoolInfos()
        {
            return pools.Keys.Select(GetPoolInfo).Where(info => info != null).ToArray();
        }

        private GameObjectPool GetOrCreatePool(GameObject prefab)
        {
            if (pools.TryGetValue(prefab, out var existingPool)) return existingPool;

            if (poolConfigs.TryGetValue(prefab, out var config)) config.ValidateConfig();

            var pool = new GameObjectPool(prefab, config);
            pools[prefab] = pool;

            OnPoolCreated?.Invoke(prefab);

            return pool;
        }

        protected override void OnDestroy()
        {
            ClearAllPools();
            base.OnDestroy();
        }
    }
}