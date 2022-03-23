// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Vortice.Graphics.Samples;

internal sealed class UWPPlatform : AppPlatform, IFrameworkViewSource
{
    private bool _isActive;
    private UWPWindow _mainWindow;

    public UWPPlatform(Application application)
        : base(application)
    {
        CoreApplication.Resuming += OnCoreApplicationResuming;
        CoreApplication.Suspending += OnCoreApplicationSuspending;

        _mainWindow = new UWPWindow(this, GetDefaultTitleName());
    }

    // <inheritdoc />
    public override Window MainWindow => _mainWindow;

    // <inheritdoc />
    public override bool IsActive => _isActive;

    IFrameworkView IFrameworkViewSource.CreateView() => _mainWindow;

    private void OnCoreApplicationResuming(object sender, object e)
    {
        OnResume();
    }

    private void OnCoreApplicationSuspending(object sender, SuspendingEventArgs e)
    {
        SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();

        //using (var device3 = game.GraphicsDevice.NativeDevice.QueryInterface<SharpDX.DXGI.Device3>())
        //{
        //    game.GraphicsContext.CommandList.ClearState();
        //    device3.Trim();
        //}

        OnSuspend();

        deferral.Complete();
    }

    public static void ExtendViewIntoTitleBar(bool extendViewIntoTitleBar)
    {
        CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = extendViewIntoTitleBar;

        if (extendViewIntoTitleBar)
        {
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
    }

    // <inheritdoc />
    public override void Run()
    {
        CoreApplication.Run(this);
    }

    // <inheritdoc />
    public override void RequestExit()
    {
        //ExitRequested = true;
        //OnExiting();
        CoreApplication.Exit();
        //Application.Current.Exit();
    }

    public void Activate()
    {
        _isActive = true;
        OnActivated();
    }

    public void Suspend()
    {
        //OnSuspend();
    }

    // https://docs.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.applicationview.preferredlaunchwindowingmode?view=winrt-22000
    private void ToggleFullScreenModeButtonTest()
    {
        var view = ApplicationView.GetForCurrentView();
        if (view.IsFullScreenMode)
        {
            view.ExitFullScreenMode();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
            // The SizeChanged event will be raised when the exit from full-screen mode is complete.
        }
        else
        {
            if (view.TryEnterFullScreenMode())
            {
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
                // The SizeChanged event will be raised when the entry to full-screen mode is complete.
            }
        }
    }
}

internal partial class AppPlatform
{
    public static AppPlatform Create(Application application)
    {
        return new UWPPlatform(application);
    }
}
