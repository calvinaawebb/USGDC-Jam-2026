using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class LazyPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> items = new();

        public LazyPool(T prefab, int initialSize = 10, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            for (int i = 0; i < initialSize; i++)
            {
                items.Push(CreateNew());
            }
        }

        private T CreateNew()
        {
            T instance = Object.Instantiate(prefab, parent);
            instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
            instance.gameObject.SetActive(false);
            return instance;
        }

        public T Get()
        {
            if (items.Count == 0)
            {
                // Something about this seems fucky
                items.Push(CreateNew());
            }

            T item = items.Pop();
            item.gameObject.SetActive(true);
            item.gameObject.hideFlags = HideFlags.None;
            return item;
        }

        public void Return(T item)
        {
            item.gameObject.hideFlags = HideFlags.HideInHierarchy;
            item.gameObject.SetActive(false);
            items.Push(item);
        }
    }
}
