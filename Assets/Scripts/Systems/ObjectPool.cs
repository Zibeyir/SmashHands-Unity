using UnityEngine;
using System.Collections.Generic;


public class ObjectPool<T> where T : Component
{
    readonly T prefab;
    readonly Transform parent;
    readonly Stack<T> pool = new Stack<T>();


    public ObjectPool(T prefab, int initial, Transform parent = null)
    {
        this.prefab = prefab; this.parent = parent;
        for (int i = 0; i < initial; i++)
        {
            var t = Object.Instantiate(prefab, parent);
            t.gameObject.SetActive(false);
            pool.Push(t);
        }
    }


    public T Get()
    {
        var t = pool.Count > 0 ? pool.Pop() : Object.Instantiate(prefab, parent);
        t.gameObject.SetActive(true);
        return t;
    }


    public void Release(T t)
    {
        t.gameObject.SetActive(false);
        pool.Push(t);
    }
}