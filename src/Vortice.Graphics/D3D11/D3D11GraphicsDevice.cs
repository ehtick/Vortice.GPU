// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Microsoft.Toolkit.Diagnostics;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;
using Vortice.DXGI;
using static Vortice.Direct3D11.D3D11;

namespace Vortice.Graphics.D3D11;

internal sealed class D3D11GraphicsDevice : GraphicsDevice
{
    private static readonly FeatureLevel[] s_featureLevels = new[]
    {
        FeatureLevel.Level_11_1,
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0
    };

    private readonly GraphicsAdapterInfo _adapterInfo;
    private readonly GraphicsDeviceFeatures _features;
    private readonly GraphicsDeviceLimits _limits;

    public D3D11GraphicsDevice(IDXGIAdapter1 adapter, in GraphicsDeviceDescriptor descriptor)
    {
        Guard.IsNotNull(adapter, nameof(adapter));

        DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport;

        if (descriptor.ValidationMode != ValidationMode.Disabled)
        {
            if (SdkLayersAvailable())
            {
                creationFlags |= DeviceCreationFlags.Debug;
            }
        }

        Adapter = adapter;

        if (D3D11CreateDevice(
            adapter,
            DriverType.Unknown,
            creationFlags,
            s_featureLevels,
            out ID3D11Device tempDevice, out FeatureLevel featureLevel, out ID3D11DeviceContext tempContext).Failure)
        {
            // If the initialization fails, fall back to the WARP device.
            // For more information on WARP, see:
            // http://go.microsoft.com/fwlink/?LinkId=286690
            D3D11CreateDevice(
                IntPtr.Zero,
                DriverType.Warp,
                creationFlags,
                s_featureLevels,
                out tempDevice, out featureLevel, out tempContext).CheckError();
        }

        NativeDevice = tempDevice.QueryInterface<ID3D11Device1>();
        ImmediateContext = tempContext.QueryInterface<ID3D11DeviceContext1>();
        FeatureLevel = featureLevel;
        tempContext.Dispose();
        tempDevice.Dispose();

        NativeDevice.DebugName = "Vortice.GPU";

        if (descriptor.ValidationMode != ValidationMode.Disabled)
        {
            ID3D11Debug? d3d11Debug = NativeDevice.QueryInterfaceOrNull<ID3D11Debug>();
            if (d3d11Debug != null)
            {
                ID3D11InfoQueue? d3d11InfoQueue = d3d11Debug.QueryInterfaceOrNull<ID3D11InfoQueue>();
                if (d3d11InfoQueue != null)
                {
                    d3d11InfoQueue.SetBreakOnSeverity(MessageSeverity.Corruption, true);
                    d3d11InfoQueue.SetBreakOnSeverity(MessageSeverity.Error, true);

                    MessageId[] hide = new[]
                    {
                        MessageId.SetPrivateDataChangingParams,
                    };

                    InfoQueueFilter filter = new()
                    {
                        DenyList = new InfoQueueFilterDescription
                        {
                            Ids = hide
                        }
                    };
                    d3d11InfoQueue.AddStorageFilterEntries(filter);
                    d3d11InfoQueue.Dispose();
                }
                d3d11Debug.Dispose();
            }

            // Init capabilites.
            AdapterDescription1 adapterDesc = adapter.Description1;
            _adapterInfo = new()
            {
                VendorId = (VendorId)adapterDesc.VendorId,
                DeviceId = (uint)adapterDesc.DeviceId,
                Name = adapterDesc.Description,
                AdapterType = GraphicsAdapterType.Unknown,
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
                Raytracing = false
            };

            _limits = new()
            {
                MaxVertexAttributes = 16,
                MaxVertexBindings = 16,
                MaxVertexAttributeOffset = 2047,
                MaxVertexBindingStride = 2048,
                MaxTextureDimension1D = 16384,
                MaxTextureDimension2D = 16384,
                MaxTextureDimension3D = 2048,
                MaxTextureDimensionCube = 16384,
                MaxTextureArrayLayers = 2048,
                MaxColorAttachments = 8,
                //MaxUniformBufferRange = RequestConstantBufferElementCount * 16,
                //MaxStorageBufferRange = uint.MaxValue,
                //MinUniformBufferOffsetAlignment = ConstantBufferDataPlacementAlignment,
                //MinStorageBufferOffsetAlignment = 16u,
                //MaxSamplerAnisotropy = MaxMaxAnisotropy,
                MaxViewports = 16,
                MaxViewportWidth = 32767,
                MaxViewportHeight = 32767,
                MaxTessellationPatchSize = 32,
                //MaxComputeSharedMemorySize = ComputeShaderThreadLocalTempRegisterPool,
                //MaxComputeWorkGroupCountX = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                //MaxComputeWorkGroupCountY = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                //MaxComputeWorkGroupCountZ = ComputeShaderDispatchMaxThreadGroupsPerDimension,
                //MaxComputeWorkGroupInvocations = ComputeShaderThreadGroupMaxThreadsPerGroup,
                //MaxComputeWorkGroupSizeX = ComputeShaderThreadGroupMaxX,
                //MaxComputeWorkGroupSizeY = ComputeShaderThreadGroupMaxY,
                //MaxComputeWorkGroupSizeZ = ComputeShaderThreadGroupMaxZ,
            };
        }
    }

    public IDXGIAdapter1 Adapter { get; }
    public ID3D11Device1 NativeDevice { get; }
    public ID3D11DeviceContext1 ImmediateContext { get; }
    public FeatureLevel FeatureLevel { get; }

    /// <inheritdoc />
    public override BackendType BackendType => BackendType.D3D11;

    /// <inheritdoc />
    public override GraphicsAdapterInfo AdapterInfo => _adapterInfo;

    /// <inheritdoc />
    public override GraphicsDeviceFeatures Features => _features;

    /// <inheritdoc />
    public override GraphicsDeviceLimits Limits => _limits;

    /// <inheritdoc />
    protected override void OnDispose()
    {
        ImmediateContext.Flush();
        ImmediateContext.Dispose();

#if DEBUG
        uint refCount = NativeDevice.Release();
        if (refCount > 0)
        {
            System.Diagnostics.Debug.WriteLine($"Direct3D11: There are {refCount} unreleased references left on the device");

            ID3D11Debug? d3d11Debug = NativeDevice.QueryInterfaceOrNull<ID3D11Debug>();
            if (d3d11Debug != null)
            {
                d3d11Debug.ReportLiveDeviceObjects(ReportLiveDeviceObjectFlags.Detail | ReportLiveDeviceObjectFlags.IgnoreInternal);
                d3d11Debug.Dispose();
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
        ImmediateContext.Flush();
    }

    /// <inheritdoc />
    protected override Buffer CreateBufferCore(in BufferDescriptor descriptor, IntPtr initialData) => new D3D11Buffer(this, descriptor, initialData);

    /// <inheritdoc />
    protected override Texture CreateTextureCore(in TextureDescriptor descriptor) => new D3D11Texture(this, descriptor);
}
