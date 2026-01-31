using MPool.Runtime.RefPool;
using UnityEngine;

namespace MPool.Examples
{
    public class PoolableObjectForTest : IRefPoolable
    {
        public string Name { get; private set; }
        public float CreatedTime { get; private set; }

        public void Initialize(string name)
        {
            Name = name;
            CreatedTime = Time.realtimeSinceStartup;
        }

        public void OnAcquireFromPool()
        {
            Name ??= string.Empty;
        }

        public void OnReturnToPool()
        {
            Name = null;
            CreatedTime = 0f;
        }
    }
}