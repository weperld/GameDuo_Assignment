using UnityEngine;

public class Manager<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object _lock = new object();

    private static T instance;
    public static T Instance
    {
        get
        {
            if (IsDestroying) return null;

            lock (_lock)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    T[] instanceObjs = FindObjectsOfType<T>();

                    // �Ŵ����� ���� ���� ��� �� ���� ������ �ٸ� ��θ� ����
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

                    // �Ŵ����� ���� ��� ����
                    if (instance == null)
                    {
                        GameObject managerObj = new GameObject($"<{typeof(T)}>");
                        instance = managerObj.AddComponent<T>();
                    }

                    instance.transform.position = Vector3.zero;
                    instance.transform.rotation = Quaternion.identity;
                    DontDestroyOnLoad(instance);
                }

                return instance;
            }
        }
    }

    public static bool IsDestroying { get; private set; } = false;

    protected virtual void Awake()
    {
        IsDestroying = false;
    }

    protected virtual void OnDestroy()
    {
        IsDestroying = true;
    }

    protected virtual void OnApplicationQuit()
    {
        IsDestroying = true;
    }
}
