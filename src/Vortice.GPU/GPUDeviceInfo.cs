// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.GPU;

public readonly struct GPUDeviceInfo
{
    public GPUDeviceFeatures Features { get; init; }
    public GPUDeviceLimits Limits { get; init; }
}
