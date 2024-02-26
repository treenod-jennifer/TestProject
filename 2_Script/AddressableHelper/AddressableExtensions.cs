using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;

namespace PokoAddressable
{
    public static class AddressableExtensions
    {
        /// <summary>
        /// Label을 이용하지 않고 어드레서블 로드
        /// </summary>
        public static void AddressableAssetLoad<T>(this GameObject obj, string key, UnityAction<T> onComplete, UnityAction onFailed = null)
            where T : Object
        {
            AddressableAssetLoader.instance.Load<T>(obj, key, onComplete, onFailed);
        }

        /// <summary>
        /// Label을 이용해 어드레서블 로드
        /// </summary>
        public static void AddressableAssetLoad<T>(this GameObject obj, string key, string label,
            UnityAction<T> onComplete, UnityAction onFailed = null) where T : Object
        {
            AddressableAssetLoader.instance.LoadWithLabel<T>(obj, key, label, onComplete, onFailed);
        }
        
        /// <summary>
        /// 언어 설정에 맞춰 어드레서블 로드
        /// </summary>
        public static void AddressableAssetLoadUseCountryLabel<T>(this GameObject obj, string key, UnityAction<T> onComplete, UnityAction onFailed = null) where T : Object
        {
            var label = LanguageUtility.SystemCountryCode;
            if (string.Equals(label, "kr"))
            {
                label = "jp";
            }
            AddressableAssetLoader.instance.LoadWithLabel<T>(obj, key, label, onComplete, onFailed);
        }

        /// <summary>
        /// 어드레서블 오브젝트를 이용해 로드
        /// </summary>
        public static void AddressableAssetLoad<T>(this GameObject obj, AssetReference assetReference, UnityAction<T> onComplete, UnityAction onFailed = null)
            where T : Object
        {
            AddressableAssetLoader.instance.Load<T>(obj, assetReference, onComplete, onFailed);
        }
        
        /// <summary>
        /// 계급 아이콘 어드레서블 로드
        /// </summary>
        public static void AddressableAssetLoadClass<T>(this GameObject obj, int classIndex, UnityAction<T> onComplete) where T : Object
        {
            var key = "local_class/c" + Mathf.Clamp(classIndex, 1, 252);
            AddressableAssetLoader.instance.Load<T>(obj, key, onComplete, null);
        }
    }
}
