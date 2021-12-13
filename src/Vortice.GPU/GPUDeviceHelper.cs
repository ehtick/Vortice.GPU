// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Runtime.InteropServices;

namespace Vortice.GPU;

/// <summary>
/// A <see langword="class"/> with methods to inspect the available devices on the current machine.
/// </summary>
internal static partial class GPUDeviceHelper
{
    /// <summary>
    /// The <see cref="Lazy{T}"/> instance used to produce the default <see cref="GPUDevice"/> instance.
    /// </summary>
    public static readonly Lazy<GPUDevice> DefaultFactory = new(GetDefaultDevice);

    private static GPUDevice GetDefaultDevice()
    {
        if (PlatformInfo.IsWindows)
        {
#if GPU_D3D12_BACKEND
            if (D3D12.D3D12GPUDevice.IsSupported.Value)
                return new D3D12.D3D12GPUDevice();
#endif
        }

        throw new GPUException("Unsupported GPUDevice detected!");
    }

    public static bool IsBackendSupported(GPUBackend backend)
    {
        if (backend == GPUBackend.Count)
        {
            backend = GetPlatformBackend();
        }

        return true;
    }

    public static GPUBackend GetPlatformBackend()
    {
        if (PlatformInfo.IsWindows)
        {
#if GPU_D3D12_BACKEND
            if (D3D12.D3D12GPUDevice.IsSupported.Value)
                return GPUBackend.Direct3D12;
#endif
        }
        else if (PlatformInfo.IsAndroid || PlatformInfo.IsLinux)
        {
#if GPU_VULKAN_BACKEND
            if (Vulkan.VulkanGPUDevice.IsSupported.Value)
                return GPUBackend.Direct3D12;
#endif
        }
        else if (PlatformInfo.IsMacOS)
        {
            // TODO: Metal
        }

        return GPUBackend.Count;
    }
}
