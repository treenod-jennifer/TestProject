public interface IDisturb{
    void SetDisturbPang(int uniquePang = 0, BlockBombType bombType = BlockBombType.NONE, bool bombEffect = false);
    bool IsLinkable();
    bool IsDisturbMove();
	bool IsDisturbBomb();
    //없어지는 방해블럭인지.
    bool IsCanPang();
    //현재 보드의 해당 방향으로 방해블럭이 존재하는지(true : 존재함, false : 존재하지 않음).
    bool IsDisturbBoardDirection(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0);
    //현재 보드의 해당 방향으로 풀울타리, 돌울타리가 존재하는지(true : 존재함, false : 존재하지 않음).
    bool IsDisturbBoard(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0);
    //해당 보드의 방향으로 물 생성이 막혀있는지(true : 물 생성 못함, false : 물 생성 가능).
    bool IsDisturbMoveWater(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0);
    //현재 보드의 방향으로 스플레시가 막혀있는지(true : 스플레시 불가능, false : 스플레시 가능).
    bool IsDisturbSplashBoard(BlockDirection blockDirection = BlockDirection.NONE, int indexX = 0, int indexY = 0);
}
