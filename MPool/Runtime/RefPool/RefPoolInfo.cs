using System;
using System.Runtime.InteropServices;

// ReSharper disable ConvertToAutoProperty

namespace MPool.Runtime.RefPool
{
    /// <summary>
    /// RefPool 的统计信息
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public readonly struct RefPoolInfo
    {
        private readonly Type poolType;
        private readonly int unusedPoolableCount;
        private readonly int usedPoolableCount;
        private readonly int acquirePoolableCount;
        private readonly int releasePoolableCount;
        private readonly int addPoolableCount;
        private readonly int removePoolableCount;

        public RefPoolInfo(Type poolType, int unusedPoolableCount, int usedPoolableCount, int acquirePoolableCount,
            int releasePoolableCount, int addPoolableCount, int removePoolableCount)
        {
            this.poolType = poolType;
            this.unusedPoolableCount = unusedPoolableCount;
            this.usedPoolableCount = usedPoolableCount;
            this.acquirePoolableCount = acquirePoolableCount;
            this.releasePoolableCount = releasePoolableCount;
            this.addPoolableCount = addPoolableCount;
            this.removePoolableCount = removePoolableCount;
        }

        public Type PoolType => poolType;
        public int UnusedPoolableCount => unusedPoolableCount;
        public int UsedPoolableCount => usedPoolableCount;
        public int AcquirePoolableCount => acquirePoolableCount;
        public int ReleasePoolableCount => releasePoolableCount;
        public int AddPoolableCount => addPoolableCount;
        public int RemovePoolableCount => removePoolableCount;
    }
}