using MFSM.Runtime.Runner;
using UnityEditor;
using UnityEngine;

namespace MFSM.Editor
{
    /// <summary>MFSMRunnerDebug Inspector</summary>
    [CustomEditor(typeof(MFSMRunnerDebug))]
    public class MFSMRunnerDebugDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var runnerProp = serializedObject.FindProperty("runner");
            if (runnerProp != null && runnerProp.objectReferenceValue == null)
                EditorGUILayout.HelpBox("将实现 IMFSMRunner 的组件拖到 Runner 上以在游戏内显示状态。", MessageType.None);
        }
    }
}