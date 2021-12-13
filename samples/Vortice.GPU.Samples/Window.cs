// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Drawing;

namespace Vortice.GPU.Samples;

public abstract class Window
{
    public abstract SizeF ClientSize { get; }

    public event EventHandler? SizeChanged;

    protected virtual void OnSizeChanged()
    {
        SizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Show();
    public abstract void Hide();
}
