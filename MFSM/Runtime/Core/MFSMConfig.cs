namespace MFSM.Runtime.Core
{
    /// <summary>MFSM 配置。保留 Id 见 MFSMConstants.AnyStateId。</summary>
    public static class MFSMConfig
    {
        /// <summary>复合状态展开最大深度（防环）。</summary>
        public const int MaxCompoundExpansionDepth = 64;

        /// <summary>父链追溯最大步数（防环）。</summary>
        public const int MaxParentChainDepth = 64;
    }
}