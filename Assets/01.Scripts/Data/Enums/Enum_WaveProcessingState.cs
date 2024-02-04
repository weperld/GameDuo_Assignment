namespace Enums
{
    public enum WaveSequence
    {
        NONE,   // 첫 웨이브 시작 전
        WAIT,   // 웨이브 정보 세팅 후 시작 대기 중
        START,  // 웨이브 시작(전투 시작)
        CLEAR,  // 웨이브 클리어
    }
}