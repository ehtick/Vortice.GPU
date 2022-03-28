// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;
using static Vortice.Graphics.D3DUtils;

namespace Vortice.Graphics.D3D11;

internal static class D3D11Factory
{
    private static readonly Lazy<bool> s_isSupported = new(CheckIsSupported);

    public static bool IsSupported() => s_isSupported.Value;

    private static bool CheckIsSupported()
    {
        if (!PlatformInfo.IsWindows)
        {
            return false;
        }

        if (CreateDXGIFactory2(false, out IDXGIFactory2? dxgiFactory).Failure)
        {
            return false;
        }

        bool foundCompatibleDevice = false;
        for (int adapterIndex = 0; dxgiFactory!.EnumAdapters1(adapterIndex, out IDXGIAdapter1 adapter).Success; adapterIndex++)
        {
            AdapterDescription1 desc = adapter.Description1;

            // Don't select the Basic Render Driver adapter.
            if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
            {
                adapter.Dispose();

                continue;
            }

            if (IsSupportedFeatureLevel(adapter, FeatureLevel.Level_11_0, DeviceCreationFlags.BgraSupport))
            {
                foundCompatibleDevice = true;
                break;
            }
        }

        return foundCompatibleDevice;
    }

    public static D3D11GraphicsDevice Create(in GraphicsDeviceDescriptor descriptor)
    {
        using (IDXGIFactory2 factory = CreateDXGIFactory2<IDXGIFactory2>(descriptor.ValidationMode != ValidationMode.Disabled))
        {
            IDXGIAdapter1? adapter = default;

            IDXGIFactory6? dxgiFactory6 = factory.QueryInterfaceOrNull<IDXGIFactory6>();

            if (dxgiFactory6 != null)
            {
                GpuPreference gpuPreference = ToDXGI(descriptor.PowerPreference);

                for (int adapterIndex = 0; dxgiFactory6!.EnumAdapterByGpuPreference(adapterIndex, gpuPreference, out adapter).Success; adapterIndex++)
                {
                    AdapterDescription1 desc = adapter!.Description1;

                    // Don't select the Basic Render Driver adapter.
                    if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                    {
                        adapter.Dispose();

                        continue;
                    }

                    if (IsSupportedFeatureLevel(adapter, FeatureLevel.Level_11_0, DeviceCreationFlags.BgraSupport))
                    {
                        break;
                    }
                }

                dxgiFactory6.Dispose();
            }

            if (adapter == null)
            {
                for (int adapterIndex = 0; factory.EnumAdapters1(adapterIndex, out adapter).Success; adapterIndex++)
                {
                    AdapterDescription1 desc = adapter.Description1;

                    // Don't select the Basic Render Driver adapter.
                    if ((desc.Flags & AdapterFlags.Software) != AdapterFlags.None)
                    {
                        adapter.Dispose();

                        continue;
                    }

                    if (IsSupportedFeatureLevel(adapter, FeatureLevel.Level_11_0, DeviceCreationFlags.BgraSupport))
                    {
                        break;
                    }
                }
            }

            if (adapter == null)
            {
                throw new GraphicsException("No Direct3D 11 device found");
            }

            return new D3D11GraphicsDevice(adapter, descriptor);
        }
    }
}
