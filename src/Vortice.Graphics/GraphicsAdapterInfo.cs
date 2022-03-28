// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics;

/// <summary>
/// Describes the <see cref="GraphicsDevice"/> physical device (adapter) information.
/// </summary>
public readonly struct GraphicsAdapterInfo
{
    /// <summary>
    /// Gets the adapter vendor, <see cref="Graphics.VendorId"/>.
    /// </summary>
    public VendorId VendorId { get; init; }

    /// <summary>
    /// Gets the device id.
    /// </summary>
    public uint DeviceId { get; init; }

    /// <summary>
    /// Gets the adapter name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the driver description.
    /// </summary>
    public string DriverDescription { get; init; }

    /// <summary>
    /// Gets the adapter type, <see cref="GraphicsAdapterType"/>.
    /// </summary>
    public GraphicsAdapterType AdapterType { get; init; }
}
