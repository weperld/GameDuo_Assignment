public class GameModes
{
    public enum Mode
    {
        CHALLENGE,
        REPEAT,
    }

    private Mode currentMode = Mode.CHALLENGE;
    public Mode _CurrentMode => currentMode;

    public GameModes(Mode startMode = Mode.CHALLENGE)
    {
        currentMode = startMode;
    }

    public void ChangeGameMode(Mode mode)
    {
        currentMode = mode;
    }
}
