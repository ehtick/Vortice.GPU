// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Direct3D11;
using Vortice.DXGI;
using static Vortice.Graphics.D3DUtils;

namespace Vortice.Graphics.D3D11;

internal class D3D11Texture : Texture
{
    public D3D11Texture(D3D11GraphicsDevice device, in TextureDescriptor descriptor)
        : base(device, descriptor)
    {
        ResourceUsage usage = descriptor.Access == CpuAccess.None ? ResourceUsage.Default : ResourceUsage.Staging;
        BindFlags bindFlags = BindFlags.None;
        CpuAccessFlags cpuAccessFlags = 0u;
        Format format = ToDXGIFormat(descriptor.Format);
        ResourceOptionFlags miscFlags = ResourceOptionFlags.None;

        if (descriptor.Access == CpuAccess.Read)
            cpuAccessFlags = CpuAccessFlags.Read;
        else if (descriptor.Access == CpuAccess.Write)
            cpuAccessFlags = CpuAccessFlags.Write;

        if (descriptor.Usage.HasFlag(TextureUsage.ShaderRead))
        {
            bindFlags |= BindFlags.ShaderResource;
        }

        if (descriptor.Usage.HasFlag(TextureUsage.ShaderWrite))
        {
            bindFlags |= BindFlags.UnorderedAccess;
        }

        if (descriptor.Usage.HasFlag(TextureUsage.RenderTarget))
        {
            if (TextureFormatUtils.IsDepthStencilFormat(descriptor.Format))
            {
                bindFlags |= BindFlags.DepthStencil;
            }
            else
            {
                bindFlags |= BindFlags.RenderTarget;
            }
        }

        if (descriptor.SharedResourceFlags.HasFlag(SharedResourceFlags.Shared_NTHandle))
        {
            miscFlags |= ResourceOptionFlags.SharedKeyedMutex | ResourceOptionFlags.SharedNTHandle;
        }
        else if (descriptor.SharedResourceFlags.HasFlag(SharedResourceFlags.Shared))
        {
            miscFlags |= ResourceOptionFlags.Shared;
        }

        // If ShaderRead and depth format, set to typeless.
        if (TextureFormatUtils.IsDepthStencilFormat(descriptor.Format) &&
            descriptor.Usage.HasFlag(TextureUsage.ShaderRead | TextureUsage.ShaderWrite))
        {
            format = GetTypelessFormatFromDepthFormat(descriptor.Format);
        }

        switch (descriptor.Dimension)
        {
            case TextureDimension.Texture1D:
                Texture1DDescription d3d11Desc1D = new()
                {
                    Width = Width,
                    MipLevels = MipLevels,
                    ArraySize = ArraySize,
                    Format = format,
                    Usage = usage,
                    BindFlags = bindFlags,
                    CPUAccessFlags = cpuAccessFlags,
                    MiscFlags = miscFlags,
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
                    Format = format,
                    SampleDescription = new SampleDescription(ToSampleCount(descriptor.SampleCount), 0),
                    Usage = usage,
                    BindFlags = bindFlags,
                    CPUAccessFlags = cpuAccessFlags,
                    MiscFlags = miscFlags
                };

                if (ArraySize >= 6 &&
                    Width == Height &&
                    MipLevels == 1)
                {
                    d3d11Desc2D.MiscFlags |= ResourceOptionFlags.TextureCube;
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
                    Format = format,
                    Usage = usage,
                    BindFlags = bindFlags,
                    CPUAccessFlags = cpuAccessFlags,
                    MiscFlags = miscFlags
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
