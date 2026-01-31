using System.Collections.Generic;
using MPool.Runtime.RefPool;
using UnityEngine;

namespace MPool.Examples
{
    public class AdvancedObjectPoolTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private int acquireCount = 10;
        [SerializeField] private int expandCount = 20;
        [SerializeField] private int shrinkCount = 10;

        private readonly List<PoolableObjectForTest> objects = new();
        private int activeObjects;

        private void AcquireObjects()
        {
            Log($"获取 {acquireCount} 个 TestPoolableObject...");

            var startTime = Time.realtimeSinceStartup;

            for (var i = 0; i < acquireCount; i++)
            {
                var obj = RefPoolMgr.Acquire<PoolableObjectForTest>();
                obj.Initialize($"Object_{activeObjects++}");
                objects.Add(obj);
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            Log($"完成! 用时: {elapsed:F2}ms");
        }

        private void ReleaseObjects()
        {
            Log("释放所有 TestPoolableObject...");

            var startTime = Time.realtimeSinceStartup;

            for (var i = 0; i < Mathf.Min(10, activeObjects); i++)
            {
                if (objects.Count == 0)
                {
                    Log($"释放失败！还没有引用任何池中对象！");
                }

                var obj = objects[0];
                objects.RemoveAt(0);
                RefPoolMgr.Release(obj);
            }

            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            Log($"释放完成! 用时: {elapsed:F2}ms");
        }

        private void ExpandPool()
        {
            Log($"扩展 TestPoolableObject 池 {expandCount} 个对象...");

            var startTime = Time.realtimeSinceStartup;
            RefPoolMgr.Expand<PoolableObjectForTest>(expandCount);
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            Log($"扩展完成! 用时: {elapsed:F2}ms");
        }

        private void ShrinkPool()
        {
            Log($"收缩 TestPoolableObject 池 {shrinkCount} 个对象...");

            var startTime = Time.realtimeSinceStartup;
            RefPoolMgr.Shrink<PoolableObjectForTest>(shrinkCount);
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            Log($"收缩完成! 用时: {elapsed:F2}ms");
        }

        private void ClearPools()
        {
            Log("清空所有对象池...");

            var startTime = Time.realtimeSinceStartup;
            RefPoolMgr.Clear();
            var elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;

            activeObjects = 0;
            Log($"清空完成! 用时: {elapsed:F2}ms");
        }

        private static void Log(string message)
        {
            Debug.Log($"[RefPool Test] {message}");
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(610, 10, 400, 400));

            GUILayout.Label("RefPool 测试", GUI.skin.label);
            GUILayout.Space(10);

            var infos = RefPoolMgr.GetAllPoolInfos();
            if (infos != null && infos.Length > 0)
            {
                foreach (var info in infos)
                {
                    GUILayout.Label(
                        $"{info.PoolType.Name}: 空闲={info.UnusedPoolableCount}, 使用={info.UsedPoolableCount}");
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button($"获取 {acquireCount} 个对象")) AcquireObjects();
            if (GUILayout.Button("释放对象")) ReleaseObjects();
            if (GUILayout.Button($"扩展池 {expandCount}")) ExpandPool();
            if (GUILayout.Button($"收缩池 {shrinkCount}")) ShrinkPool();
            if (GUILayout.Button("清空所有池")) ClearPools();

            GUILayout.EndArea();
        }
    }
}