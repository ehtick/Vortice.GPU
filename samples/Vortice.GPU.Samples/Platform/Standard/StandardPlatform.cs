// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static SDL2.SDL;
using static SDL2.SDL.SDL_EventType;

namespace Vortice.GPU.Samples;

internal class StandardPlatform : AppPlatform
{
    private const int _eventsPerPeep = 64;
    private readonly SDL_Event[] _events = new SDL_Event[_eventsPerPeep];
    private bool _exiting = false;

    public StandardPlatform(Application application)
        : base(application)
    {
        // Init SDL2
        if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_GAMECONTROLLER) != 0)
        {
            throw new Exception($"Unable to initialize SDL: {SDL_GetError()}");
        }

        MainWindow = new SDL2Window();
    }

    // <inheritdoc />
    public override Window MainWindow { get; }

    // <inheritdoc />
    public override bool IsActive { get; }

    // <inheritdoc />
    public override void Run()
    {
        Application.InitBeforeRun();

        MainWindow.Show();

        while (!_exiting)
        {
            PollSDLEvents();
            Application.Tick();
        }

        SDL_Quit();
    }

    // <inheritdoc />
    public override void RequestExit()
    {
        SDL_Quit();
    }

    private void PollSDLEvents()
    {
        SDL_PumpEvents();
        int eventsRead;

        do
        {
            eventsRead = SDL_PeepEvents(_events, _eventsPerPeep, SDL_eventaction.SDL_GETEVENT, SDL_EventType.SDL_FIRSTEVENT, SDL_EventType.SDL_LASTEVENT);
            for (int i = 0; i < eventsRead; i++)
            {
                HandleSDLEvent(_events[i]);
            }
        } while (eventsRead == _eventsPerPeep);
    }

    private void HandleSDLEvent(SDL_Event e)
    {
        switch (e.type)
        {
            case SDL_QUIT:
            case SDL_APP_TERMINATING:
                _exiting = true;
                break;
        }
    }
}

internal partial class AppPlatform
{
    public static AppPlatform Create(Application application)
    {
        return new StandardPlatform(application);
    }
}
