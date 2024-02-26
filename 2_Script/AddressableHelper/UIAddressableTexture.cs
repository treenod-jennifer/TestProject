using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// [사용 방법]
/// 해당 컴포넌트를 프리팹내에 추가한 뒤, 로드할 텍스쳐와 텍스쳐를 적용할 오브젝트를 drag&drop으로 설정합니다.
/// [사용처]
/// 어드레서블을 이용할 때, 스크립트에서 매번 로딩 코드를 사용하지 않아도 되도록 하기 위해 제작된 코드입니다.
/// 어드레서블 텍스쳐를 로딩한 뒤, 특별한 콜백처리를 해주지 않아도 되는 부분에서 사용합니다.
/// </summary>
public class UIAddressableTexture : MonoBehaviour
{
    [SerializeField] private AssetReferenceTexture refTexture;  //로딩할 어드레서블 텍스쳐
    [SerializeField] private UITexture baseTexture; //로딩해온 텍스쳐를 적용할 위치
    [SerializeField] private bool useMakePixelPerfect = false; //로딩 이후, 텍스쳐를 본 사이즈로 변경할지에 대한 유무

    private void LoadAddressable()
    {
        refTexture.LoadAssetAsync().Completed += OnLoadTexture;
    }
    
    public void OnLoadTexture(AsyncOperationHandle<Texture> asyncOperationHandle)
    {
        baseTexture.mainTexture = refTexture.Asset as Texture;

        if (useMakePixelPerfect == true)
            baseTexture.MakePixelPerfect();
    }

    private void Start()
    {
       LoadAddressable();
    }

    private void OnDestroy()
    {
        refTexture.ReleaseAsset();
    }
}
