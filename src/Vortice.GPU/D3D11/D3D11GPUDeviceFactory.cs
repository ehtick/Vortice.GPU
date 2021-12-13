// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using static Vortice.Direct3D11.D3D11;
using static Vortice.DXGI.DXGI;

namespace Vortice.GPU.D3D11;

internal static class D3D11GPUDeviceFactory
{
    public static readonly Lazy<bool> IsSupported = new(CheckIsSupported);
    private static IDXGIFactory2? _dxgiFactory;

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

    public static IDXGIFactory2 Factory { get => _dxgiFactory ??= CreateDXGIFactory(); set => _dxgiFactory = value; }

    private static IDXGIFactory2 CreateDXGIFactory()
    {
#if DEBUG
        return CreateDXGIFactory2<IDXGIFactory4>(true);
#else
        return CreateDXGIFactory2<IDXGIFactory4>(false);
#endif
    }

    public static D3D11GPUDevice CreateDefault(GpuPreference gpuPreference = GpuPreference.HighPerformance)
    {
        IDXGIAdapter1? adapter = default;

        IDXGIFactory6? dxgiFactory6 = Factory.QueryInterfaceOrNull<IDXGIFactory6>();

        if (dxgiFactory6 != null)
        {
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
            for (int adapterIndex = 0; Factory.EnumAdapters1(adapterIndex, out adapter).Success; adapterIndex++)
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
            throw new GPUException("No Direct3D 11 device found");
        }

        return new D3D11.D3D11GPUDevice(adapter);
    }
}
