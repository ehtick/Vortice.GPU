// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Mathematics;
using static SDL2.SDL;
using static SDL2.SDL.SDL_WindowFlags;

namespace Vortice.Graphics.Samples;

internal class SDL2Window : Window
{
    private readonly IntPtr _window;

    public SDL2Window(string title)
        : base(title)
    {
        SDL_WindowFlags flags = SDL_WINDOW_ALLOW_HIGHDPI | SDL_WINDOW_HIDDEN | SDL_WINDOW_RESIZABLE;

        _window = SDL_CreateWindow(title,
            SDL_WINDOWPOS_CENTERED,
            SDL_WINDOWPOS_CENTERED,
            DefaultWindowSize.Width, DefaultWindowSize.Height,
            flags);

        // Native handle
        var wmInfo = new SDL_SysWMinfo();
        SDL_GetWindowWMInfo(_window, ref wmInfo);

        // Window handle is selected per subsystem as defined at:
        // https://wiki.libsdl.org/SDL_SysWMinfo
        switch (wmInfo.subsystem)
        {
            case SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
                //Source = SwapChainSource.CreateWin32(
                //    wmInfo.info.win.hinstance,
                //    wmInfo.info.win.window
                //    );
                break;

            case SDL_SYSWM_TYPE.SDL_SYSWM_X11:
                //return wmInfo.info.x11.window;
                break;

            case SDL_SYSWM_TYPE.SDL_SYSWM_COCOA:
                //return wmInfo.info.cocoa.window;
                break;

            case SDL_SYSWM_TYPE.SDL_SYSWM_UIKIT:
                //return wmInfo.info.uikit.window;
                break;

            case SDL_SYSWM_TYPE.SDL_SYSWM_WAYLAND:
                //return wmInfo.info.wl.shell_surface;
                break;

            case SDL_SYSWM_TYPE.SDL_SYSWM_ANDROID:
                //return wmInfo.info.android.window;
                break;

            default:
                break;
        }

        SDL_ShowWindow(_window);
    }

    /// <inheritdoc />
    public override SizeI ClientSize
    {
        get
        {
            SDL_GetWindowSize(_window, out int width, out int height);
            return new(width, height);
        }
    }

    /// <inheritdoc />
    public override void Show()
    {
        SDL_ShowWindow(_window);
    }

    public override void Hide()
    {
        SDL_HideWindow(_window);
    }

    /// <inheritdoc />
    protected override void SetTitle(string title)
    {
        SDL_SetWindowTitle(_window, title);
    }
}
