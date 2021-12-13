// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.GPU.Vulkan;

public class VulkanGPUDevice : GPUDevice
{
    private readonly GPUDeviceInfo _info;
    private readonly GPUAdapterInfo _adapterInfo;

    public VulkanGPUDevice(VkPhysicalDevice physicalDevice)
    {
        PhysicalDevice = physicalDevice;

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

        _adapterInfo = new()
        {
        };
    }


    public VkPhysicalDevice PhysicalDevice { get; }

    public VkDevice NativeDevice { get; }

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
        vkDeviceWaitIdle(NativeDevice).CheckResult(); 
    }
}
