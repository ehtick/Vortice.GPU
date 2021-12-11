// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Diagnostics;

namespace Vortice.GPU;

public abstract class Resource : IDisposable
{
    protected Resource(Device device)
    {
        Guard.IsNotNull(device, nameof(device));

        Device = device;
    }

    /// <summary>
    /// Get the <see cref="GPU.Device"/> object that created the resource.
    /// </summary>
    public Device Device { get; }

    /// <summary>
    /// Finalizes an instance of the <see cref="Resource" /> class.
    /// </summary>
    ~Resource() => Dispose(disposing: false);

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
}
