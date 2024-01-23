namespace MediaHelpers.YouTubeLibrary.Video.General;
public abstract class YouTubeMainLoaderViewModel<V>(IExit exit, IPausePlayer player) : IVideoPlayerViewModel, ITelevisionLoaderViewModel where V : class
{
    public V? SelectedItem { get; protected set; }
    public Action? StateHasChanged { get; set; }
    public bool PlayButtonVisible { get; set; }
    public bool CloseButtonVisible { get; set; }
    //fullscreen is needed because that is how to implement the interface.
    //if there is a way it needs to do it, will be done.
    public bool FullScreen { get; set; }
    public string ProgressText { get; set; } = "00:00:00/00:00:00";
    public abstract Task SaveProgressAsync();
    public abstract Task VideoFinishedAsync();
    public Action? ComponentStop { get; set; }
    public Action? FinishInit { get; set; }
     //somebody needs to set this to allow the youtube player to play/stop.
    //anything else needed requires delegates as well.
    public int VideoPosition { get; set; } //this will need to be set somehow (?)
    public int VideoLength { get; set; } //this is needed so firstrun processes can do something with the information.
    public string VideoID { get; set; } = "";
    public int ResumeSecs { get; set; }
    public abstract bool CanPlay { get; }
    public async Task CloseScreenAsync()
    {
        await SaveProgressAsync();
        exit.ExitApp();
    }
    //this is going to be iffy
    public void PlayPause()
    {
        player.Pause();
    }
    public abstract Task InitAsync();
}