using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResourceLoader
{
    private class LoadedResource
    {
        public GameObject loadedGO;
        public int loadCount;

        public LoadedResource(GameObject loadedGO)
        {
            this.loadedGO = loadedGO;
            loadCount = 0;
        }
    }

    private static Dictionary<string, LoadedResource> dict_ResourceLoadInfo = new Dictionary<string, LoadedResource>();

    public static GameObject Load(string path)
    {
        if (!dict_ResourceLoadInfo.ContainsKey(path))
        {
            var rsc = Resources.Load<GameObject>(path);
            dict_ResourceLoadInfo.Add(path, new LoadedResource(rsc));
        }
        dict_ResourceLoadInfo[path].loadCount++;
        return dict_ResourceLoadInfo[path].loadedGO;
    }
    public static T Load<T>(string path) where T : MonoBehaviour
    {
        var loadedGO = Load(path);

        if (!loadedGO.TryGetComponent<T>(out T component))
            component = loadedGO.AddComponent<T>();

        return component;
    }

    public static void UnLoad(string path)
    {
        if (!dict_ResourceLoadInfo.ContainsKey(path)) return;

        dict_ResourceLoadInfo[path].loadCount--;
        if (dict_ResourceLoadInfo[path].loadCount <= 0)
        {
            var rsc = dict_ResourceLoadInfo[path].loadedGO;
            Resources.UnloadAsset(rsc);
            dict_ResourceLoadInfo.Remove(path);
        }
    }
}
