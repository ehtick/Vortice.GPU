// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics;

public readonly struct GPUAdapterInfo
{
    /// <summary>
    /// Gets the adapter description.
    /// </summary>
    public string AdapterName { get; init; }

    /// <summary>
    /// Gets the adapter type, <see cref="GPUAdapterType"/>.
    /// </summary>
    public GPUAdapterType AdapterType { get; init; }

    /// <summary>
    /// Gets the adapter vendor, <see cref="GPU.VendorId"/>.
    /// </summary>
    public VendorId Vendor { get; init; }

    /// <summary>
    ///  Gets the PCI ID of the hardware device (if available).
    /// </summary>
    public uint VendorId {get; init; }
}
