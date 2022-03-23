// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Mathematics;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace Vortice.Graphics.Samples;

internal class UWPWindow : Window, IFrameworkView
{
    private readonly UWPPlatform _platform;
    private CoreWindow? _coreWindow;
    private bool _windowClosed;

    public UWPWindow(UWPPlatform platform, string title)
         : base(title)
    {
        _platform = platform;
    }

    private void OnApplicationViewActivated(CoreApplicationView sender, IActivatedEventArgs e)
    {
        CoreWindow.GetForCurrentThread().Activate();
        _platform.Activate();
    }

    void IFrameworkView.Initialize(CoreApplicationView applicationView)
    {
        //ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        applicationView.Activated += OnApplicationViewActivated;
    }

    void IFrameworkView.SetWindow(CoreWindow window)
    {
        _coreWindow = window;
        window.SizeChanged += OnCoreWindowSizeChanged;
        window.Closed += OnCoreWindowClosed;
        //UWPPlatform.ExtendViewIntoTitleBar(true);
    }

    private void OnCoreWindowSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs e)
    {
        OnSizeChanged();
    }

    private void OnCoreWindowClosed(CoreWindow sender, CoreWindowEventArgs args)
    {
        _windowClosed = true;
    }

    void IFrameworkView.Load(string entryPoint)
    {
    }

    void IFrameworkView.Run()
    {
        ApplicationView applicationView = ApplicationView.GetForCurrentView();
        applicationView.Title = "Alimer";

        //_platform.OnInit();

        while (!_windowClosed)
        {
            CoreWindow.GetForCurrentThread().Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

            //_platform.Tick();
        }

        //_platform.Destroy();
    }

    void IFrameworkView.Uninitialize()
    {
        // TODO: Fire post run
    }

    /// <inheritdoc />
    public override SizeI ClientSize
    {
        get
        {
            return new((int)_coreWindow!.Bounds.Width, (int)_coreWindow!.Bounds.Height);
        }
    }

    /// <inheritdoc />
    public override void Show()
    {
        // TODO
        //_coreWindow.Visible = true;
    }

    public override void Hide()
    {
        // TODO
        //_coreWindow.Visible = false;
    }

    protected override void SetTitle(string title)
    {
        ApplicationView.GetForCurrentView().Title = title;
    }
}
