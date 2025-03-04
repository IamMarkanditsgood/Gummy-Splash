namespace Mkey
{
    public enum AutoWin { Fast, Slow, Bombs }
    public enum MoveDir { Left, Right, Up, Down }
    public enum TouchState { None, Down, BeginSwap, EndSwap }
    public enum GameMode { Play, Edit }
    public enum MatchBoardState { ShowEstimate, Fill, Collect, Waiting, Iddle}
    public enum SpawnerStyle { AllEnabled, AllEnabledAlign, DisabledAligned }
    public enum FillType {Step, Fast}
    public enum BombDir { Vertical, Horizontal, Radial, Random, Color} 
    public enum BombType { StaticMatch, DynamicMatch, DynamicClick }
    public enum MatchGroupType {None, Simple, Hor4, Vert4, LT, BigLT, MiddleLT, Hor5, Vert5, Rect, ExtRect }
    public enum BombCombine { ColorBombAndColorBomb, RadBombAndRadBomb, HV, ColorBombAndRadBomb, BombAndHV, ColorBombAndHV, RandRocketAndHorBomb, RandRocketAndVertBomb, RandRocketAndRadBomb, RandRocketAndColorBomb, RandRocketAndRandRocket, None }
}
