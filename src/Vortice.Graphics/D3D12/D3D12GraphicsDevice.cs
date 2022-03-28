// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Microsoft.Toolkit.Diagnostics;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;
using Vortice.DXGI;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.Graphics.D3D12;

internal sealed class D3D12GraphicsDevice : GraphicsDevice
{
    private readonly GraphicsAdapterInfo _adapterInfo;
    private readonly GraphicsDeviceFeatures _features;
    private readonly GraphicsDeviceLimits _limits;

    public D3D12GraphicsDevice(IDXGIAdapter1 adapter, in GraphicsDeviceDescriptor descriptor)
    {
        Guard.IsNotNull(adapter, nameof(adapter));

        if (descriptor.ValidationMode != ValidationMode.Disabled)
        {
            if (D3D12GetDebugInterface(out ID3D12Debug? debugController).Success)
            {
                debugController!.EnableDebugLayer();

                if (descriptor.ValidationMode == ValidationMode.GPU)
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
        if (descriptor.ValidationMode != ValidationMode.Disabled)
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
        GraphicsAdapterType adapterType = GraphicsAdapterType.Unknown;
        if ((adapterDesc.Flags & AdapterFlags.Software) != 0)
        {
            adapterType = GraphicsAdapterType.CPU;
        }
        else
        {
            adapterType = featureDataArchitecture.Uma ? GraphicsAdapterType.Integrated : GraphicsAdapterType.Discrete;
            IsCacheCoherentUMA = featureDataArchitecture.CacheCoherentUMA;
        }

        // Convert the adapter's D3D12 driver version to a readable string like "24.21.13.9793".
        string driverDescription = string.Empty;
        if (adapter.CheckInterfaceSupport<IDXGIDevice>(out long umdVersion))
        {
            driverDescription = "D3D12 driver version ";

            for (int i = 0; i < 4; ++i)
            {
                ushort driverVersion = (ushort)((umdVersion >> (48 - 16 * i)) & 0xFFFF);
                driverDescription += driverVersion + ".";
            }
        }


        SupportsRenderPass = false;
        if (featureDataOptions5.RenderPassesTier > RenderPassTier.Tier0
            && adapterDesc.VendorId != (int)VendorId.Intel)
        {
            SupportsRenderPass = true;
        }

        _adapterInfo = new()
        {
            VendorId = (VendorId)adapterDesc.VendorId,
            DeviceId = (uint)adapterDesc.DeviceId,
            Name = adapterDesc.Description,
            DriverDescription = driverDescription,
            AdapterType = adapterType,
        };

        _features = new()
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
        };

        _limits = new()
        {
            MaxVertexAttributes = InputAssemblerVertexInputResourceSlotCount,
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
            MaxComputeWorkGroupStorageSize = ComputeShaderThreadLocalTempRegisterPool,
            MaxComputeInvocationsPerWorkGroup = ComputeShaderThreadGroupMaxThreadsPerGroup,
            MaxComputeWorkGroupSizeX = ComputeShaderThreadGroupMaxX,
            MaxComputeWorkGroupSizeY = ComputeShaderThreadGroupMaxY,
            MaxComputeWorkGroupSizeZ = ComputeShaderThreadGroupMaxZ,
            MaxComputeWorkGroupsPerDimension = ComputeShaderDispatchMaxThreadGroupsPerDimension,
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
    public override BackendType BackendType => BackendType.D3D12;

    /// <inheritdoc />
    public override GraphicsAdapterInfo AdapterInfo => _adapterInfo;

    /// <inheritdoc />
    public override GraphicsDeviceFeatures Features => _features;

    /// <inheritdoc />
    public override GraphicsDeviceLimits Limits => _limits;

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
    protected override Buffer CreateBufferCore(in BufferDescriptor descriptor, IntPtr initialData) => new D3D12Buffer(this, descriptor, initialData);

    /// <inheritdoc />
    protected override Texture CreateTextureCore(in TextureDescriptor descriptor) => new D3D12Texture(this, descriptor);
}
