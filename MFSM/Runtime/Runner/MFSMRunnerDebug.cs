using UnityEngine;

namespace MFSM.Runtime.Runner
{
    /// <summary>游戏内显示 Runner 的当前状态与路径。将 IMFSMRunner 组件拖到 Runner 上。</summary>
    public class MFSMRunnerDebug : MonoBehaviour
    {
        [SerializeField] [Tooltip("实现 IMFSMRunner 的组件")]
        private Component runner;

        [SerializeField] private bool showInGame = true;
        [SerializeField] private int fontSize = 14;
        [SerializeField] private float x = 10f;
        [SerializeField] private float y = 10f;
        [SerializeField] private float lineHeight = 22f;
        [SerializeField] private int maxPathLength = 80;

        private IMFSMRunner _runner;

        private void Awake()
        {
            _runner = runner as IMFSMRunner;
        }

        private void OnValidate()
        {
            _runner = runner as IMFSMRunner;
        }

        private void OnGUI()
        {
            if (!showInGame || _runner == null)
                return;

            var oldFontSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = fontSize;

            var pathStr = _runner.GetCurrentPathString();
            if (pathStr.Length > maxPathLength && maxPathLength > 3)
                pathStr = pathStr.Substring(0, maxPathLength - 3) + "...";

            GUI.Label(new Rect(x, y, 400, lineHeight), $"MFSM: {_runner.CurrentStateId ?? "(none)"}");
            GUI.Label(new Rect(x, y + lineHeight, 600, lineHeight * 2), $"Path: {pathStr}");

            GUI.skin.label.fontSize = oldFontSize;
        }
    }
}