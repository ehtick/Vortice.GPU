// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.


using System.Runtime.InteropServices;
using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;
using static Vortice.GPU.Vulkan.VulkanUtils;

namespace Vortice.GPU.Vulkan;

internal static unsafe class VulkanGPUDeviceFactory
{
    private static readonly VkString s_EngineName = new VkString("Vortice");
    public static readonly Lazy<bool> IsSupported = new(CheckIsSupported);
#if !NET5_0_OR_GREATER
    private static readonly PFN_vkDebugUtilsMessengerCallbackEXT DebugMessagerCallbackDelegate = DebugMessengerCallback;
#endif
    private static VkInstance s_Instance;
    private static VkDebugUtilsMessengerEXT s_debugMessenger;

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

    public static bool DebugUtils { get; private set; }
    public static VkInstance Instance => s_Instance;
    public static VkDebugUtilsMessengerEXT DebugMessenger => s_debugMessenger;

    public static void Shutdown()
    {
    }

    public static VulkanGPUDevice Create(in GPUDeviceDescriptor descriptor)
    {
        if (Instance.IsNull)
        {
            var appInfo = new VkApplicationInfo
            {
                sType = VkStructureType.ApplicationInfo,
                //pApplicationName = name,
                //applicationVersion = new VkVersion(1, 0, 0),
                pEngineName = s_EngineName,
                engineVersion = new VkVersion(1, 0, 0),
                apiVersion = VkVersion.Version_1_2
            };

            ReadOnlySpan<VkExtensionProperties> availableExtensions = vkEnumerateInstanceExtensionProperties();
            HashSet<string> availableInstanceExtensions = new(GetInstanceExtensions());

            List<string> instanceLayers = new();

            if (descriptor.ValidationMode != ValidationMode.Disabled)
            {
                // Determine the optimal validation layers to enable that are necessary for useful debugging
                GetOptimalValidationLayers(instanceLayers);
            }

            List<string> instanceExtensions = new();

            // Check if VK_EXT_debug_utils is supported, which supersedes VK_EXT_Debug_Report
            foreach (VkExtensionProperties availableExtension in availableExtensions)
            {
                if (availableExtension.GetExtensionName() == VK_EXT_DEBUG_UTILS_EXTENSION_NAME)
                {
                    DebugUtils = true;
                    instanceExtensions.Add(VK_EXT_DEBUG_UTILS_EXTENSION_NAME);
                }
                else if (availableExtension.GetExtensionName() == VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME)
                {
                    instanceExtensions.Add(VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME);
                }
                else if (availableExtension.GetExtensionName() == VK_EXT_SWAPCHAIN_COLOR_SPACE_EXTENSION_NAME)
                {
                    instanceExtensions.Add(VK_EXT_SWAPCHAIN_COLOR_SPACE_EXTENSION_NAME);
                }
            }

            instanceExtensions.Add(VK_KHR_SURFACE_EXTENSION_NAME);
            if (PlatformInfo.IsWindows)
            {
                instanceExtensions.Add("VK_KHR_win32_surface");
            }
            else if (PlatformInfo.IsLinux)
            {
                if (availableInstanceExtensions.Contains(VK_KHR_ANDROID_SURFACE_EXTENSION_NAME))
                {
                    instanceExtensions.Add(VK_KHR_ANDROID_SURFACE_EXTENSION_NAME);
                }
                if (availableInstanceExtensions.Contains("VK_KHR_xlib_surface"))
                {
                    instanceExtensions.Add("VK_KHR_xlib_surface");
                }
                if (availableInstanceExtensions.Contains("VK_KHR_wayland_surface"))
                {
                    instanceExtensions.Add("VK_KHR_wayland_surface");
                }
            }
            else if (PlatformInfo.IsMacOS)
            {
                if (availableInstanceExtensions.Contains("VK_EXT_metal_surface"))
                {
                    instanceExtensions.Add("VK_EXT_metal_surface");
                }
                else
                {
                    // Legacy MoltenVK extensions
                    if (availableInstanceExtensions.Contains("VK_MVK_macos_surface"))
                    {
                        instanceExtensions.Add("VK_MVK_macos_surface");
                    }
                    if (availableInstanceExtensions.Contains("VK_MVK_ios_surface"))
                    {
                        instanceExtensions.Add("VK_MVK_ios_surface");
                    }
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
#if NET5_0_OR_GREATER
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

            vkLoadInstance(s_Instance);

            if (descriptor.ValidationMode != ValidationMode.Disabled && DebugUtils)
            {
                vkCreateDebugUtilsMessengerEXT(s_Instance, &debugUtilsCreateInfo, null, out s_debugMessenger).CheckResult();
            }
        }

        VkPhysicalDevice physicalDevice = VkPhysicalDevice.Null;
        ReadOnlySpan<VkPhysicalDevice> physicalDevices = vkEnumeratePhysicalDevices(s_Instance);
        foreach (VkPhysicalDevice candidatePhysicalDevice in physicalDevices)
        {
            vkGetPhysicalDeviceProperties(candidatePhysicalDevice, out VkPhysicalDeviceProperties properties);

            bool discrete = properties.deviceType == VkPhysicalDeviceType.DiscreteGpu;

            if (discrete && descriptor.PowerPreference != GPUPowerPreference.HighPerformance)
                continue;

            VulkanPhysicalDeviceExtensions physicalDeviceExt = QueryPhysicalDeviceExtensions(candidatePhysicalDevice);
            bool suitable = physicalDeviceExt.Swapchain && physicalDeviceExt.depth_clip_enable;

            if (!suitable)
            {
                continue;
            }

            if (discrete || physicalDevice.IsNull)
            {
                physicalDevice = candidatePhysicalDevice;
                if (discrete)
                {
                    break; // if this is discrete GPU, look no further (prioritize discrete GPU)
                }
            }
        }

        if (physicalDevice.IsNull)
        {
            throw new GPUException("Vulkan: Failed to find GPUs with Vulkan support");
        }

        return new VulkanGPUDevice(physicalDevice);
    }

    private static string[] GetInstanceExtensions()
    {
        uint count = 0;
        VkResult result = vkEnumerateInstanceExtensionProperties((byte*)null, &count, null);
        if (result != VkResult.Success)
        {
            return Array.Empty<string>();
        }

        if (count == 0)
        {
            return Array.Empty<string>();
        }

        VkExtensionProperties* props = stackalloc VkExtensionProperties[(int)count];
        vkEnumerateInstanceExtensionProperties((byte*)null, &count, props);

        string[] extensions = new string[count];
        for (int i = 0; i < count; i++)
        {
            extensions[i] = props[i].GetExtensionName();
        }

        return extensions;
    }

    private static void GetOptimalValidationLayers(List<string> instanceLayers)
    {
        ReadOnlySpan<VkLayerProperties> availableLayers = vkEnumerateInstanceLayerProperties();

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

    private static bool ValidateLayers(List<string> required, ReadOnlySpan<VkLayerProperties> availableLayers)
    {
        foreach (string layer in required)
        {
            bool found = false;
            foreach (VkLayerProperties availableLayer in availableLayers)
            {
                if (availableLayer.GetLayerName() == layer)
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

#if NET5_0_OR_GREATER
        [UnmanagedCallersOnly]
#endif
    private static uint DebugMessengerCallback(VkDebugUtilsMessageSeverityFlagsEXT messageSeverity,
        VkDebugUtilsMessageTypeFlagsEXT messageTypes,
        VkDebugUtilsMessengerCallbackDataEXT* pCallbackData,
        void* userData)
    {
        string? message = Interop.GetString(pCallbackData->pMessage);
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
