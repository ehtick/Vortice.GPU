// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics;

/// <summary>
/// Structure that describes the <see cref="GPUDevice"/>.
/// </summary>
public record struct GPUDeviceDescriptor : IEquatable<GPUDeviceDescriptor>
{
    public GPUDeviceDescriptor(ValidationMode validationMode, PowerPreference powerPreference)
        : this(GPUBackend.Count, validationMode, powerPreference)
    {
    }

    public GPUDeviceDescriptor(
        GPUBackend preferredBackend,
        ValidationMode validationMode,
        PowerPreference powerPreference = PowerPreference.HighPerformance)
    {
        PreferredBackend = preferredBackend;
        ValidationMode = validationMode;
        PowerPreference = powerPreference;
    }

    /// <summary>
    /// Gets or sets the preferred <see cref="GPUBackend"/> to use.
    /// </summary>
    public GPUBackend PreferredBackend { get; init; } = GPUBackend.Count;

    /// <summary>
    /// Gets or sets the <see cref="GPU.ValidationMode"/> to use.
    /// </summary>
    public ValidationMode ValidationMode { get; init; } = ValidationMode.Disabled;

    /// <summary>
    /// Gets or sets the <see cref="PowerPreference"/> to use.
    /// </summary>
    public PowerPreference PowerPreference { get; init; } = PowerPreference.HighPerformance;

}
