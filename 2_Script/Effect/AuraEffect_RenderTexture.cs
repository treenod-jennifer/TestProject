using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraEffect_RenderTexture : AuraEffect
{
    protected UITexture MirrorRoot { get; private set; }

    //protected override void Start()
    //{
    //    base.Start();

    //    whiteSpace = GetWhiteSpace(TargetTexture.height);

    //    MirrorRoot = MakeMirror();

    //    RenderTextureObjectMaker.Instance.Add(EffectRoot);

    //    MakeCamera();
    //}

    public override void Make(GameObject effect)
    {
        Make(effect, Color.white);
    }

    public void Make(GameObject effect, Color mirrorColor)
    {
        base.Make(effect);

        whiteSpace = GetWhiteSpace(TargetTexture.height);

        MirrorRoot = MakeMirror(mirrorColor);

        ObjectMaker.Instance.RenderTextureObjectAdd(EffectRoot);

        MakeCamera();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (MirrorRoot != null) Destroy(MirrorRoot.gameObject);
    }
    
    private UITexture MakeMirror(Color color)
    {
        if (TargetTexture == null) return null;

        var mirrorObj = new GameObject();

        var mirrorTexture = mirrorObj.AddComponent<UITexture>();
        mirrorTexture.pivot = TargetTexture.pivot;
        mirrorTexture.width = Mathf.RoundToInt(TargetTexture.width * whiteSpace);
        mirrorTexture.height = Mathf.RoundToInt(TargetTexture.height * whiteSpace);
        mirrorTexture.depth = TargetTexture.depth - 1;
        mirrorTexture.color = color;

        Vector2 pivot = GetPivot(TargetTexture.pivot) - (Vector2.one * 0.5f);
        pivot *= new Vector2(mirrorTexture.width - TargetTexture.width, mirrorTexture.height - TargetTexture.height);

        mirrorObj.transform.parent = transform;
        mirrorObj.transform.localPosition = pivot;
        mirrorObj.transform.localEulerAngles = Vector3.zero;
        mirrorObj.transform.localScale = Vector3.one;

        var renderTexture = new RenderTexture
        (
            TargetTexture.width,
            TargetTexture.height,
            TargetTexture.depth
        );
        renderTexture.format = RenderTextureFormat.ARGB32;
        mirrorTexture.mainTexture = renderTexture;

        return mirrorTexture;
    }

    private void MakeCamera()
    {
        if (EffectRoot == null) return;

        Texture2D texture2D = TargetTexture.mainTexture as Texture2D;
        if (texture2D == null) return;

        GameObject obj = new GameObject();
        obj.name = "EffectCamera";
        obj.transform.parent = EffectRoot.transform;

        Vector2 pivot = GetPivot(TargetTexture.pivot) - (Vector2.one * 0.5f);
        pivot *= new Vector2(texture2D.width * -1, texture2D.height * -1);

        obj.transform.localPosition = new Vector3(pivot.x, pivot.y, -1000);
        obj.transform.localScale = Vector3.one;

        Camera camera = obj.AddComponent<Camera>();
        camera.targetTexture = MirrorRoot.mainTexture as RenderTexture;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.cullingMask = LayerMask.GetMask("RenderTextureObject");
        camera.orthographic = true;

        float originSize = EffectRoot.transform.localScale.y * TargetTexture.height * 0.5f;
        float widgetRatio = (float)(TargetTexture.mainTexture as Texture2D).height / TargetTexture.height;

        camera.orthographicSize = originSize * widgetRatio * whiteSpace;
    }

    private float whiteSpace;
    private const int WHITE_SPACE = 100;
    private float GetWhiteSpace(int targetHeight)
    {
        return (float)(targetHeight + WHITE_SPACE) / targetHeight;
    }
}
