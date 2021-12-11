// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Vulkan;

namespace Vortice.GPU.Vulkan;

public class VulkanDevice : Device
{
    public VkDevice NativeDevice { get; }

    protected override void OnDispose() => throw new NotImplementedException();
}
