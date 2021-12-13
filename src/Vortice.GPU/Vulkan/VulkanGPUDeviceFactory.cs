// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.


using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.GPU.Vulkan;

internal static unsafe class VulkanGPUDeviceFactory
{
    private static readonly VkString s_EngineName = new VkString("Vortice");
    public static readonly Lazy<bool> IsSupported = new(CheckIsSupported);
    private static VkInstance s_vkInstance;

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

    public static void Shutdown()
    {
        if (s_vkInstance != VkInstance.Null)
        {
            vkDestroyInstance(s_vkInstance, null);
            s_vkInstance = VkInstance.Null;
        }
    }

    public static VkInstance Instance
    {
        get
        {
            if (s_vkInstance == VkInstance.Null)
            {
                s_vkInstance = CreateInstance();
            }

            return s_vkInstance;
        }
        set => s_vkInstance = value;
    }

    private static VkInstance CreateInstance()
    {
        //VkString name = applicationName;
        var appInfo = new VkApplicationInfo
        {
            sType = VkStructureType.ApplicationInfo,
            //pApplicationName = name,
            //applicationVersion = new VkVersion(1, 0, 0),
            pEngineName = s_EngineName,
            engineVersion = new VkVersion(1, 0, 0),
            apiVersion = vkEnumerateInstanceVersion()
        };

        List<string> instanceExtensions = new();

        VkInstanceCreateInfo createInfo = new()
        {
            sType = VkStructureType.InstanceCreateInfo,
            pApplicationInfo = &appInfo,
            //enabledExtensionCount = vkInstanceExtensions.Length,
            //ppEnabledExtensionNames = vkInstanceExtensions
        };

        VkResult result = vkCreateInstance(&createInfo, null, out VkInstance instance);
        if (result != VkResult.Success)
        {
            throw new InvalidOperationException($"Failed to create vulkan instance: {result}");
        }

        vkLoadInstance(instance);
        return instance;
    }

    public static VulkanGPUDevice CreateDefault()
    {
        VkPhysicalDevice physicalDevice = VkPhysicalDevice.Null;

        return new VulkanGPUDevice(physicalDevice);
    }
}
