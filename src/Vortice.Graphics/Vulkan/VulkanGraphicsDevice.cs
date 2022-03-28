// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Vulkan;
using static System.Math;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.Graphics.Vulkan;

internal unsafe class VulkanGraphicsDevice : GraphicsDevice
{
    private readonly VkDevice _handle;
    private readonly GraphicsAdapterInfo _adapterInfo;
    private readonly GraphicsDeviceFeatures _features;
    private readonly GraphicsDeviceLimits _limits;

    private readonly uint _graphicsQueueFamily = VK_QUEUE_FAMILY_IGNORED;
    private readonly uint _computeQueueFamily = VK_QUEUE_FAMILY_IGNORED;
    private readonly uint _copyQueueFamily = VK_QUEUE_FAMILY_IGNORED;

    private readonly VkQueue _graphicsQueue = VkQueue.Null;
    private readonly VkQueue _computeQueue = VkQueue.Null;
    private readonly VkQueue _copyQueue = VkQueue.Null;

    private readonly VkPhysicalDeviceAccelerationStructureFeaturesKHR acceleration_structure_features;
    private readonly VkPhysicalDeviceRayTracingPipelineFeaturesKHR raytracing_features;
    private readonly VkPhysicalDeviceRayQueryFeaturesKHR raytracing_query_features;
    private readonly VkPhysicalDeviceFragmentShadingRateFeaturesKHR fragment_shading_rate_features;
    private readonly VkPhysicalDeviceMeshShaderFeaturesNV mesh_shader_features;
    private readonly VkPhysicalDeviceVertexAttributeDivisorFeaturesEXT vertexDivisorFeatures;
    private readonly VkPhysicalDeviceExtendedDynamicStateFeaturesEXT extendedDynamicStateFeatures;
    private readonly VkPhysicalDeviceVertexInputDynamicStateFeaturesEXT vertexInputDynamicStateFeatures;
    private readonly VkPhysicalDeviceExtendedDynamicState2FeaturesEXT extended_dynamic_state2_features;
    private readonly VkPhysicalDeviceConditionalRenderingFeaturesEXT conditional_rendering_features;
    private readonly VkPhysicalDeviceDepthClipEnableFeaturesEXT depth_clip_enable_features;
    private readonly VkPhysicalDeviceDynamicRenderingFeatures dynamicRenderingFeaturesKHR;

    public VulkanGraphicsDevice(VkPhysicalDevice physicalDevice)
    {
        PhysicalDevice = physicalDevice;

        vkGetPhysicalDeviceProperties(physicalDevice, out VkPhysicalDeviceProperties properties);

        VkPhysicalDeviceFeatures2 features2 = new VkPhysicalDeviceFeatures2
        {
            sType = VkStructureType.PhysicalDeviceFeatures2
        };

        VkPhysicalDeviceVulkan11Features features_1_1 = new()
        {
            sType = VkStructureType.PhysicalDeviceVulkan11Features
        };


        VkPhysicalDeviceVulkan12Features features_1_2 = new()
        {
            sType = VkStructureType.PhysicalDeviceVulkan12Features
        };

        features2.pNext = &features_1_1;
        features_1_1.pNext = &features_1_2;
        void** featuresChain = &features_1_2.pNext;

        acceleration_structure_features = default;
        raytracing_features = default;
        raytracing_query_features = default;
        fragment_shading_rate_features = default;
        mesh_shader_features = default;

        // Properties
        VkPhysicalDeviceProperties2 properties2 = new()
        {
            sType = VkStructureType.PhysicalDeviceProperties2
        };

        VkPhysicalDeviceVulkan11Properties properties_1_1 = new()
        {
            sType = VkStructureType.PhysicalDeviceVulkan11Properties
        };

        VkPhysicalDeviceVulkan12Properties properties_1_2 = new()
        {
            sType = VkStructureType.PhysicalDeviceVulkan12Properties
        };

        VkPhysicalDeviceSamplerFilterMinmaxProperties sampler_minmax_properties = new()
        {
            sType = VkStructureType.PhysicalDeviceSamplerFilterMinmaxProperties
        };

        VkPhysicalDeviceAccelerationStructurePropertiesKHR acceleration_structure_properties = default;
        VkPhysicalDeviceRayTracingPipelinePropertiesKHR raytracing_properties = default;
        VkPhysicalDeviceFragmentShadingRatePropertiesKHR fragment_shading_rate_properties = default;
        VkPhysicalDeviceMeshShaderPropertiesNV mesh_shader_properties = default;

        properties2.pNext = &properties_1_1;
        properties_1_1.pNext = &properties_1_2;
        void** properties_chain = &properties_1_2.pNext;
        *properties_chain = &sampler_minmax_properties;
        properties_chain = &sampler_minmax_properties.pNext;

        vkGetPhysicalDeviceProperties2(PhysicalDevice, &properties2);
        vkGetPhysicalDeviceFeatures2(PhysicalDevice, &features2);

        List<string> enabledExtensions = new();
        enabledExtensions.Add(VK_KHR_SWAPCHAIN_EXTENSION_NAME);

        int queueFamiliesCount = 0;
        vkGetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamiliesCount, null);
        VkQueueFamilyProperties* queueFamilies = stackalloc VkQueueFamilyProperties[queueFamiliesCount];
        vkGetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamiliesCount, queueFamilies);

        uint* queueOffsets = stackalloc uint[queueFamiliesCount];
        VkNativeArray<float>[] queuePriorities = new VkNativeArray<float>[queueFamiliesCount];
        for (int familyIndex = 0; familyIndex < queueFamiliesCount; familyIndex++)
        {
            queuePriorities[familyIndex] = new();
        }

        uint graphicsQueueIndex = 0;
        uint computeQueueIndex = 0;
        uint copyQueueIndex = 0;

        bool FindVacantQueue(ref uint family, ref uint index, int queueFamiliesCount, VkQueueFlags required, VkQueueFlags ignoreFlags, float priority)
        {
            for (int familyIndex = 0; familyIndex < queueFamiliesCount; familyIndex++)
            {
                if ((queueFamilies[familyIndex].queueFlags & ignoreFlags) != 0)
                    continue;

                // A graphics queue candidate must support present for us to select it.
                if ((required & VkQueueFlags.Graphics) != VkQueueFlags.None)
                {
                    //VkBool32 supported = VK_FALSE;
                    //if (vkGetPhysicalDeviceSurfaceSupportKHR(vk.physicalDevice, familyIndex, surface, &supported) != VK_SUCCESS || !supported)
                    //    continue;
                }

                if (queueFamilies[familyIndex].queueCount > 0 &&
                    (queueFamilies[familyIndex].queueFlags & required) == required)
                {
                    family = (uint)familyIndex;
                    queueFamilies[familyIndex].queueCount--;
                    index = queueOffsets[familyIndex]++;
                    queuePriorities[familyIndex].Add(priority);
                    return true;
                }
            }

            return false;
        }

        if (!FindVacantQueue(ref _graphicsQueueFamily, ref graphicsQueueIndex,
             queueFamiliesCount, VkQueueFlags.Graphics | VkQueueFlags.Compute, 0, 0.5f))
        {
            //Log.Error("Vulkan: Could not find suitable graphics queue.");
            return;
        }

        // Prefer another graphics queue since we can do async graphics that way.
        // The compute queue is to be treated as high priority since we also do async graphics on it.
        if (!FindVacantQueue(ref _computeQueueFamily, ref computeQueueIndex, queueFamiliesCount,
            VkQueueFlags.Graphics | VkQueueFlags.Compute, 0, 1.0f) &&
            !FindVacantQueue(ref _computeQueueFamily, ref computeQueueIndex, queueFamiliesCount,
            VkQueueFlags.Compute, 0, 1.0f))
        {
            // Fallback to the graphics queue if we must.
            _computeQueueFamily = _graphicsQueueFamily;
            computeQueueIndex = graphicsQueueIndex;
        }

        // For transfer, try to find a queue which only supports transfer, e.g. DMA queue.
        // If not, fallback to a dedicated compute queue.
        // Finally, fallback to same queue as compute.
        if (!FindVacantQueue(ref _copyQueueFamily, ref copyQueueIndex, queueFamiliesCount,
            VkQueueFlags.Transfer, VkQueueFlags.Graphics | VkQueueFlags.Compute, 0.5f) &&
            !FindVacantQueue(ref _copyQueueFamily, ref copyQueueIndex, queueFamiliesCount,
            VkQueueFlags.Compute, VkQueueFlags.Graphics, 0.5f))
        {
            _copyQueueFamily = _computeQueueFamily;
            copyQueueIndex = computeQueueIndex;
        }

        uint queueCreateInfoCount = 0;
        VkDeviceQueueCreateInfo* queueInfos = stackalloc VkDeviceQueueCreateInfo[queueFamiliesCount];

        for (int familyIndex = 0; familyIndex < queueFamiliesCount; familyIndex++)
        {
            if (queueOffsets[familyIndex] == 0)
                continue;

            VkDeviceQueueCreateInfo info = new()
            {
                sType = VkStructureType.DeviceQueueCreateInfo,
                queueFamilyIndex = (uint)familyIndex,
                queueCount = queueOffsets[familyIndex],
                pQueuePriorities = queuePriorities[familyIndex].Data
            };
            queueInfos[queueCreateInfoCount++] = info;
        }

        using var deviceExtensionNames = new VkStringArray(enabledExtensions);

        var deviceCreateInfo = new VkDeviceCreateInfo
        {
            sType = VkStructureType.DeviceCreateInfo,
            pNext = &features2,
            queueCreateInfoCount = queueCreateInfoCount,
            pQueueCreateInfos = queueInfos,
            enabledExtensionCount = deviceExtensionNames.Length,
            ppEnabledExtensionNames = deviceExtensionNames,
            pEnabledFeatures = null,
        };

        VkResult result = vkCreateDevice(PhysicalDevice, &deviceCreateInfo, null, out _handle);
        if (result != VkResult.Success)
        {
            throw new Exception($"Failed to create Vulkan Logical Device, {result}");
        }

        vkLoadDevice(_handle);

        vkGetDeviceQueue(_handle, _graphicsQueueFamily, graphicsQueueIndex, out _graphicsQueue);
        vkGetDeviceQueue(_handle, _computeQueueFamily, computeQueueIndex, out _computeQueue);
        vkGetDeviceQueue(_handle, _copyQueueFamily, copyQueueIndex, out _copyQueue);

        GraphicsAdapterType adapterType = properties.deviceType switch
        {
            VkPhysicalDeviceType.IntegratedGpu => GraphicsAdapterType.Integrated,
            VkPhysicalDeviceType.DiscreteGpu => GraphicsAdapterType.Discrete,
            VkPhysicalDeviceType.Cpu => GraphicsAdapterType.CPU,
            _ => GraphicsAdapterType.Unknown,
        };

        _adapterInfo = new()
        {
            VendorId = (VendorId)properties.vendorID,
            DeviceId = properties2.properties.deviceID,
            Name = properties.GetDeviceName(),
            DriverDescription = Interop.GetString(properties_1_2.driverName),
            AdapterType = adapterType,
        };

        _features = new()
        {
            IndependentBlend = features2.features.independentBlend,
            ComputeShader = true,
            TessellationShader = features2.features.tessellationShader,
            MultiViewport = features2.features.multiViewport,
            IndexUInt32 = features2.features.fullDrawIndexUint32,
            MultiDrawIndirect = features2.features.multiDrawIndirect,
            FillModeNonSolid = features2.features.fillModeNonSolid,
            SamplerAnisotropy = features2.features.samplerAnisotropy,
            TextureCompressionETC2 = features2.features.textureCompressionETC2,
            TextureCompressionASTC_LDR = features2.features.textureCompressionASTC_LDR,
            TextureCompressionBC = true,
            TextureCubeArray = true,
            Raytracing = false
        };

        _limits = new()
        {
            MaxVertexAttributes = properties2.properties.limits.maxVertexInputAttributes,
            MaxVertexBindings = properties2.properties.limits.maxVertexInputBindings,
            MaxVertexAttributeOffset = properties2.properties.limits.maxVertexInputAttributeOffset,
            MaxVertexBindingStride = properties2.properties.limits.maxVertexInputBindingStride,
            MaxTextureDimension1D = properties2.properties.limits.maxImageDimension1D,
            MaxTextureDimension2D = properties2.properties.limits.maxImageDimension2D,
            MaxTextureDimension3D = properties2.properties.limits.maxImageDimension3D,
            MaxTextureDimensionCube = properties2.properties.limits.maxImageDimensionCube,
            MaxTextureArrayLayers = properties2.properties.limits.maxImageArrayLayers,
            MaxColorAttachments = properties2.properties.limits.maxColorAttachments,
            MaxUniformBufferRange = properties2.properties.limits.maxUniformBufferRange,
            MaxStorageBufferRange = properties2.properties.limits.maxStorageBufferRange,
            MinUniformBufferOffsetAlignment = properties2.properties.limits.minUniformBufferOffsetAlignment,
            MinStorageBufferOffsetAlignment = properties2.properties.limits.minStorageBufferOffsetAlignment,
            MaxSamplerAnisotropy = (uint)properties2.properties.limits.maxSamplerAnisotropy,
            MaxViewports = properties2.properties.limits.maxViewports,
            MaxViewportWidth = properties2.properties.limits.maxViewportDimensions[0],
            MaxViewportHeight = properties2.properties.limits.maxViewportDimensions[1],
            MaxTessellationPatchSize = properties2.properties.limits.maxTessellationPatchSize,
            MaxComputeWorkGroupStorageSize = properties2.properties.limits.maxComputeSharedMemorySize,
            MaxComputeInvocationsPerWorkGroup = properties2.properties.limits.maxComputeWorkGroupInvocations,
            MaxComputeWorkGroupSizeX = properties2.properties.limits.maxComputeWorkGroupSize[0],
            MaxComputeWorkGroupSizeY = properties2.properties.limits.maxComputeWorkGroupSize[1],
            MaxComputeWorkGroupSizeZ = properties2.properties.limits.maxComputeWorkGroupSize[2],
            MaxComputeWorkGroupsPerDimension = Min(
                Min(
                    properties2.properties.limits.maxComputeWorkGroupCount[0],
                    properties2.properties.limits.maxComputeWorkGroupCount[1]),
                properties2.properties.limits.maxComputeWorkGroupCount[2]
            ),
        };
    }

    public VkPhysicalDevice PhysicalDevice { get; }

    public VkDevice NativeDevice => _handle;

    /// <inheritdoc />
    public override BackendType BackendType => BackendType.Vulkan;

    /// <inheritdoc />
    public override GraphicsAdapterInfo AdapterInfo => _adapterInfo;

    /// <inheritdoc />
    public override GraphicsDeviceFeatures Features => _features;

    /// <inheritdoc />
    public override GraphicsDeviceLimits Limits => _limits;

    /// <inheritdoc />
    protected override void OnDispose()
    {
        WaitIdle();
    }

    /// <inheritdoc />
    public override void WaitIdle()
    {
        //vkDeviceWaitIdle(NativeDevice).CheckResult(); 
    }

    /// <inheritdoc />
    protected override Buffer CreateBufferCore(in BufferDescriptor descriptor, IntPtr initialData) => new VulkanBuffer(this, descriptor, initialData);

    /// <inheritdoc />
    protected override Texture CreateTextureCore(in TextureDescriptor descriptor) => new VulkanTexture(this, descriptor);
}
