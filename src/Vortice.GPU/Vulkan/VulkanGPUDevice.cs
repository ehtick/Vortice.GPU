// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.GPU.Vulkan;

internal unsafe class VulkanGPUDevice : GPUDevice
{
    private readonly VkDevice _handle;
    private readonly GPUDeviceInfo _info;
    private readonly GPUAdapterInfo _adapterInfo;

    private readonly VkPhysicalDeviceProperties2 properties2;
    private readonly VkPhysicalDeviceVulkan11Properties properties_1_1;
    private readonly VkPhysicalDeviceVulkan12Properties properties_1_2;
    private readonly VkPhysicalDeviceSamplerFilterMinmaxProperties sampler_minmax_properties;
    private readonly VkPhysicalDeviceAccelerationStructurePropertiesKHR acceleration_structure_properties;
    private readonly VkPhysicalDeviceRayTracingPipelinePropertiesKHR raytracing_properties;
    private readonly VkPhysicalDeviceFragmentShadingRatePropertiesKHR fragment_shading_rate_properties;
    private readonly VkPhysicalDeviceMeshShaderPropertiesNV mesh_shader_properties;

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
    private readonly VkPhysicalDeviceDynamicRenderingFeaturesKHR dynamicRenderingFeaturesKHR;

    public VulkanGPUDevice(VkPhysicalDevice physicalDevice)
        : base(GPUBackend.Vulkan)
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
        void** features_chain = &features_1_2.pNext;
        acceleration_structure_features = default;
        raytracing_features = default;
        raytracing_query_features = default;
        fragment_shading_rate_features = default;
        mesh_shader_features = default;

        //properties2.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_PROPERTIES_2;
        //properties_1_1.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_1_PROPERTIES;
        //properties_1_2.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_VULKAN_1_2_PROPERTIES;
        //properties2.pNext = &properties_1_1;
        //properties_1_1.pNext = &properties_1_2;
        //void** properties_chain = &properties_1_2.pNext;
        //sampler_minmax_properties = default;
        //acceleration_structure_properties = default;
        //raytracing_properties = default;
        //fragment_shading_rate_properties = default;
        //mesh_shader_properties = default;
        //
        //sampler_minmax_properties.sType = VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_SAMPLER_FILTER_MINMAX_PROPERTIES;
        //*properties_chain = &sampler_minmax_properties;
        //properties_chain = &sampler_minmax_properties.pNext;

        List<string> enabledExtensions = new();
        enabledExtensions.Add(VK_KHR_SWAPCHAIN_EXTENSION_NAME);

        vkGetPhysicalDeviceFeatures2(PhysicalDevice, out features2);

        uint queueFamiliesCount = 0u;
        vkGetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamiliesCount, null);
        VkQueueFamilyProperties* queueFamilies = stackalloc VkQueueFamilyProperties[(int)queueFamiliesCount];
        vkGetPhysicalDeviceQueueFamilyProperties(physicalDevice, &queueFamiliesCount, queueFamilies);

        List<uint> queueOffsets = new();
        List<List<float>> queuePriorities = new();

        uint graphicsQueueIndex = 0;
        uint computeQueueIndex = 0;
        uint copyQueueIndex = 0;


        bool FindVacantQueue(out uint family, uint index, VkQueueFlags required, VkQueueFlags ignore_flags, float priority)
        {
            for (int familyIndex = 0; familyIndex < queueFamiliesCount; familyIndex++)
            {
                if ((queueFamilies[familyIndex].queueFlags & ignore_flags) != 0)
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

            family = QueueFamilyIgnored;
            return false;
        }

        float priority = 1.0f;
        VkDeviceQueueCreateInfo queueCreateInfo = new VkDeviceQueueCreateInfo
        {
            sType = VkStructureType.DeviceQueueCreateInfo,
            //queueFamilyIndex = queueFamilies.graphicsFamily,
            //queueCount = 1,
            pQueuePriorities = &priority
        };

        using var deviceExtensionNames = new VkStringArray(enabledExtensions);

        var deviceCreateInfo = new VkDeviceCreateInfo
        {
            sType = VkStructureType.DeviceCreateInfo,
            pNext = &features2,
            queueCreateInfoCount = 1,
            pQueueCreateInfos = &queueCreateInfo,
            enabledExtensionCount = deviceExtensionNames.Length,
            ppEnabledExtensionNames = deviceExtensionNames,
            pEnabledFeatures = null,
        };

        VkResult result = vkCreateDevice(PhysicalDevice, &deviceCreateInfo, null, out _handle);
        if (result != VkResult.Success)
            throw new Exception($"Failed to create Vulkan Logical Device, {result}");

        _info = new()
        {
            Features = new()
            {
                IndependentBlend = true,
                ComputeShader = true,
                TessellationShader = true,
                MultiViewport = true,
                IndexUInt32 = true,
                MultiDrawIndirect = true,
                FillModeNonSolid = true,
                SamplerAnisotropy = true,
                TextureCompressionETC2 = false,
                TextureCompressionASTC_LDR = false,
                TextureCompressionBC = true,
                TextureCubeArray = true,
                Raytracing = false
            },
            Limits = new()
            {
                MaxVertexAttributes = 16,
                MaxVertexBindings = 16,
                MaxVertexAttributeOffset = 2047,
                MaxVertexBindingStride = 2048,
                //MaxTextureDimension1D = RequestTexture1DUDimension,
                //MaxTextureDimension2D = RequestTexture2DUOrVDimension,
                //MaxTextureDimension3D = RequestTexture3DUVOrWDimension,
                //MaxTextureDimensionCube = RequestTextureCubeDimension,
                //MaxTextureArrayLayers = RequestTexture2DArrayAxisDimension,
                //MaxColorAttachments = SimultaneousRenderTargetCount,
                //MaxUniformBufferRange = RequestConstantBufferElementCount * 16,
                //MaxStorageBufferRange = uint.MaxValue,
                //MinUniformBufferOffsetAlignment = ConstantBufferDataPlacementAlignment,
                //MinStorageBufferOffsetAlignment = 16u,
                //MaxSamplerAnisotropy = MaxMaxAnisotropy,
                //MaxViewports = ViewportAndScissorRectObjectCountPerPipeline,
                //MaxViewportWidth = ViewportBoundsMax,
                //MaxViewportHeight = ViewportBoundsMax,
                //MaxTessellationPatchSize = InputAssemblerPatchMaxControlPointCount,
                //MaxComputeSharedMemorySize = ComputeShaderThreadLocalTempRegisterPool,
                //MaxComputeWorkGroupCountX = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                //MaxComputeWorkGroupCountY = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                //MaxComputeWorkGroupCountZ = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                //MaxComputeWorkGroupInvocations = ComputeShaderThreadGroupMaxThreadsPerGroup,
                //MaxComputeWorkGroupSizeX = ComputeShaderThreadGroupMaxX,
                //MaxComputeWorkGroupSizeY = ComputeShaderThreadGroupMaxY,
                //MaxComputeWorkGroupSizeZ = ComputeShaderThreadGroupMaxZ,
            }
        };

        GPUAdapterType adapterType = GPUAdapterType.Unknown;
        switch (properties.deviceType)
        {
            case VkPhysicalDeviceType.IntegratedGpu:
                adapterType = GPUAdapterType.IntegratedGPU;
                break;

            case VkPhysicalDeviceType.DiscreteGpu:
                adapterType = GPUAdapterType.DiscreteGPU;
                break;
            case VkPhysicalDeviceType.Cpu:
                adapterType = GPUAdapterType.CPU;
                break;

            default:
                adapterType = GPUAdapterType.Unknown;
                break;
        }

        _adapterInfo = new()
        {
            AdapterName = properties.GetDeviceName(),
            AdapterType = adapterType,
            Vendor = (VendorId)properties.vendorID,
            VendorId = properties.vendorID,
        };
    }

    public VkPhysicalDevice PhysicalDevice { get; }

    public VkDevice NativeDevice => _handle;

    /// <inheritdoc />
    public override GPUDeviceInfo Info => _info;

    /// <inheritdoc />
    public override GPUAdapterInfo AdapterInfo => _adapterInfo;

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
    protected override Texture CreateTextureCore(in TextureDescriptor descriptor) => new VulkanTexture(this, descriptor);
}
