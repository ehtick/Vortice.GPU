// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Toolkit.Diagnostics;
using Vortice.Direct3D;
using static Vortice.GPU.D3DUtils;
using static Vortice.Direct3D11.D3D11;
using Vortice.DXGI;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Debug;

namespace Vortice.GPU.D3D11;

internal class D3D11GPUDevice : GPUDevice
{
    private static readonly FeatureLevel[] s_featureLevels = new[]
    {
        FeatureLevel.Level_11_1,
        FeatureLevel.Level_11_0,
        FeatureLevel.Level_10_1,
        FeatureLevel.Level_10_0
    };

    public D3D11GPUDevice(IDXGIAdapter1 adapter)
    {
        Guard.IsNotNull(adapter, nameof(adapter));

        DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport;

        if (ValidationMode != ValidationMode.Disabled)
        {
            if (SdkLayersAvailable())
            {
                creationFlags |= DeviceCreationFlags.Debug;
            }
        }

        Adapter = adapter;

        {
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
        }

        if (ValidationMode != ValidationMode.Disabled)
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
        }
    }

    public IDXGIAdapter1 Adapter { get; }
    public ID3D11Device1 NativeDevice { get; }
    public ID3D11DeviceContext1 ImmediateContext { get; }
    public FeatureLevel FeatureLevel { get; }

    /// <inheritdoc />
    protected override void OnDispose()
    {
        Adapter.Dispose();
    }
}
