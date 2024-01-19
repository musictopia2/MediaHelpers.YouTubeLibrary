namespace MediaHelpers.YouTubeLibrary.Video.General;
public class PauseService : IPausePlayer
{
    public Action? ComponentPlay { get; set; }
    void IPausePlayer.Pause()
    {
        ComponentPlay?.Invoke();
    }
}