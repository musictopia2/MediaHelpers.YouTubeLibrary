namespace MediaHelpers.YouTubeLibrary.Video.TVShows.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection RegisterYouTubeTelevisionLoaderRerunProcesses<T, E>(this IServiceCollection services)
        where T: class, IMediaForcePlay
        where E : class, IEpisodeTable
    {
        services.RegisterTelevisionContainer<E>()
            .RegisterCoreHolidayTelevisionServices<E>()
            .RegisterNextReRunTelevisionLogic<E>()
            .AddSingleton<RerunYouTubeTelevisionLoaderViewModel<E>>()
            .AddSingleton<IVideoPlayerViewModel>(pp => pp.GetRequiredService<RerunYouTubeTelevisionLoaderViewModel<E>>())
            .AddSingleton<ITelevisionLoaderViewModel>(pp => pp.GetRequiredService<RerunYouTubeTelevisionLoaderViewModel<E>>())
            .RegisterRerunTelevisionLoaderLogic<E>()
            .RegisterYouTubeLoaderBaseProcesses<T, E>();
        return services;
    }
    private static IServiceCollection RegisterYouTubeLoaderBaseProcesses<T, E>(this IServiceCollection services)
        where T: class, IMediaForcePlay
    {
        services.AddSingleton<ISimpleVideoPlayer, YouTubeVideoPlayer>()
           .AddSingleton<PauseService>()
           .AddSingleton<IPausePlayer>(pp => pp.GetRequiredService<PauseService>())
           .AddSingleton<IMediaForcePlay, T>();
        return services;
    }
}