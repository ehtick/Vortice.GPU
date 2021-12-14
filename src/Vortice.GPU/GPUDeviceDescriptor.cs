// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.GPU;

/// <summary>
/// Structure that describes the <see cref="GPUDevice"/>.
/// </summary>
public readonly struct GPUDeviceDescriptor : IEquatable<GPUDeviceDescriptor>
{
    /// <summary>
    /// Gets or sets the preferred <see cref="GPUBackend"/> to use.
    /// </summary>
    public GPUBackend PreferredBackend { get; init; } = GPUBackend.Count;

    /// <summary>
    /// Gets or sets the <see cref="GPUPowerPreference"/> to use.
    /// </summary>
    public GPUPowerPreference PowerPreference { get; init; } = GPUPowerPreference.HighPerformance;

    /// <summary>
    /// Gets or sets the <see cref="GPU.ValidationMode"/> to use.
    /// </summary>
    public ValidationMode ValidationMode { get; init; } = ValidationMode.Disabled;

    /// <summary>
    /// Compares two <see cref="GPUDeviceDescriptor"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="GPUDeviceDescriptor"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="GPUDeviceDescriptor"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator ==(GPUDeviceDescriptor left, GPUDeviceDescriptor right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="GPUDeviceDescriptor"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="GPUDeviceDescriptor"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="GPUDeviceDescriptor"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    public static bool operator !=(GPUDeviceDescriptor left, GPUDeviceDescriptor right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GPUDeviceDescriptor other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(GPUDeviceDescriptor other)
    {
        return
            PreferredBackend == other.PreferredBackend &&
            PowerPreference == other.PowerPreference &&
            ValidationMode == other.ValidationMode;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        {
            hashCode.Add(PreferredBackend);
            hashCode.Add(PowerPreference);
            hashCode.Add(ValidationMode);
        }

        return hashCode.ToHashCode();
    }
}
