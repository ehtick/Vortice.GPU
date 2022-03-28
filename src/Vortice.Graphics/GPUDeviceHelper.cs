// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#if !EXCLUDE_VULKAN_BACKEND
using Vortice.Graphics.Vulkan;
#endif

#if !EXCLUDE_D3D12_BACKEND
using Vortice.Graphics.D3D12;
#endif

#if !EXCLUDE_D3D11_BACKEND
using Vortice.Graphics.D3D11;
#endif

namespace Vortice.Graphics;

/// <summary>
/// A <see langword="class"/> with methods to inspect the available devices on the current machine.
/// </summary>
internal static partial class GPUDeviceHelper
{
    public static void Shutdown()
    {
#if !EXCLUDE_VULKAN_BACKEND
        VulkanFactory.Shutdown();
#endif

#if !EXCLUDE_D3D11_BACKEND
        //D3D11.D3D11GPUDeviceFactory.Shutdown();
#endif

#if !EXCLUDE_D3D12_BACKEND
        //D3D12.D3D12GPUDeviceFactory.Shutdown();
#endif
    }

    public static GraphicsDevice CreateDevice(in GraphicsDeviceDescriptor? descriptor = default)
    {
        GraphicsDeviceDescriptor defaultDesc = descriptor ?? new GraphicsDeviceDescriptor(BackendType.Count);
        BackendType backend = defaultDesc.PreferredBackend;
        if (backend == BackendType.Count)
        {
            backend = GetPlatformBackend();
        }

        switch (backend)
        {
#if !EXCLUDE_VULKAN_BACKEND
            case BackendType.Vulkan:
                if (IsBackendSupported(BackendType.Vulkan))
                {
                    return VulkanFactory.Create(defaultDesc);
                }

                throw new GraphicsException($"{nameof(BackendType.Vulkan)} is not supported");
#endif

#if !EXCLUDE_D3D11_BACKEND
            case BackendType.D3D11:
                if (IsBackendSupported(BackendType.D3D11))
                {
                    return D3D11Factory.Create(defaultDesc);
                }

                throw new GraphicsException($"{nameof(BackendType.D3D11)} is not supported");
#endif

#if !EXCLUDE_D3D12_BACKEND
            case BackendType.D3D12:
                if (IsBackendSupported(BackendType.D3D12))
                {
                    return D3D12Factory.Create(defaultDesc);
                }

                throw new GraphicsException($"{nameof(BackendType.D3D12)} is not supported");
#endif

            default:
                throw new GraphicsException($"{backend} is not supported!");
        }
    }

    public static bool IsBackendSupported(BackendType backend)
    {
        if (backend == BackendType.Count)
        {
            backend = GetPlatformBackend();
        }

        switch (backend)
        {
#if !EXCLUDE_VULKAN_BACKEND
            case BackendType.Vulkan:
                return VulkanUtils.IsSupported();
#endif

#if !EXCLUDE_D3D11_BACKEND
            case BackendType.D3D11:
                return D3D11Factory.IsSupported();
#endif

#if !EXCLUDE_D3D12_BACKEND
            case BackendType.D3D12:
                return D3D12Factory.IsSupported();
#endif

            default:
                return false;
        }
    }

    public static BackendType GetPlatformBackend()
    {
        if (PlatformInfo.IsWindows)
        {
#if !EXCLUDE_D3D12_BACKEND
            if (D3D12Factory.IsSupported())
                return BackendType.D3D12;
#endif

#if !EXCLUDE_VULKAN_BACKEND
            if (VulkanUtils.IsSupported())
                return BackendType.Vulkan;
#endif

#if !EXCLUDE_D3D11_BACKEND
            if (D3D11Factory.IsSupported())
                return BackendType.D3D11;
#endif
        }
        else if (PlatformInfo.IsAndroid || PlatformInfo.IsLinux)
        {
#if !EXCLUDE_VULKAN_BACKEND
            if (VulkanUtils.IsSupported())
                return BackendType.Vulkan;
#endif
        }
        else if (PlatformInfo.IsMacOS)
        {
            // TODO: Metal
        }

        return BackendType.Count;
    }
}
