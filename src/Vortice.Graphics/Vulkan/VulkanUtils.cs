// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.Graphics.Vulkan;

internal static class VulkanUtils
{
    private static readonly Lazy<bool> s_isSupported = new(CheckIsSupported);

    public static bool IsSupported() => s_isSupported.Value;

    private static bool CheckIsSupported()
    {
        try
        {
            VkResult result = vkInitialize();
            if (result != VkResult.Success)
                return false;

            // We require Vulkan 1.1 or higher
            VkVersion version = vkEnumerateInstanceVersion();
            if (version < VkVersion.Version_1_1)
                return false;

            // TODO: Enumerate physical devices and try to create instance.

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static VulkanPhysicalDeviceExtensions QueryPhysicalDeviceExtensions(VkPhysicalDevice physicalDevice)
    {
        VulkanPhysicalDeviceExtensions extensions = new VulkanPhysicalDeviceExtensions();

        ReadOnlySpan<VkExtensionProperties> availableDeviceExtensions = vkEnumerateDeviceExtensionProperties(physicalDevice);

        foreach (VkExtensionProperties vkExtension in availableDeviceExtensions)
        {
            if (vkExtension.GetExtensionName() == VK_KHR_SWAPCHAIN_EXTENSION_NAME)
            {
                extensions.Swapchain = true;
            }
            else if (vkExtension.GetExtensionName() == VK_EXT_DEPTH_CLIP_ENABLE_EXTENSION_NAME)
            {
                extensions.depth_clip_enable = true;
            }
            else if (vkExtension.GetExtensionName() == VK_EXT_MEMORY_BUDGET_EXTENSION_NAME)
            {
                extensions.memory_budget = true;
            }
            else if (vkExtension.GetExtensionName() == VK_KHR_PERFORMANCE_QUERY_EXTENSION_NAME)
            {
                extensions.performance_query = true;
            }
            else if (vkExtension.GetExtensionName() == VK_KHR_DEFERRED_HOST_OPERATIONS_EXTENSION_NAME)
            {
                extensions.deferred_host_operations = true;
            }
            else if (vkExtension.GetExtensionName() == VK_KHR_CREATE_RENDERPASS_2_EXTENSION_NAME)
            {
                extensions.renderPass2 = true;
            }
            else if (vkExtension.GetExtensionName() == VK_KHR_ACCELERATION_STRUCTURE_EXTENSION_NAME)
            {
                extensions.accelerationStructure = true;
            }
            else if (vkExtension.GetExtensionName() == VK_KHR_RAY_TRACING_PIPELINE_EXTENSION_NAME)
            {
                extensions.raytracingPipeline = true;
            }
            else if (vkExtension.GetExtensionName() == VK_KHR_RAY_QUERY_EXTENSION_NAME)
            {
                extensions.rayQuery = true;
            }
            else if (vkExtension.GetExtensionName() == VK_KHR_FRAGMENT_SHADING_RATE_EXTENSION_NAME)
            {
                extensions.fragment_shading_rate = true;
            }
            else if (vkExtension.GetExtensionName() == VK_NV_MESH_SHADER_EXTENSION_NAME)
            {
                extensions.NV_mesh_shader = true;
            }
            else if (vkExtension.GetExtensionName() == VK_EXT_CONDITIONAL_RENDERING_EXTENSION_NAME)
            {
                extensions.EXT_conditional_rendering = true;
            }
            else if (vkExtension.GetExtensionName() == "VK_EXT_full_screen_exclusive")
            {
                extensions.win32_full_screen_exclusive = true;
            }
            else if (vkExtension.GetExtensionName() == VK_EXT_VERTEX_ATTRIBUTE_DIVISOR_EXTENSION_NAME)
            {
                extensions.vertex_attribute_divisor = true;
            }
            else if (vkExtension.GetExtensionName() == VK_EXT_EXTENDED_DYNAMIC_STATE_EXTENSION_NAME)
            {
                extensions.extended_dynamic_state = true;
            }
            else if (vkExtension.GetExtensionName() == VK_EXT_VERTEX_INPUT_DYNAMIC_STATE_EXTENSION_NAME)
            {
                extensions.vertex_input_dynamic_state = true;
            }
            else if (vkExtension.GetExtensionName() == VK_EXT_EXTENDED_DYNAMIC_STATE_2_EXTENSION_NAME)
            {
                extensions.extended_dynamic_state2 = true;
            }
            else if (vkExtension.GetExtensionName() == VK_KHR_DYNAMIC_RENDERING_EXTENSION_NAME)
            {
                extensions.dynamic_rendering = true;
            }

            vkGetPhysicalDeviceProperties(physicalDevice, out VkPhysicalDeviceProperties properties);

            if (properties.apiVersion >= VkVersion.Version_1_2)
            {
                extensions.renderPass2 = true;
            }
            else if (properties.apiVersion >= VkVersion.Version_1_1)
            {
            }
        }

        return extensions;
    }
}

public unsafe struct VkNativeArray<T> where T : unmanaged
{
    public const int CapacityInBytes = 256;
    private static readonly int s_sizeofT = sizeof(T);

    private fixed byte _storage[CapacityInBytes];
    private uint _count;

    public uint Count => _count;
    public T* Data => (T*)Unsafe.AsPointer(ref this);

    public void Add(T item)
    {
        byte* basePtr = (byte*)Data;
        int offset = (int)(_count * s_sizeofT);
#if DEBUG
        Debug.Assert((offset + s_sizeofT) <= CapacityInBytes);
#endif
        Unsafe.Write(basePtr + offset, item);

        _count += 1;
    }

    public ref T this[uint index]
    {
        get
        {
            byte* basePtr = (byte*)Unsafe.AsPointer(ref this);
            int offset = (int)(index * s_sizeofT);
            return ref Unsafe.AsRef<T>(basePtr + offset);
        }
    }

    public ref T this[int index]
    {
        get
        {
            byte* basePtr = (byte*)Unsafe.AsPointer(ref this);
            int offset = index * s_sizeofT;
            return ref Unsafe.AsRef<T>(basePtr + offset);
        }
    }
}
