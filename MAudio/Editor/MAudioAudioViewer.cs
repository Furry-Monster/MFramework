using UnityEditor;
using UnityEngine;

namespace MAudio.Editor
{
    /// <summary>
    /// MAudio 查看器：运行态下显示 Clip 数量与各类型池状态。
    /// </summary>
    public class MAudioAudioViewer : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("MFramework/MAudio Viewer")]
        public static void Open()
        {
            var w = GetWindow<MAudioAudioViewer>("MAudio Viewer");
            w.minSize = new Vector2(280, 180);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("进入 Play 模式后可查看 MAudio 状态。", MessageType.Info);
                return;
            }

            var handler = Runtime.AudioHandler.Instance;
            if (handler == null)
            {
                EditorGUILayout.HelpBox("场景中未找到 AudioHandler。", MessageType.Warning);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("Clips", EditorStyles.boldLabel);
            var names = handler.GetClipNames();
            EditorGUILayout.LabelField($"已加载: {names?.Count ?? 0}");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Pool", EditorStyles.boldLabel);
            var infos = handler.GetPoolInfo();
            if (infos != null && infos.Count > 0)
                foreach (var info in infos)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(info.Type.ToString(), GUILayout.Width(70));
                    EditorGUILayout.LabelField($"空闲={info.IdleCount} 使用={info.UsedCount} 总数={info.TotalCount}");
                    EditorGUILayout.EndHorizontal();
                }

            EditorGUILayout.EndScrollView();
        }
    }
}