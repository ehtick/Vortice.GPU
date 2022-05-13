// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Direct3D12;
using static Vortice.Direct3D12.D3D12;
using static Vortice.Graphics.D3DUtils;
using static Vortice.Graphics.D3D12.D3D12Utils;

namespace Vortice.Graphics.D3D12;

internal class D3D12Texture : Texture
{
    public D3D12Texture(D3D12GraphicsDevice device, in TextureDescriptor descriptor)
        : base(device, descriptor)
    {
        ResourceDescription resourceDesc = new ResourceDescription();
        resourceDesc.Dimension = ToD3D12(descriptor.Dimension);
        resourceDesc.Alignment = 0;
        resourceDesc.Width = (ulong)descriptor.Width;
        resourceDesc.Height = descriptor.Height;
        resourceDesc.DepthOrArraySize = (ushort)descriptor.DepthOrArraySize;
        resourceDesc.MipLevels = (ushort)descriptor.MipLevels;
        resourceDesc.Format = ToDXGIFormat(descriptor.Format);
        resourceDesc.SampleDescription = new(ToSampleCount(descriptor.SampleCount), 0);
        resourceDesc.Layout = TextureLayout.Unknown;
        resourceDesc.Flags = ResourceFlags.None;

        if (descriptor.Usage.HasFlag(TextureUsage.RenderTarget))
        {
            if (TextureFormatUtils.IsDepthStencilFormat(descriptor.Format))
            {
                resourceDesc.Flags |= ResourceFlags.AllowDepthStencil;
                if (!descriptor.Usage.HasFlag(TextureUsage.ShaderRead))
                {
                    resourceDesc.Flags |= ResourceFlags.DenyShaderResource;
                }
            }
            else
            {
                resourceDesc.Flags |= ResourceFlags.AllowRenderTarget;
            }
        }

        if (descriptor.Usage.HasFlag(TextureUsage.ShaderWrite))
        {
            resourceDesc.Flags |= ResourceFlags.AllowUnorderedAccess;
        }

        ClearValue? paramClearValue = default;
        if (descriptor.Usage.HasFlag(TextureUsage.RenderTarget))
        {
            ClearValue clearValue = default;
            clearValue.Format = resourceDesc.Format;

            if (TextureFormatUtils.IsDepthStencilFormat(descriptor.Format))
            {
                clearValue.DepthStencil.Depth = 1.0f;
            }

            if (TextureFormatUtils.IsDepthStencilFormat(descriptor.Format) &&
                descriptor.Usage.HasFlag(TextureUsage.ShaderRead | TextureUsage.ShaderWrite))
            {
                resourceDesc.Format = GetTypelessFormatFromDepthFormat(descriptor.Format);
            }
            else
            {
                paramClearValue = clearValue;
            }
        }

        ResourceStates resourceState = ResourceStates.PixelShaderResource | ResourceStates.NonPixelShaderResource; // ConvertResourceStates(info.initialState);

        //if (initialData != nullptr)
        //{
        //    resourceState = D3D12_RESOURCE_STATE_COMMON;
        //}

        Handle = device.NativeDevice.CreateCommittedResource(
            HeapProperties.DefaultHeapProperties,
            HeapFlags.None,
            resourceDesc,
            resourceState,
            paramClearValue);
    }

    public ID3D12Resource Handle { get; }

    protected override void OnLabelChanged()
    {
        Handle.Name = _label ?? string.Empty;
    }
}
