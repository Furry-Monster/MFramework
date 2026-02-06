using UnityEngine;

// ReSharper disable StaticMemberInGenericType

namespace MUtils.Singleton
{
    public class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
    {
        private static T _instance;
        private static readonly object _locker = new();
        private static bool _quitting;

        public static T Instance
        {
            get
            {
                if (_quitting)
                {
                    Debug.LogWarning(
                        $"[PersistentSingleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (_locker)
                {
                    if (_instance is null)
                    {
                        _instance = FindFirstObjectByType<T>();

                        if (FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1)
                        {
                            Debug.LogError(
                                "[PersistentSingleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                            return _instance;
                        }

                        if (_instance is null)
                        {
                            var singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
                            singleton.name = "(persis_singleton)" + typeof(T).Name;

                            DontDestroyOnLoad(singleton);

#if UNITY_EDITOR
                            Debug.Log(
                                $"[PersistentSingleton] An instance of {typeof(T)} is needed in the scene, so '{singleton.name}' was created with DontDestroyOnLoad.");
#endif
                        }
                    }

                    return _instance;
                }
            }
        }

        protected PersistentSingleton()
        {
        }

        protected virtual void Awake()
        {
            if (_instance is null)
            {
                _instance = this as T;
            }
            else if (_instance != this as T)
            {
                Debug.LogError($"[PersistentSingleton] Multiple instances of {typeof(T)} found. Destroying this one.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
#if UNITY_EDITOR
                Debug.Log($"[PersistentSingleton] Destroying {typeof(T)} Singleton.");
#endif
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _quitting = true;
        }
    }
}