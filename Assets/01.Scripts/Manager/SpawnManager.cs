using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : Manager<SpawnManager>
{
    private Dictionary<string, Queue<GameObject>> dict_Queues = new Dictionary<string, Queue<GameObject>>();

    #region 생성 함수
    private void GenerateNewObjInQueue<T>(string path, int count) where T : MonoBehaviour
    {
        var loadedPrefab = ResourceLoader.Load<T>(path);

        if (!dict_Queues.ContainsKey(path)) dict_Queues.Add(path, new Queue<GameObject>());
        var que = dict_Queues[path];

        for (int i = 0; i < count; i++)
        {
            var newObj = Instantiate(loadedPrefab);
            newObj.transform.SetParent(transform);
            newObj.gameObject.SetActive(false);
            que.Enqueue(newObj.gameObject);
        }
    }
    #endregion

    #region 스폰 함수
    public T Spawn<T>(string resourcePath, Transform parent = null) where T : MonoBehaviour
    {
        if (!dict_Queues.TryGetValue(resourcePath, out var v) || v.Count == 0)
            GenerateNewObjInQueue<T>(resourcePath, 2);

        var que = dict_Queues[resourcePath];
        var spawnObj = que.Dequeue();
        spawnObj.transform.parent = parent;
        spawnObj.gameObject.SetActive(true);
        spawnObj.transform.position = Vector3.zero;
        spawnObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        return spawnObj.GetComponent<T>();
    }
    public T[] SpawnMany<T>(int count, string resourcePath, Transform parent = null) where T : MonoBehaviour
    {
        count = Mathf.Max(count, 1);
        if (!dict_Queues.TryGetValue(resourcePath, out var v) || v.Count < count)
        {
            int needCount = v == null ? count : count - v.Count;
            GenerateNewObjInQueue<T>(resourcePath, needCount * 2);
        }

        var ret = new T[count];
        for (int i = 0; i < count; i++)
            ret[i] = Spawn<T>(resourcePath, parent);

        return ret;
    }
    #endregion

    #region 디스폰 함수
    public void Despawn(string originalRscPath, params GameObject[] objs)
    {
        if (objs == null || objs.Length == 0) return;

        if (!dict_Queues.TryGetValue(originalRscPath, out var v))
            dict_Queues.Add(originalRscPath, new Queue<GameObject>());
        var queue = dict_Queues[originalRscPath];

        foreach (var obj in objs)
        {
            if (obj == null) continue;

            obj.transform.SetParent(transform, false);
            obj.gameObject.SetActive(false);

            if (queue.Contains(obj)) continue;
            queue.Enqueue(obj);
        }
    }
    #endregion
}