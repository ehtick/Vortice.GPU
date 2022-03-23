// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Microsoft.Toolkit.Diagnostics;
using Vortice.Mathematics;

namespace Vortice.Graphics.Samples;

public abstract class Window
{
    protected static readonly SizeI DefaultWindowSize = new(1200,800);

    private string _title = string.Empty;

    protected Window(string title)
    {
        _title = title;
    }

    /// <summary>
    /// Gets andSets the title of system window.
    /// </summary>
    public string Title
    {
        get => _title;
        set
        {
            Guard.IsNotNull(value, nameof(value));

            if (_title != value)
            {
                _title = value;
                SetTitle(_title);
            }
        }
    }

    public abstract SizeI ClientSize { get; }

    public event EventHandler? SizeChanged;

    protected virtual void OnSizeChanged()
    {
        SizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Show();
    public abstract void Hide();

    protected abstract void SetTitle(string title);
}
