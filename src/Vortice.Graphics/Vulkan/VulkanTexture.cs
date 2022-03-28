// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.Graphics.Vulkan;

internal unsafe class VulkanTexture : Texture
{
    private VkImage _handle;

    public VulkanTexture(VulkanGraphicsDevice device, in TextureDescriptor descriptor)
        : base(device, descriptor)
    {
        //VkImageCreateInfo createInfo = new VkImageCreateInfo
        //{
        //    sType = VkStructureType.ImageCreateInfo,
        //    pNext = null,
        //    flags = VkImageCreateFlags.None,
        //    imageType = VkImageType.Image2D
        //};
        //
        //vkCreateImage(device.NativeDevice, &createInfo, null, out _handle).CheckResult();
    }

    public VkImage Handle => _handle;
}
