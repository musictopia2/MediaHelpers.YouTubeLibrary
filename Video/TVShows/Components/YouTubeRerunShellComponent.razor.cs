namespace MediaHelpers.YouTubeLibrary.Video.TVShows.Components;
public partial class YouTubeRerunShellComponent<E>
    where E: class, IEpisodeTable
{
    private YouTubeFullScreenComponent? _player;
    [Inject]
    private RerunYouTubeTelevisionLoaderViewModel<E>? DataContext { get; set; }
    [Inject]
    private PauseService? PauseProcess { get; set; }
    [Inject]
    private IMediaForcePlay? ForcePlay { get; set; }

    private bool _finishedFirst = false;

    private void FinishInit()
    {
        _finishedFirst = true;
        StateHasChanged();
    }

    private string VideoID => DataContext!.SelectedItem!.FullPath();
    //private int ResumeAt => DataContext!.ResumeSecs; //try this
    private int ResumeAt => DataContext!.SelectedItem!.ResumeAt is null ? 0 : DataContext!.SelectedItem.ResumeAt.Value;
    private int EndAt => DataContext!.SelectedItem!.ClosingLength is null ? 0 : DataContext!.SelectedItem.ClosingLength.Value;

    private int _progresses = 0;
    private bool _manuallyStarted;
    private async Task ShowProgressAsync(ProgressModel progress)
    {
        DataContext!.ProgressText = progress.GetProgress();
        DataContext.VideoPosition = progress.UpTo; //try this.
        if (_manuallyStarted == false)
        {
            _manuallyStarted = true;
            await ForcePlay!.ForcePlayAsync();
            return;
        }
        await DataContext!.SendProgressAsync();
        _progresses++;
        if (_progresses >= 10)
        {
            await DataContext.SaveProgressAsync();
            _progresses = 0; //start over again for performance.
        }
        //StateHasChanged();
        //_progress = progress.GetProgress();
    }
    protected override void OnInitialized()
    {
        DataContext!.StateHasChanged = StateHasChanged;
        //not sure if for future if i decide to use the television bar or not.
        DataContext.FinishInit = FinishInit;
        PauseProcess!.ComponentPlay = async () =>
        {
            await _player!.PlayPauseAsync();
        };

        DataContext!.ComponentStop = async () =>
        {
            //if this means something else, can do.
            await _player!.PlayPauseAsync(); //try this.  hopefully this is all that is needed (since will completely reload anyways or close out)
        };
    }
}