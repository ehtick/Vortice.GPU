// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Mathematics;

namespace Vortice.Graphics.Samples;

public abstract class Window
{
    public abstract Size ClientSize { get; }

    public event EventHandler? SizeChanged;

    protected virtual void OnSizeChanged()
    {
        SizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Show();
    public abstract void Hide();
}
