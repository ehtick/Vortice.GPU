// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.GPU.Vulkan;

public class VulkanGPUDevice : GPUDevice
{
    public static readonly Lazy<bool> IsSupported = new(CheckIsSupported);

    private static bool CheckIsSupported()
    {
        try
        {
            VkResult result = vkInitialize();
            if (result != VkResult.Success)
            {
                return false;
            }

            // TODO: Enumerate physical devices and try to create instance.

            return true;
        }
        catch
        {
            return false;
        }
    }

    public VkDevice NativeDevice { get; }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        WaitIdle();
    }

    /// <inheritdoc />
    public override void WaitIdle()
    {
        vkDeviceWaitIdle(NativeDevice).CheckResult(); 
    }
}
