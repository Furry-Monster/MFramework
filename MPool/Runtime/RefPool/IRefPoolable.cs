namespace MPool.Runtime.RefPool
{
    /// <summary>
    /// 可被 RefPool 池化的引用类型接口
    /// <remarks>实现此接口的类型可由 RefPool/RefPoolMgr 管理，减少 GC</remarks>
    /// </summary>
    public interface IRefPoolable
    {
        /// <summary>
        /// 从池中取出时调用
        /// </summary>
        void OnAcquireFromPool();

        /// <summary>
        /// 归还到池时调用
        /// </summary>
        void OnReturnToPool();
    }
}