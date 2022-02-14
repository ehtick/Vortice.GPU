// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Diagnostics;

namespace Vortice.Graphics;

public abstract class GraphicsResource : IDisposable
{
    protected string? _label;

    protected GraphicsResource(GraphicsDevice device)
    {
        Guard.IsNotNull(device, nameof(device));

        Device = device;
    }

    /// <summary>
    /// Get the <see cref="GraphicsDevice"/> object that created the resource.
    /// </summary>
    public GraphicsDevice Device { get; }

    /// <summary>
    /// Gets or set the label that identifies the resource.
    /// </summary>
    public string? Label
    {
        get => _label;
        set
        {
            _label = value;
            OnLabelChanged();
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="Resource" /> class.
    /// </summary>
    ~GraphicsResource() => Dispose(disposing: false);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose()" />
    /// <param name="disposing"><c>true</c> if the method was called from <see cref="Dispose()" />; otherwise, <c>false</c>.</param>
    protected virtual void Dispose(bool disposing)
    {
    }

    protected virtual void OnLabelChanged()
    {
    }
}
