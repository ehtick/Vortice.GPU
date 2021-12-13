// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.GPU.Vulkan;

internal unsafe class VulkanTexture : Texture
{
    private VkImage _handle;

    public VulkanTexture(VulkanGPUDevice device) : base(device)
    {
        VkImageCreateInfo createInfo = new VkImageCreateInfo
        {
            sType = VkStructureType.ImageCreateInfo,
            pNext = null,
            flags = VkImageCreateFlags.None,
            imageType = VkImageType.Image2D
        };

        vkCreateImage(device.NativeDevice, &createInfo, null, out _handle).CheckResult();
    }

    public VkImage Handle => _handle;
}
