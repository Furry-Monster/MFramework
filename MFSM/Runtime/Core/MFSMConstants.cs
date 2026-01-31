namespace MFSM.Runtime.Core
{
    /// <summary>MFSM 保留常量。</summary>
    public static class MFSMConstants
    {
        /// <summary>任意状态 Id。作为 fromStateId 时表示从任意当前状态可触发。不可作为真实状态 Id 注册。</summary>
        public const string AnyStateId = "*";
    }
}