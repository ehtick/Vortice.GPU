// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Direct3D11;
using Vortice.DXGI;
using static Vortice.Graphics.D3DUtils;

namespace Vortice.Graphics.D3D11;

internal unsafe class D3D11Texture : Texture
{
    public D3D11Texture(D3D11GPUDevice device, in TextureDescriptor descriptor)
        : base(device, descriptor)
    {
        switch (descriptor.Dimension)
        {
            case TextureDimension.Texture1D:
                Texture1DDescription d3d11Desc1D = new()
                {
                    Width = Width,
                    MipLevels = MipLevels,
                    ArraySize = ArraySize,
                    Format = ToDXGIFormat(Format),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                };

                Handle = device.NativeDevice.CreateTexture1D(d3d11Desc1D);
                break;
            case TextureDimension.Texture2D:
                Texture2DDescription d3d11Desc2D = new()
                {
                    Width = Width,
                    Height = Height,
                    MipLevels = MipLevels,
                    ArraySize = ArraySize,
                    Format = ToDXGIFormat(Format),
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };

                if (ArraySize >= 6 && Width == Height)
                {
                    d3d11Desc2D.OptionFlags |= ResourceOptionFlags.TextureCube;
                }

                Handle = device.NativeDevice.CreateTexture2D(d3d11Desc2D);
                break;
            case TextureDimension.Texture3D:
                Texture3DDescription d3d11Desc3D = new()
                {
                    Width = Width,
                    Height = Height,
                    Depth = Depth,
                    MipLevels = MipLevels,
                    Format = ToDXGIFormat(Format),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };

                Handle = device.NativeDevice.CreateTexture3D(d3d11Desc3D);
                break;
            default:
                throw new GraphicsException($"Invalid texture dimension: {descriptor.Dimension}");
        }
    }

    public ID3D11Resource Handle { get; }

    protected override void OnLabelChanged()
    {
        Handle.DebugName = _label ?? string.Empty;
    }
}
