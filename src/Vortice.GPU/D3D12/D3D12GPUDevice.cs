// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Diagnostics;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DXGI;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.GPU.D3D12;

internal unsafe class D3D12GPUDevice : GPUDevice
{
    private readonly GPUDeviceInfo _info;
    private readonly GPUAdapterInfo _adapterInfo;

    public D3D12GPUDevice(IDXGIAdapter1 adapter)
    {
        Guard.IsNotNull(adapter, nameof(adapter));

        if (ValidationMode != ValidationMode.Disabled)
        {
            if (D3D12GetDebugInterface(out ID3D12Debug? debugController).Success)
            {
                debugController!.EnableDebugLayer();

                if (ValidationMode == ValidationMode.GPU)
                {
                    ID3D12Debug1? debugController1 = debugController.QueryInterfaceOrNull<ID3D12Debug1>();
                    if (debugController1 != null)
                    {
                        debugController1.SetEnableGPUBasedValidation(true);
                        debugController1.SetEnableSynchronizedCommandQueueValidation(true);
                        debugController1.Dispose();
                    }

                    ID3D12Debug2? debugController2 = debugController.QueryInterfaceOrNull<ID3D12Debug2>();
                    if (debugController1 != null)
                    {
                        debugController2.SetGPUBasedValidationFlags(GpuBasedValidationFlags.None);
                        debugController2.Dispose();
                    }
                }

                debugController.Dispose();
            }
            else
            {
                //Log.Debug("WARNING: Direct3D Debug Device is not available\n");
            }
        }


        Adapter = adapter;
        NativeDevice = D3D12CreateDevice<ID3D12Device2>(adapter, FeatureLevel.Level_11_0);
        NativeDevice.Name = "Vortice.GPU";

        // Configure debug device (if active).
        if (ValidationMode != ValidationMode.Disabled)
        {
            ID3D12InfoQueue? d3d12InfoQueue = NativeDevice.QueryInterfaceOrNull<ID3D12InfoQueue>();
            if (d3d12InfoQueue != null)
            {
                d3d12InfoQueue.SetBreakOnSeverity(MessageSeverity.Corruption, true);
                d3d12InfoQueue.SetBreakOnSeverity(MessageSeverity.Error, true);

                MessageId[] hide = new MessageId[]
                {
                    MessageId.ClearRenderTargetViewMismatchingClearValue,
                    MessageId.ClearDepthStencilViewMismatchingClearValue,
                    MessageId.MapInvalidNullRange,
                    MessageId.UnmapInvalidNullRange,
                    MessageId.ExecuteCommandListsWrongSwapChainBufferReference,
                    MessageId.ResourceBarrierMismatchingCommandListType
                };

                InfoQueueFilter filter = new()
                {
                    DenyList = new InfoQueueFilterDescription()
                    {
                        Ids = hide
                    }
                };

                d3d12InfoQueue.AddStorageFilterEntries(filter);

                // Break on DEVICE_REMOVAL_PROCESS_AT_FAULT
                d3d12InfoQueue.SetBreakOnID(MessageId.DeviceRemovalProcessAtFault, true);
                d3d12InfoQueue.Dispose();
            }
        }

        // Init capabilites.
        AdapterDescription1 adapterDesc = adapter.Description1;
        FeatureDataArchitecture1 featureDataArchitecture = NativeDevice.Architecture1;
        FeatureDataD3D12Options1 featureDataOptions1 = NativeDevice.Options1;
        FeatureDataD3D12Options5 featureDataOptions5 = NativeDevice.Options5;

        // Init capabilites.
        GPUAdapterType adapterType;
        if ((adapterDesc.Flags & AdapterFlags.Software) != 0)
        {
            adapterType = GPUAdapterType.CPU;
        }
        else
        {
            adapterType = featureDataArchitecture.Uma ? GPUAdapterType.IntegratedGPU : GPUAdapterType.DiscreteGPU;
            IsCacheCoherentUMA = featureDataArchitecture.CacheCoherentUMA;
        }

        SupportsRenderPass = false;
        if (featureDataOptions5.RenderPassesTier > RenderPassTier.Tier0
            && adapterDesc.VendorId != (int)VendorId.Intel)
        {
            SupportsRenderPass = true;
        }

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
                Raytracing = featureDataOptions5.RaytracingTier >= RaytracingTier.Tier1_0
            },
            Limits = new()
            {
                MaxVertexAttributes = 16,
                MaxVertexBindings = 16,
                MaxVertexAttributeOffset = 2047,
                MaxVertexBindingStride = 2048,
                MaxTextureDimension1D = RequestTexture1DUDimension,
                MaxTextureDimension2D = RequestTexture2DUOrVDimension,
                MaxTextureDimension3D = RequestTexture3DUVOrWDimension,
                MaxTextureDimensionCube = RequestTextureCubeDimension,
                MaxTextureArrayLayers = RequestTexture2DArrayAxisDimension,
                MaxColorAttachments = SimultaneousRenderTargetCount,
                MaxUniformBufferRange = RequestConstantBufferElementCount * 16,
                MaxStorageBufferRange = uint.MaxValue,
                MinUniformBufferOffsetAlignment = ConstantBufferDataPlacementAlignment,
                MinStorageBufferOffsetAlignment = 16u,
                MaxSamplerAnisotropy = MaxMaxAnisotropy,
                MaxViewports = ViewportAndScissorRectObjectCountPerPipeline,
                MaxViewportWidth = ViewportBoundsMax,
                MaxViewportHeight = ViewportBoundsMax,
                MaxTessellationPatchSize = InputAssemblerPatchMaxControlPointCount,
                MaxComputeSharedMemorySize = ComputeShaderThreadLocalTempRegisterPool,
                MaxComputeWorkGroupCountX = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                MaxComputeWorkGroupCountY = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                MaxComputeWorkGroupCountZ = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                MaxComputeWorkGroupInvocations = ComputeShaderThreadGroupMaxThreadsPerGroup,
                MaxComputeWorkGroupSizeX = ComputeShaderThreadGroupMaxX,
                MaxComputeWorkGroupSizeY = ComputeShaderThreadGroupMaxY,
                MaxComputeWorkGroupSizeZ = ComputeShaderThreadGroupMaxZ,
            }
        };

        _adapterInfo = new()
        {
            AdapterName = adapterDesc.Description,
            AdapterType = adapterType,
            Vendor = (VendorId)adapterDesc.VendorId,
            VendorId = (uint)adapterDesc.VendorId,
        };
    }

    public IDXGIAdapter1 Adapter { get; }

    public ID3D12Device2 NativeDevice { get; }

    /// <summary>
    /// Gets whether or not the current device has a cache coherent UMA architecture.
    /// </summary>
    public bool IsCacheCoherentUMA { get; }

    public bool SupportsRenderPass { get; }

    /// <inheritdoc />
    public override GPUDeviceInfo Info => _info;

    /// <inheritdoc />
    public override GPUAdapterInfo AdapterInfo => _adapterInfo;

    /// <inheritdoc />
    protected override void OnDispose()
    {
#if DEBUG
        uint refCount = NativeDevice.Release();
        if (refCount > 0)
        {
            System.Diagnostics.Debug.WriteLine($"Direct3D12: There are {refCount} unreleased references left on the device");

            ID3D12DebugDevice? d3d12DebugDevice = NativeDevice.QueryInterfaceOrNull<ID3D12DebugDevice>();
            if (d3d12DebugDevice != null)
            {
                d3d12DebugDevice.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail | ReportLiveDeviceObjectFlags.IgnoreInternal);
                d3d12DebugDevice.Dispose();
            }
        }
#else
        NativeDevice.Dispose();
#endif

        Adapter.Dispose();
    }

    /// <inheritdoc />
    public override void WaitIdle()
    {
    }

    /// <inheritdoc />
    protected override Texture CreateTextureCore(in TextureDescriptor descriptor) => new D3D12Texture(this, descriptor);
}
