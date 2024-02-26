using UnityEngine;

public class UIItemGallery : MonoBehaviour
{
    [SerializeField] private UIUrlTexture _texture;
    [SerializeField] private UISprite     _spriteBlack;
    private int _paintingId;

    private void OnDestroy() => _texture = null;

    public void UpdateData(int paintingId)
    {
        if (this._paintingId == paintingId)
        {
            return;
        }

        this._paintingId = paintingId;
        _texture.SettingTextureScale(_texture.width, _texture.height);
        _texture.LoadCDN(Global.gameImageDirectory, "Gallery/", $"painting_{paintingId}.png");
    }

    public void SetBlack(float alpha)
    {
        var color = _spriteBlack.color;
        color.a            = alpha;
        _spriteBlack.color = color;
    }
    
    public Texture GetTexture() => _texture.mainTexture;
}
