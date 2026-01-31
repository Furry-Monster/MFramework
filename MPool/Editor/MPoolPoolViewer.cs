using UnityEditor;
using UnityEngine;

namespace MPool.Editor
{
    /// <summary>
    /// MPool 池查看器：在运行态下查看 RefPool / GOPool 统计
    /// </summary>
    public class MPoolPoolViewer : EditorWindow
    {
        private Vector2 _scroll;
        private bool _refresh;

        [MenuItem("MFramework/MPool Viewer")]
        public static void Open()
        {
            var w = GetWindow<MPoolPoolViewer>("MPool Pool Viewer");
            w.minSize = new Vector2(320, 200);
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("进入 Play 模式后可查看 RefPool / GOPool 实时统计。", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawRefPoolSection();
            DrawGOPoolSection();

            EditorGUILayout.EndScrollView();

            if (Event.current.type == EventType.Repaint && _refresh)
                Repaint();
        }

        private void DrawRefPoolSection()
        {
            var infos = Runtime.RefPool.RefPoolMgr.GetAllPoolInfos();
            if (infos == null || infos.Length == 0)
            {
                EditorGUILayout.LabelField("RefPool", "（暂无）");
                return;
            }

            EditorGUILayout.LabelField("RefPool", EditorStyles.boldLabel);
            foreach (var info in infos)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(info.PoolType?.Name ?? "?", GUILayout.Width(120));
                EditorGUILayout.LabelField(
                    $"空闲={info.UnusedPoolableCount} 使用={info.UsedPoolableCount} 获取={info.AcquirePoolableCount} 归还={info.ReleasePoolableCount}");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);
        }

        private void DrawGOPoolSection()
        {
            var goMgr = GameObjectPoolManagerInstance;
            if (goMgr == null)
            {
                EditorGUILayout.LabelField("GOPool", "（需场景中存在 GameObjectPoolManager）");
                return;
            }

            var infos = goMgr.GetAllPoolInfos();
            if (infos == null || infos.Length == 0)
            {
                EditorGUILayout.LabelField("GOPool", "（暂无）");
                return;
            }

            EditorGUILayout.LabelField("GOPool", EditorStyles.boldLabel);
            foreach (var info in infos)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(info.poolName ?? "?", GUILayout.Width(120));
                EditorGUILayout.LabelField(
                    $"可用={info.availableCount} 使用={info.inUseCount} 创建={info.totalCreated} 获取={info.totalSpawned} 归还={info.totalReturned}");
                EditorGUILayout.EndHorizontal();
            }
        }

        private static Runtime.GOPool.GameObjectPoolManager GameObjectPoolManagerInstance => !Application.isPlaying
            ? null
            : FindFirstObjectByType<Runtime.GOPool.GameObjectPoolManager>();
    }
}