using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace LLib
{
    public class TrackedPool<T> : IObjectPool<T> where T : class
    {
        private readonly ObjectPool<T> _pool;
        private readonly HashSet<T> _activeRegistry = new();
    
    
        public int CountInactive => _pool.CountInactive;
    
    
        public TrackedPool(
            System.Func<T> createFunc, 
            System.Action<T> actionOnGet = null, 
            System.Action<T> actionOnRelease = null, 
            System.Action<T> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = 10, 
            int maxSize = 10000)
        {
            _pool = new ObjectPool<T>(
                createFunc: createFunc,
                actionOnGet : obj => 
                {
                    if (_activeRegistry.Add(obj))
                    {
                        actionOnGet?.Invoke(obj);
                    }
                },
                actionOnRelease :obj => 
                {
                    if (_activeRegistry.Remove(obj))
                    {
                        actionOnRelease?.Invoke(obj);
                    }
                },
                actionOnDestroy : obj => 
                {
                    _activeRegistry.Remove(obj);
                    actionOnDestroy?.Invoke(obj);
                },
                collectionCheck, 
                defaultCapacity, 
                maxSize
            );
        }
    
    
        public T Get()
        {
            return _pool.Get();
        }

    
        public PooledObject<T> Get(out T v)
        {
            return _pool.Get(out v);
        }

    
        public void Release(T element)
        {
            _pool.Release(element);
        }

    
        public void ReleaseAll()
        {
            var activeList = new List<T>(_activeRegistry);
            foreach (var obj in activeList)
            {
                if (obj is Object unityObj && unityObj == null) 
                    continue; 

                _pool.Release(obj);
            }
        
            _activeRegistry.Clear();
        }

    
        public void Clear()
        {
            ReleaseAll();
            _pool.Clear();
        }
    }
}

