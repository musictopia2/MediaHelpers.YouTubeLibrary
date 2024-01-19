namespace MediaHelpers.YouTubeLibrary.Video.TVShows.ViewModels;
public class FirstRunYouTubeTelevisionLoaderViewModel<E> : BaseYouTubeTelevisionLoaderViewModel<E>
    where E : class, IEpisodeTable
{
    private readonly IFirstRunTelevisionLoaderLogic<E> _loadLogic;
    private readonly IFirstRunTelevisionRemoteControlHostService _hostService;
    private readonly INextEpisodeLogic<E> _nextLogic;
    private bool _canStart;
    public FirstRunYouTubeTelevisionLoaderViewModel(IFirstRunTelevisionLoaderLogic<E> loadLogic,
        ITelevisionVideoLoader<E> reload,
        IPausePlayer player,
        TelevisionContainerClass<E> containerClass,
        IFirstRunTelevisionRemoteControlHostService hostService,
        INextEpisodeLogic<E> nextLogic,
        ISystemError error,
        IToast toast,
        IExit exit) : base(loadLogic, reload, player, containerClass, hostService, error, toast, exit)
    {
        _loadLogic = loadLogic;
        _hostService = hostService;
        _nextLogic = nextLogic;
        _hostService.Start = StartAsync;
        _hostService.IntroBegins = ShowIntroBeginsAsync;
        _hostService.EndEpisode = EndEpisodeAsync;
        _hostService.ThemeSongOver = ThemeSongOverAsync;
        _hostService.Rewind = Rewind;
    }
    public Func<Task>? Rewind { get; set; }
    public override bool CanPlay => _canStart;
    private Task StartAsync()
    {
        _canStart = true;
        return Task.CompletedTask;
    }
    private async Task ShowIntroBeginsAsync()
    {
        await _loadLogic.IntroBeginsAsync(SelectedItem!); //i think
    }
    private async Task EndEpisodeAsync()
    {
        //may decide later to not always stop it (depending on conditions).
        //this can't be youtube either.
        E episode = StopEpisode();
        await _loadLogic.EndTVEpisodeEarlyAsync(episode);
        //for now, no questions asked.  eventually figure out a better way to figure out whether to really end it or ignore (because done by mistake)
        //await _loadLogic.EndTVEpisodeEarlyAsync(SelectedItem!);
    }
    private async Task ThemeSongOverAsync()
    {
        await _loadLogic.ThemeSongOverAsync(SelectedItem!);
    }
    protected override async Task StartNextEpisodeAsync(IEpisodeTable tempItem, EnumTelevisionHoliday holiday)
    {
        IShowTable show = tempItem.ShowTable;
        SelectedItem = await _nextLogic.GetNextEpisodeAsync(show);
        LoadNewEpisode();
    }

    protected override async Task FinishSkippingEpisodeForeverAsync(IEpisodeTable tempItem, EnumTelevisionHoliday holiday)
    {
        await StartNextEpisodeAsync(tempItem, holiday); //this simple.
    }

    protected override async Task FinishEditEpisodeLaterAsync(IEpisodeTable tempItem, EnumTelevisionHoliday holiday)
    {
        await StartNextEpisodeAsync(tempItem, holiday);
    }

    protected override async Task FinishModifyingHoliday(IEpisodeTable tempItem, EnumTelevisionHoliday holiday)
    {
        await StartNextEpisodeAsync(tempItem, holiday); //for now until i figure out something else.throw new NotImplementedException();
    }
}