namespace MPool.Runtime.GOPool
{
    /// <summary>
    /// GameObject池化对象接口
    /// 实现此接口的MonoBehaviour组件会在池化生命周期中收到回调
    /// </summary>
    public interface IGameObjectPoolable
    {
        /// <summary>
        /// 从对象池中获取时调用
        /// </summary>
        void OnSpawnFromPool();

        /// <summary>
        /// 归还到对象池时调用
        /// </summary>
        void OnReturnToPool();
    }
}