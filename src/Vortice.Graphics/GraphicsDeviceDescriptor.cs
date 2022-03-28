// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics;

/// <summary>
/// Structure that describes the <see cref="GraphicsDevice"/>.
/// </summary>
public record struct GraphicsDeviceDescriptor : IEquatable<GraphicsDeviceDescriptor>
{
    public GraphicsDeviceDescriptor(ValidationMode validationMode,
        PowerPreference powerPreference = PowerPreference.HighPerformance)
        : this(BackendType.Count, validationMode, powerPreference)
    {
    }

    public GraphicsDeviceDescriptor(
        BackendType preferredBackend,
        ValidationMode validationMode = ValidationMode.Disabled,
        PowerPreference powerPreference = PowerPreference.HighPerformance)
    {
        Name = default;
        PreferredBackend = preferredBackend;
        ValidationMode = validationMode;
        PowerPreference = powerPreference;
    }

    /// <summary>
    /// Gets or sets the preferred <see cref="BackendType"/> to use.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the preferred <see cref="BackendType"/> to use.
    /// </summary>
    public BackendType PreferredBackend { get; }

    /// <summary>
    /// Gets or sets the <see cref="Graphics.ValidationMode"/> to use.
    /// </summary>
    public ValidationMode ValidationMode { get; }

    /// <summary>
    /// Gets the <see cref="Graphics.PowerPreference"/> to use.
    /// </summary>
    public PowerPreference PowerPreference { get; }

}
