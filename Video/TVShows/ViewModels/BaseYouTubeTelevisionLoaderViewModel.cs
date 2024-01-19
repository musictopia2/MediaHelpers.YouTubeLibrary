﻿namespace MediaHelpers.YouTubeLibrary.Video.TVShows.ViewModels;
public abstract class BaseYouTubeTelevisionLoaderViewModel<E> : YouTubeMainLoaderViewModel<E>
    where E: class, IEpisodeTable
{
    private readonly IBasicTelevisionRemoteControlHostService _hostService;
    private readonly IBasicTelevisionLoaderLogic<E> _loadLogic;
    private readonly ITelevisionVideoLoader<E> _reload;
    private readonly TelevisionContainerClass<E> _containerClass;
    private readonly ISystemError _error;
    private readonly IToast _toast;
    public BasicList<SkipSceneClass> Skips { get; set; } = [];
    public BaseYouTubeTelevisionLoaderViewModel(IBasicTelevisionLoaderLogic<E> loadLogic,
        ITelevisionVideoLoader<E> reload,
        IPausePlayer player,
        //TelevisionHolidayViewModel holidayViewModel,
        TelevisionContainerClass<E> containerClass,
        IBasicTelevisionRemoteControlHostService hostService,
        //ITelevisionListLogic listLogic,
        ISystemError error,
        IToast toast,
        IExit exit
        ) : base(exit, player)
    {
        _loadLogic = loadLogic;
        _reload = reload;
        _containerClass = containerClass;
        _hostService = hostService;
        _error = error;
        _toast = toast;
        if (ee1.EpisodeChosen.HasValue == false)
        {
            throw new CustomBasicException("No episode was chosen");
        }
        containerClass.EpisodeChosen = _loadLogic.GetChosenEpisode();
        if (containerClass.EpisodeChosen is null)
        {
            throw new CustomBasicException("There was no episode chosen.  Rethink");
        }
        _hostService.NewClient = SendOtherDataAsync;
        _hostService.SkipEpisodeForever = SkipEpisodeForeverAsync;
        _hostService.ModifyHoliday = ModifyHolidayAsync;
        _hostService.EditLater = EditEpisodeLaterAsync;
        SelectedItem = containerClass.EpisodeChosen;
    }
    protected static bool DidManuallyChooseHolidayEpisode(IEpisodeTable episode)
    {
        return episode.Holiday != EnumTelevisionHoliday.None;
    }
    protected abstract Task FinishModifyingHoliday(IEpisodeTable tempItem, EnumTelevisionHoliday holiday);
    private async Task ModifyHolidayAsync(EnumTelevisionHoliday holiday)
    {
        if (holiday == SelectedItem!.Holiday)
        {
            _toast.ShowInfoToast("No holiday change");
            return;
        }
        EnumTelevisionHoliday previous = SelectedItem.Holiday!.Value;
        var tempItem = StopEpisode();
        await _loadLogic.ModifyHolidayAsync(tempItem, holiday);
        //await StartNextEpisodeAsync(tempItem, previous);
        await FinishModifyingHoliday(tempItem, previous);
    }
    protected abstract Task StartNextEpisodeAsync(IEpisodeTable tempItem, EnumTelevisionHoliday holiday);
    protected abstract Task FinishEditEpisodeLaterAsync(IEpisodeTable tempItem, EnumTelevisionHoliday holiday);
    private async Task EditEpisodeLaterAsync()
    {
        var tempItem = StopEpisode();
        await _loadLogic.EditEpisodeLaterAsync(tempItem);
        await FinishEditEpisodeLaterAsync(tempItem, tempItem.Holiday!.Value);
    }
    protected abstract Task FinishSkippingEpisodeForeverAsync(IEpisodeTable tempItem, EnumTelevisionHoliday holiday);
    private async Task SkipEpisodeForeverAsync()
    {
        var tempItem = StopEpisode();
        await _loadLogic.ForeverSkipEpisodeAsync(tempItem);
        await FinishSkippingEpisodeForeverAsync(tempItem, tempItem.Holiday!.Value);
    }
    protected E StopEpisode()
    {
        ResumeSecs = 0;
        VideoPosition = 0;
        if (SelectedItem is null)
        {
            throw new CustomBasicException("No episode was even chosen");
        }
        var tempItem = SelectedItem;
        SelectedItem = null;
        if (tempItem is null)
        {
            throw new CustomBasicException("The temp item is null.  Wrong");
        }
        ComponentStop?.Invoke();
        return tempItem;
    }

    protected void LoadNewEpisode()
    {
        if (SelectedItem is null)
        {
            throw new CustomBasicException("No episode was even chosen");
        }
        _reload.ChoseEpisode(SelectedItem);
    }
    public override Task SaveProgressAsync()
    {
        return _loadLogic.UpdateTVShowProgressAsync(SelectedItem!, VideoPosition);
    }
    public override Task VideoFinishedAsync()
    {
        return _loadLogic.FinishTVEpisodeAsync(SelectedItem!);
    }
    private bool _hasIntro;
    private void BeforeInitEpisode()
    {
        if (_containerClass.EpisodeChosen is null)
        {
            throw new CustomBasicException("There was no episode chosen.  Rethink");
        }
        SelectedItem = _containerClass.EpisodeChosen;
        int secs = _loadLogic.GetSeconds(SelectedItem!);
        ResumeSecs = secs;
        VideoPosition = ResumeSecs;
        VideoID = SelectedItem!.FullPath();
        _hasIntro = SelectedItem.BeginAt > 0;
    }
    public override async Task InitAsync()
    {
        try
        {
            BeforeInitEpisode();
            await _loadLogic.InitializeEpisodeAsync(SelectedItem!);
            ProcessSkips();
            await _hostService.InitializeAsync(); //i think i forgot this.
            FinishInit?.Invoke();
        }
        catch (Exception ex)
        {
            _error.ShowSystemError(ex.Message);
        }
    }

    private (int startTime, int howLong) GetSkipData()
    {
        return (SelectedItem!.BeginAt, SelectedItem!.OpeningLength!.Value);
    }
    private void ProcessSkips()
    {
        if (_hasIntro)
        {
            var (StartTime, HowLong) = GetSkipData();
            SkipSceneClass skip = new()
            {
                StartTime = StartTime,
                HowLong = HowLong,
                EndTime = StartTime + HowLong //because you tube requires the end time.
            };
            Skips = [skip];
        }
    }
    protected Task SendOtherDataAsync()
    {
        int startAt;
        if (SelectedItem!.StartAt.HasValue == false)
        {
            startAt = 0;
        }
        else
        {
            startAt = SelectedItem.StartAt.Value; //i think.
        }
        return _hostService.SendProgressAsync(new TelevisionModel(SelectedItem.ShowTable.ShowName, ProgressText, SelectedItem.Holiday!, CanPlay == false, startAt, SelectedItem.CanEdit));
    }
    public async Task SendProgressAsync()
    {
        await SendOtherDataAsync(); //has to be this way so the ui can call the method to send.
    }
}