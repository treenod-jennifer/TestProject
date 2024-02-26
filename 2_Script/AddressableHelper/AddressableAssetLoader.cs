using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace PokoAddressable
{
    public class AddressableAssetLoader :  MonoSingleton<AddressableAssetLoader>
    {
        /// <summary>
        /// key 값을 이용해 어드레서블 로드
        /// </summary>
        public void Load<T>(GameObject obj, string key, [CanBeNull] UnityAction<T> onCompleted, UnityAction onFailed)
            where T : Object
        {
            if (obj == null) return;

            var handle = Addressables.LoadAssetsAsync<T>(key,
                delegate(T o)
                {
                    if (obj != null)
                    {
                        onCompleted?.Invoke(o);
                    }
                });
            
            if (handle.OperationException != null)
                onFailed?.Invoke();
            
            var release = obj.GetComponent<AddressableAutoRelease>();
            if (release == null)
            {
                release = obj.AddComponent<AddressableAutoRelease>();
            }
            release.OnRelease.AddListener(() => { Addressables.Release(handle); });
        }

        /// <summary>
        /// key 값과 label을 이용해 어드레서블 로드
        /// </summary>
        public void LoadWithLabel<T>(GameObject obj, string key, string label, [CanBeNull] UnityAction<T> onCompleted, UnityAction onFailed)
            where T : Object
        {
            if (obj == null) return;
            
            var handle = Addressables.LoadAssetsAsync<T>((IEnumerable) new List<object> {key, label},
                delegate(T o)
                {
                    if (obj != null)
                    {
                        onCompleted?.Invoke(o);
                    }
                },
                Addressables.MergeMode.Intersection);

            if (handle.OperationException != null)
                onFailed?.Invoke();
            
            var release = obj.GetComponent<AddressableAutoRelease>();
            if (release == null)
            {
                release = obj.AddComponent<AddressableAutoRelease>();
            }
            release.OnRelease.AddListener(() => { Addressables.Release(handle); });
        }

        /// <summary>
        /// 어드레서블 오브젝트를 이용해 로드
        /// </summary>
        public void Load<T>(GameObject obj, AssetReference assetReference, [CanBeNull] UnityAction<T> onCompleted, UnityAction onFailed)
            where T : Object
        {
            if (obj == null) return;
            
            var handle = Addressables.LoadAssetsAsync<T>(assetReference,
                delegate(T o)
                {
                    if (obj != null)
                    {
                        onCompleted?.Invoke(o);
                    }
                });

            if (handle.OperationException != null)
                onFailed?.Invoke();
            
            var release = obj.GetComponent<AddressableAutoRelease>();
            if (release == null)
            {
                release = obj.AddComponent<AddressableAutoRelease>();
            }
            release.OnRelease.AddListener(() => { Addressables.Release(handle); });
        }
    }
}
