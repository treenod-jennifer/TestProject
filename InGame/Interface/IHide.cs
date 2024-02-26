
public interface IHide
{
    bool IsRainbowBomb { get; set; }
    
    bool IsHideDeco();
    
    void SetHideDecoPang(int uniquePang = 0, BlockColorType pangColorType = BlockColorType.NONE);
}
