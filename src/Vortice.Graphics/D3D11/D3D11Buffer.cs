// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Graphics.D3DUtils;

namespace Vortice.Graphics.D3D11;

internal class D3D11Buffer : Buffer
{
    public D3D11Buffer(D3D11GraphicsDevice device, in BufferDescriptor descriptor, IntPtr initialData)
        : base(device, descriptor)
    {
        BufferDescription d3d11Desc = new()
        {
            SizeInBytes = (int)descriptor.Size,
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None,
            StructureByteStride = 0
        };

        if (descriptor.Usage.HasFlag(BufferUsage.Uniform))
        {
            d3d11Desc.SizeInBytes = (int)MathHelper.AlignUp((uint)d3d11Desc.SizeInBytes, 64u);
            d3d11Desc.Usage = ResourceUsage.Dynamic;
            d3d11Desc.BindFlags = BindFlags.ConstantBuffer;
            d3d11Desc.CpuAccessFlags = CpuAccessFlags.Write;
        }
        else
        {
            if (descriptor.Usage.HasFlag(BufferUsage.Vertex))
            {
                d3d11Desc.BindFlags = BindFlags.VertexBuffer;
            }

            if (descriptor.Usage.HasFlag(BufferUsage.Index))
            {
                d3d11Desc.BindFlags = BindFlags.IndexBuffer;
            }

            bool byteAddressBuffer = false;
            bool structuredBuffer = false;

            if (descriptor.Usage.HasFlag(BufferUsage.ShaderRead))
            {
                d3d11Desc.BindFlags |= BindFlags.ShaderResource;
                byteAddressBuffer = true;
            }

            // UAV buffers cannot be dynamic
            if (descriptor.Usage.HasFlag(BufferUsage.ShaderWrite))
            {
                d3d11Desc.BindFlags |= BindFlags.UnorderedAccess;
                byteAddressBuffer = true;
            }
            //else if (CheckBitsAny(desc->usage, BufferUsage::Dynamic))
            //{
            //    bufferDesc.Usage = D3D11_USAGE_DYNAMIC;
            //    bufferDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
            //}

            if (descriptor.Usage.HasFlag(BufferUsage.Indirect))
            {
                d3d11Desc.OptionFlags |= ResourceOptionFlags.DrawIndirectArguments;
            }

            if (byteAddressBuffer)
            {
                d3d11Desc.OptionFlags |= ResourceOptionFlags.BufferAllowRawViews;
            }
            else if (structuredBuffer)
            {
                //bufferDesc.StructureByteStride = desc->stride;
                d3d11Desc.OptionFlags |= ResourceOptionFlags.BufferStructured;
            }
        }

        Handle = device.NativeDevice.CreateBuffer(d3d11Desc, initialData);
    }

    public ID3D11Buffer Handle { get; }

    protected override void OnLabelChanged()
    {
        Handle.DebugName = _label ?? string.Empty;
    }
}
