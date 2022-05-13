// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics;

/// <summary>
/// A bitmask indicating how a resource need to be shared with other graphics APIs or other GPU devices.
/// </summary>
[Flags]
public enum SharedResourceFlags
{
    None = 0,
    /// <summary>
    /// D3D11: adds D3D11_RESOURCE_MISC_SHARED
    /// D3D12: adds D3D12_HEAP_FLAG_SHARED
    /// Vulkan: Ignored
    /// </summary>
    Shared = 0x01,
    /// <summary>
    /// D3D11: adds (D3D11_RESOURCE_MISC_SHARED_KEYEDMUTEX | D3D11_RESOURCE_MISC_SHARED_NTHANDLE)
    /// D3D12, Vulkan: ignored
    /// </summary>
    Shared_NTHandle = 0x02,
    /// <summary>
    /// D3D12: adds D3D12_RESOURCE_FLAG_ALLOW_CROSS_ADAPTER and D3D12_HEAP_FLAG_SHARED_CROSS_ADAPTER
    /// D3D11, Vulkan: ignored
    /// </summary>
    Shared_CrossAdapter = 0x04,
}
