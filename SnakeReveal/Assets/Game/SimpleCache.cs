using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public abstract class SimpleCache<TComponent> : MonoBehaviour where TComponent : Component
    {
        private const int InitialCapacity = 1000;

        [SerializeField] private TComponent _prefab;

        private readonly Stack<TComponent> _cache = new(InitialCapacity);

        /// <summary>
        ///     access to private prefab for edit mode instantiation,
        ///     should not be required or used at runtime!!!
        /// </summary>
        public TComponent Prefab => _prefab;

        public virtual TComponent Get()
        {
            TComponent result;

            if (_cache.Count == 0)
            {
                result = Instantiate();
            }
            else
            {
                result = GetCached();
                result.gameObject.SetActive(true);
            }

            return result;
        }

        public virtual void Return(TComponent component)
        {
            component.gameObject.SetActive(false);
            Transform componentTransform = component.transform;
            componentTransform.parent = transform;
            componentTransform.localPosition = new Vector3(0f, 0f, 0f);
            componentTransform.rotation = Quaternion.identity;
            componentTransform.localScale = Vector3.one;
            _cache.Push(component);
        }

        protected virtual TComponent GetCached()
        {
            TComponent result = _cache.Pop();
            result.gameObject.SetActive(true);
            return result;
        }

        protected virtual TComponent Instantiate() => Instantiate(_prefab);
    }
}