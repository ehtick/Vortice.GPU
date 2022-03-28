// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Vortice.Graphics.Vulkan.VulkanUtils;

namespace Vortice.Graphics.Vulkan;

internal static unsafe class VulkanFactory
{
    private static readonly VkString s_engineName = new("Vortice");

#if !NET6_0_OR_GREATER
    private static readonly PFN_vkDebugUtilsMessengerCallbackEXT DebugMessagerCallbackDelegate = DebugMessengerCallback;
#endif
    private static VkInstance s_Instance;
    private static VkDebugUtilsMessengerEXT s_debugMessenger;

    public static bool DebugUtils { get; private set; }
    public static VkInstance Instance => s_Instance;

    public static void Shutdown()
    {
        if (s_debugMessenger != VkDebugUtilsMessengerEXT.Null)
        {
            vkDestroyDebugUtilsMessengerEXT(s_Instance, s_debugMessenger);
            s_debugMessenger = VkDebugUtilsMessengerEXT.Null;
        }

        if (s_Instance != VkInstance.Null)
        {
            vkDestroyInstance(s_Instance);
            s_Instance = VkInstance.Null;
        }
    }

    public static VulkanGraphicsDevice Create(in GraphicsDeviceDescriptor descriptor)
    {
        if (Instance.IsNull)
        {
            var appInfo = new VkApplicationInfo
            {
                sType = VkStructureType.ApplicationInfo,
                //pApplicationName = name,
                //applicationVersion = new VkVersion(1, 0, 0),
                pEngineName = s_engineName,
                engineVersion = new VkVersion(1, 0, 0),
                apiVersion = VkVersion.Version_1_2
            };

            HashSet<string> availableInstanceLayers = new(EnumerateInstanceLayers());
            HashSet<string> availableInstanceExtensions = new(GetInstanceExtensions());

            List<string> instanceLayers = new();

            if (descriptor.ValidationMode != ValidationMode.Disabled)
            {
                // Determine the optimal validation layers to enable that are necessary for useful debugging
                GetOptimalValidationLayers(availableInstanceLayers, instanceLayers);
            }

            List<string> instanceExtensions = new();

            // Check if VK_EXT_debug_utils is supported, which supersedes VK_EXT_Debug_Report
            foreach (string availableExtension in availableInstanceExtensions)
            {
                if (availableExtension == VK_EXT_DEBUG_UTILS_EXTENSION_NAME)
                {
                    DebugUtils = true;
                    instanceExtensions.Add(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
                }
                else if (availableExtension == VK_EXT_SWAPCHAIN_COLOR_SPACE_EXTENSION_NAME)
                {
                    instanceExtensions.Add(VK_EXT_SWAPCHAIN_COLOR_SPACE_EXTENSION_NAME);
                }
            }

            instanceExtensions.Add(VK_KHR_SURFACE_EXTENSION_NAME);
            if (PlatformInfo.IsWindows)
            {
                instanceExtensions.Add(VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
            }
            else if (PlatformInfo.IsAndroid)
            {
                instanceExtensions.Add(VK_KHR_ANDROID_SURFACE_EXTENSION_NAME);
            }
            else if (PlatformInfo.IsMacOS)
            {
                instanceExtensions.Add(VK_EXT_METAL_SURFACE_EXTENSION_NAME);
            }
            else if (PlatformInfo.IsLinux)
            {
                if (availableInstanceExtensions.Contains(VK_KHR_XCB_SURFACE_EXTENSION_NAME))
                {
                    instanceExtensions.Add(VK_KHR_XCB_SURFACE_EXTENSION_NAME);
                }
                if (availableInstanceExtensions.Contains(VK_KHR_XLIB_SURFACE_EXTENSION_NAME))
                {
                    instanceExtensions.Add(VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
                }
                if (availableInstanceExtensions.Contains(KHRWaylandSurfaceExtensionName))
                {
                    instanceExtensions.Add(KHRWaylandSurfaceExtensionName);
                }
            }

            using VkStringArray vkLayerNames = new(instanceLayers);
            using VkStringArray vkInstanceExtensions = new(instanceExtensions);

            VkInstanceCreateInfo createInfo = new()
            {
                sType = VkStructureType.InstanceCreateInfo,
                pApplicationInfo = &appInfo,
                enabledLayerCount = vkLayerNames.Length,
                ppEnabledLayerNames = vkLayerNames,
                enabledExtensionCount = vkInstanceExtensions.Length,
                ppEnabledExtensionNames = vkInstanceExtensions
            };

            VkDebugUtilsMessengerCreateInfoEXT debugUtilsCreateInfo = new()
            {
                sType = VkStructureType.DebugUtilsMessengerCreateInfoEXT
            };

            if (descriptor.ValidationMode != ValidationMode.Disabled && DebugUtils)
            {
                debugUtilsCreateInfo.messageSeverity = VkDebugUtilsMessageSeverityFlagsEXT.Error | VkDebugUtilsMessageSeverityFlagsEXT.Warning;
                debugUtilsCreateInfo.messageType = VkDebugUtilsMessageTypeFlagsEXT.Validation | VkDebugUtilsMessageTypeFlagsEXT.Performance;

                if (descriptor.ValidationMode == ValidationMode.Verbose)
                {
                    debugUtilsCreateInfo.messageSeverity |= VkDebugUtilsMessageSeverityFlagsEXT.Verbose | VkDebugUtilsMessageSeverityFlagsEXT.Info;
                }

#if NET6_0_OR_GREATER
                debugUtilsCreateInfo.pfnUserCallback = &DebugMessengerCallback;
#else
                debugUtilsCreateInfo.pfnUserCallback = Marshal.GetFunctionPointerForDelegate(DebugMessagerCallbackDelegate);
#endif
                createInfo.pNext = &debugUtilsCreateInfo;
            }

            VkResult result = vkCreateInstance(&createInfo, null, out s_Instance);
            if (result != VkResult.Success)
            {
                throw new InvalidOperationException($"Failed to create vulkan instance: {result}");
            }

            vkLoadInstanceOnly(s_Instance);

            if (descriptor.ValidationMode != ValidationMode.Disabled && DebugUtils)
            {
                vkCreateDebugUtilsMessengerEXT(s_Instance, &debugUtilsCreateInfo, null, out s_debugMessenger).CheckResult();
            }
        }

        // Find physical device, setup queue's and create device.
        int physicalDevicesCount = 0;
        vkEnumeratePhysicalDevices(s_Instance, &physicalDevicesCount, null).CheckResult();

        if (physicalDevicesCount == 0)
        {
            throw new GraphicsException("Vulkan: Failed to find GPUs with Vulkan support");
        }

        VkPhysicalDevice* physicalDevices = stackalloc VkPhysicalDevice[physicalDevicesCount];
        vkEnumeratePhysicalDevices(s_Instance, &physicalDevicesCount, physicalDevices).CheckResult();

        VkPhysicalDevice physicalDevice = VkPhysicalDevice.Null;
        for (int i = 0; i < physicalDevicesCount; i++)
        {
            physicalDevice = physicalDevices[i];

            vkGetPhysicalDeviceProperties(physicalDevice, out VkPhysicalDeviceProperties properties);

            bool discrete = properties.deviceType == VkPhysicalDeviceType.DiscreteGpu;

            if (discrete && descriptor.PowerPreference != PowerPreference.HighPerformance)
                continue;

            VulkanPhysicalDeviceExtensions physicalDeviceExt = QueryPhysicalDeviceExtensions(physicalDevice);
            bool suitable = physicalDeviceExt.Swapchain && physicalDeviceExt.depth_clip_enable;

            if (!suitable)
            {
                continue;
            }

            if (discrete)
            {
                // If this is discrete GPU, look no further (prioritize discrete GPU)
                break; 
            }
        }

        if (physicalDevice.IsNull)
        {
            throw new GraphicsException("Vulkan: Failed to find GPUs with Vulkan support");
        }

        return new VulkanGraphicsDevice(physicalDevice);
    }

    private static string[] EnumerateInstanceLayers()
    {
        if (!IsSupported())
        {
            return Array.Empty<string>();
        }

        int count = 0;
        VkResult result = vkEnumerateInstanceLayerProperties(&count, null);
        if (result != VkResult.Success)
        {
            return Array.Empty<string>();
        }

        if (count == 0)
        {
            return Array.Empty<string>();
        }

        VkLayerProperties* properties = stackalloc VkLayerProperties[count];
        vkEnumerateInstanceLayerProperties(&count, properties).CheckResult();

        string[] resultExt = new string[count];
        for (int i = 0; i < count; i++)
        {
            resultExt[i] = properties[i].GetLayerName();
        }

        return resultExt;
    }

    private static string[] GetInstanceExtensions()
    {
        int count = 0;
        VkResult result = vkEnumerateInstanceExtensionProperties((byte*)null, &count, null);
        if (result != VkResult.Success)
        {
            return Array.Empty<string>();
        }

        if (count == 0)
        {
            return Array.Empty<string>();
        }

        VkExtensionProperties* props = stackalloc VkExtensionProperties[count];
        vkEnumerateInstanceExtensionProperties((byte*)null, &count, props);

        string[] extensions = new string[count];
        for (int i = 0; i < count; i++)
        {
            extensions[i] = props[i].GetExtensionName();
        }

        return extensions;
    }

    private static void GetOptimalValidationLayers(HashSet<string> availableLayers, List<string> instanceLayers)
    {
        // The preferred validation layer is "VK_LAYER_KHRONOS_validation"
        List<string> validationLayers = new()
        {
            "VK_LAYER_KHRONOS_validation"
        };

        if (ValidateLayers(validationLayers, availableLayers))
        {
            instanceLayers.AddRange(validationLayers);
            return;
        }

        // Otherwise we fallback to using the LunarG meta layer
        validationLayers = new()
        {
            "VK_LAYER_LUNARG_standard_validation"
        };

        if (ValidateLayers(validationLayers, availableLayers))
        {
            instanceLayers.AddRange(validationLayers);
            return;
        }

        // Otherwise we attempt to enable the individual layers that compose the LunarG meta layer since it doesn't exist
        validationLayers = new()
        {
            "VK_LAYER_GOOGLE_threading",
            "VK_LAYER_LUNARG_parameter_validation",
            "VK_LAYER_LUNARG_object_tracker",
            "VK_LAYER_LUNARG_core_validation",
            "VK_LAYER_GOOGLE_unique_objects",
        };

        if (ValidateLayers(validationLayers, availableLayers))
        {
            instanceLayers.AddRange(validationLayers);
            return;
        }

        // Otherwise as a last resort we fallback to attempting to enable the LunarG core layer
        validationLayers = new()
        {
            "VK_LAYER_LUNARG_core_validation"
        };

        if (ValidateLayers(validationLayers, availableLayers))
        {
            instanceLayers.AddRange(validationLayers);
            return;
        }
    }

    private static bool ValidateLayers(List<string> required, HashSet<string> availableLayers)
    {
        foreach (string layer in required)
        {
            bool found = false;
            foreach (string availableLayer in availableLayers)
            {
                if (availableLayer == layer)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                //Log.Warn("Validation Layer '{}' not found", layer);
                return false;
            }
        }

        return true;
    }

#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly]
#endif
    private static uint DebugMessengerCallback(VkDebugUtilsMessageSeverityFlagsEXT messageSeverity,
        VkDebugUtilsMessageTypeFlagsEXT messageTypes,
        VkDebugUtilsMessengerCallbackDataEXT* pCallbackData,
        void* userData)
    {
        string message = Interop.GetString(pCallbackData->pMessage);
        if (messageTypes == VkDebugUtilsMessageTypeFlagsEXT.Validation)
        {
            if (messageSeverity == VkDebugUtilsMessageSeverityFlagsEXT.Error)
            {
                //Log.Error($"[Vulkan]: Validation: {messageSeverity} - {message}");
            }
            else if (messageSeverity == VkDebugUtilsMessageSeverityFlagsEXT.Warning)
            {
                //Log.Warn($"[Vulkan]: Validation: {messageSeverity} - {message}");
            }

            //Debug.WriteLine($"[Vulkan]: Validation: {messageSeverity} - {message}");
        }
        else
        {
            if (messageSeverity == VkDebugUtilsMessageSeverityFlagsEXT.Error)
            {
                //Log.Error($"[Vulkan]: {messageSeverity} - {message}");
            }
            else if (messageSeverity == VkDebugUtilsMessageSeverityFlagsEXT.Warning)
            {
                //Log.Warn($"[Vulkan]: {messageSeverity} - {message}");
            }

            //Debug.WriteLine($"[Vulkan]: {messageSeverity} - {message}");
        }

        return VK_FALSE;
    }
}
