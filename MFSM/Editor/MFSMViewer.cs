using System.Linq;
using MFSM.Runtime;
using MFSM.Runtime.Runner;
using UnityEditor;
using UnityEngine;

namespace MFSM.Editor
{
    /// <summary>Play 模式下查看场景中所有 MFSM Runner 的状态与路径。</summary>
    public class MFSMViewer : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("MFramework/MFSM Viewer")]
        public static void Open()
        {
            var w = GetWindow<MFSMViewer>("MFSM Viewer");
            w.minSize = new Vector2(320, 120);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("进入 Play 模式后可查看 MFSM 状态。", MessageType.Info);
                return;
            }

            var runners = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            var count = runners.OfType<IMFSMRunner>().Count();

            if (count == 0)
            {
                EditorGUILayout.HelpBox("场景中未找到 IMFSMRunner 组件。", MessageType.Warning);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.LabelField($"Runner 数量: {count}", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            foreach (var mb in runners)
            {
                if (mb is not IMFSMRunner r)
                    continue;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.ObjectField(mb, typeof(MonoBehaviour), true);
                EditorGUILayout.LabelField("当前状态", r.CurrentStateId ?? "(none)");
                EditorGUILayout.LabelField("路径", r.GetCurrentPathString());
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}