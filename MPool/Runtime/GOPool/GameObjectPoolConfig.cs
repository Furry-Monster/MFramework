using UnityEngine;

namespace MPool.Runtime.GOPool
{
    [CreateAssetMenu(fileName = "GameObjectPoolConfig", menuName = "MPool/GameObject Pool Config", order = 1)]
    public class GameObjectPoolConfig : ScriptableObject
    {
        [Header("预制体设置")] [Tooltip("要池化的预制体")] public GameObject prefab;

        [Header("容量设置")] [Tooltip("初始池大小")] public int initialSize = 10;

        [Tooltip("最大池大小，0表示无限制")] public int maxSize = 100;

        [Tooltip("是否自动扩展")] public bool autoExpand = true;

        [Header("生命周期设置")] [Tooltip("是否在场景切换时保持")]
        public bool dontDestroyOnLoad = true;

        [Tooltip("自定义父节点，为空则自动创建")] public Transform customParent;

        [Tooltip("池名称，为空则使用预制体名称")] public string poolName = "";

        public string GetPoolName()
        {
            if (!string.IsNullOrEmpty(poolName))
                return poolName;

            return prefab != null ? prefab.name : "Unknown";
        }

        [ContextMenu("验证配置")]
        public void ValidateConfig()
        {
            if (prefab == null)
            {
                Debug.LogError($"[GameObjectPoolConfig] {name}: 预制体不能为空！");
                return;
            }

            if (initialSize < 0)
            {
                Debug.LogWarning($"[GameObjectPoolConfig] {name}: 初始大小不能为负数，已设置为0");
                initialSize = 0;
            }

            if (maxSize > 0 && maxSize < initialSize)
            {
                Debug.LogWarning($"[GameObjectPoolConfig] {name}: 最大大小不能小于初始大小，已设置为初始大小");
                maxSize = initialSize;
            }

            if (string.IsNullOrEmpty(poolName)) poolName = prefab.name;
        }
    }
}