using System;
using System.Collections.Generic;
using UnityEngine;

namespace MPool.Runtime.GOPool
{
    /// <summary>
    /// 单一预制体的GameObject对象池
    /// </summary>
    public class GameObjectPool
    {
        private readonly GameObject prefab;
        private readonly Queue<GameObject> available;
        private readonly HashSet<GameObject> inUse;
        private readonly Transform parent;
        private readonly int maxSize;
        private readonly bool autoExpand;
        private readonly string poolName;

        // 统计信息
        private int totalCreated;
        private int totalSpawned;
        private int totalReturned;

        // 事件
        public event Action<GameObject> OnObjectSpawned;
        public event Action<GameObject> OnObjectReturned;
        public event Action<GameObject> OnObjectCreated;
        public event Action<GameObject> OnObjectDestroyed;

        public GameObjectPool(GameObject prefab, GameObjectPoolConfig config = null)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            this.prefab = prefab;
            available = new Queue<GameObject>();
            inUse = new HashSet<GameObject>();

            if (config != null)
            {
                maxSize = config.maxSize;
                autoExpand = config.autoExpand;
                poolName = config.GetPoolName();

                if (config.customParent != null)
                {
                    parent = config.customParent;
                }
                else
                {
                    var parentGO = new GameObject($"Pool_{poolName}");
                    parent = parentGO.transform;

                    if (config.dontDestroyOnLoad) UnityEngine.Object.DontDestroyOnLoad(parentGO);
                }
            }
            else
            {
                maxSize = 100;
                autoExpand = true;
                poolName = prefab.name;

                var parentGO = new GameObject($"Pool_{poolName}");
                parent = parentGO.transform;
            }
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        public GameObject Get(Vector3 position = default, Quaternion rotation = default)
        {
            GameObject obj = null;

            if (available.Count > 0)
                obj = available.Dequeue();
            else if (autoExpand && (maxSize <= 0 || totalCreated < maxSize))
                obj = CreateNewObject();
            else if (maxSize > 0 && totalCreated >= maxSize) return null;

            if (obj == null) return null;

            obj.transform.position = position;
            obj.transform.rotation = rotation;

            obj.SetActive(true);

            inUse.Add(obj);
            totalSpawned++;

            var poolable = obj.GetComponent<IGameObjectPoolable>();
            poolable?.OnSpawnFromPool();

            OnObjectSpawned?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// 归还对象
        /// </summary>
        public bool Return(GameObject obj)
        {
            if (obj == null) return false;

            if (!inUse.Contains(obj)) return false;

            var poolable = obj.GetComponent<IGameObjectPoolable>();
            poolable?.OnReturnToPool();

            obj.SetActive(false);

            inUse.Remove(obj);
            available.Enqueue(obj);
            totalReturned++;

            OnObjectReturned?.Invoke(obj);

            return true;
        }

        /// <summary>
        /// 预热池（预先创建指定数量的对象）
        /// </summary>
        public void Prewarm(int count)
        {
            if (count <= 0) return;

            var actualCount = maxSize > 0 ? Mathf.Min(count, maxSize) : count;

            for (var i = 0; i < actualCount; i++)
            {
                var obj = CreateNewObject();
                obj.SetActive(false);
                available.Enqueue(obj);
            }
        }

        /// <summary>
        /// 清空池
        /// </summary>
        public void Clear()
        {
            while (available.Count > 0)
            {
                var obj = available.Dequeue();
                if (obj != null)
                {
                    OnObjectDestroyed?.Invoke(obj);
                    UnityEngine.Object.Destroy(obj);
                }
            }

            foreach (var obj in inUse)
                if (obj != null)
                {
                    OnObjectDestroyed?.Invoke(obj);
                    UnityEngine.Object.Destroy(obj);
                }

            inUse.Clear();
            totalCreated = 0;
        }

        /// <summary>
        /// 收缩池（移除多余的可用对象）
        /// </summary>
        public void Shrink(int targetCount)
        {
            if (targetCount < 0) targetCount = 0;

            var toRemove = available.Count - targetCount;
            for (var i = 0; i < toRemove && available.Count > 0; i++)
            {
                var obj = available.Dequeue();
                if (obj != null)
                {
                    OnObjectDestroyed?.Invoke(obj);
                    UnityEngine.Object.Destroy(obj);
                    totalCreated--;
                }
            }
        }

        private GameObject CreateNewObject()
        {
            var obj = UnityEngine.Object.Instantiate(prefab, parent);
            obj.name = $"{prefab.name}_{totalCreated}";
            totalCreated++;

            OnObjectCreated?.Invoke(obj);

            return obj;
        }

        #region 属性访问器

        /// <summary>
        /// 预制体
        /// </summary>
        public GameObject Prefab => prefab;

        /// <summary>
        /// 池名称
        /// </summary>
        public string PoolName => poolName;

        /// <summary>
        /// 可用对象数量
        /// </summary>
        public int AvailableCount => available.Count;

        /// <summary>
        /// 使用中对象数量
        /// </summary>
        public int InUseCount => inUse.Count;

        /// <summary>
        /// 总创建数量
        /// </summary>
        public int TotalCreated => totalCreated;

        /// <summary>
        /// 总获取次数
        /// </summary>
        public int TotalSpawned => totalSpawned;

        /// <summary>
        /// 总归还次数
        /// </summary>
        public int TotalReturned => totalReturned;

        /// <summary>
        /// 父节点
        /// </summary>
        public Transform Parent => parent;

        #endregion
    }
}