using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MPool.Runtime.GOPool
{
    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public class GameObjectPoolInfo
    {
        public GameObject prefab;
        public string poolName;
        public int availableCount;
        public int inUseCount;
        public int totalCreated;
        public int totalSpawned;
        public int totalReturned;
    }
}