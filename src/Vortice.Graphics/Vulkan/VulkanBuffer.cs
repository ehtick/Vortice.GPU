// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.Graphics.Vulkan;

internal unsafe class VulkanBuffer : Buffer
{
    private VkBuffer _handle;

    public VulkanBuffer(VulkanGraphicsDevice device, in BufferDescriptor descriptor, IntPtr initialData)
        : base(device, descriptor)
    {
        VkBufferCreateInfo createInfo = new VkBufferCreateInfo
        {
            sType = VkStructureType.BufferCreateInfo,
            pNext = null,
            flags = VkBufferCreateFlags.None,
            size = descriptor.Size,
            usage = VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst
        };
        
        vkCreateBuffer(device.NativeDevice, &createInfo, null, out _handle).CheckResult();
    }

    public VkBuffer Handle => _handle;
}
