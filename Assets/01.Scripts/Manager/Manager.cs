using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object _lock = new object();

    private static T instance;
    public static T Instance
    {
        get
        {
            if (IsDetroying) return null;

            lock (_lock)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    T[] instanceObjs = FindObjectsOfType<T>();

                    // 매니저가 여러 개일 경우 한 개를 제외한 다른 모두를 제거
                    if (instanceObjs.Length > 1)
                    {
                        for (int i = instanceObjs.Length - 1; i >= 0; i--)
                        {
                            var inst = instanceObjs[i];
                            if (inst == instance) continue;

                            Destroy(inst.gameObject);
                        }

                        return instance;
                    }

                    // 매니저가 없을 경우 생성
                    if (instance == null)
                    {
                        GameObject managerObj = new GameObject($"<{typeof(T)}>");
                        instance = managerObj.AddComponent<T>();
                        DontDestroyOnLoad(managerObj);
                    }
                }

                return instance;
            }
        }
    }

    public static bool IsDetroying { get; private set; } = false;

    protected virtual void Awake()
    {
        IsDetroying = false;
    }

    protected virtual void OnDestroy()
    {
        IsDetroying = true;
    }

    protected virtual void OnApplicationQuit()
    {
        IsDetroying = true;
    }
}
