namespace MFSM.Runtime.Core
{
    public static class MFSMConstants
    {
        /// <summary>任意状态 Id。作为 from 时表示任意当前状态可触发；不可作为真实状态 Id 注册。</summary>
        public const string AnyStateId = "*";
    }
}