// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Vortice.Direct3D12;
using Vortice.Mathematics;
using static Vortice.Direct3D12.D3D12;

namespace Vortice.Graphics.D3D12;

internal class D3D12Buffer : Buffer
{
    public D3D12Buffer(D3D12GraphicsDevice device, in BufferDescriptor descriptor, IntPtr initialData)
        : base(device, descriptor)
    {
        ResourceDescription resourceDesc = ResourceDescription.Buffer(descriptor.Size);
        if (descriptor.Usage.HasFlag(BufferUsage.Constant))
        {
            resourceDesc.Width = MathHelper.AlignUp(resourceDesc.Width, ConstantBufferDataPlacementAlignment);
        }

        HeapProperties heapProperties = HeapProperties.DefaultHeapProperties;
        ResourceStates resourceState = ResourceStates.Common;

        if (descriptor.Access == CpuAccess.Read)
        {
            heapProperties = HeapProperties.ReadbackHeapProperties;
            resourceState = ResourceStates.CopyDest;
            resourceDesc.Flags |= ResourceFlags.DenyShaderResource;
        }
        else if (descriptor.Access == CpuAccess.Write)
        {
            heapProperties = HeapProperties.UploadHeapProperties;
            resourceState = ResourceStates.GenericRead;
        }

        Handle = device.NativeDevice.CreateCommittedResource(
            heapProperties,
            HeapFlags.None,
            resourceDesc,
            resourceState);
    }

    public ID3D12Resource Handle { get; }

    protected override void OnLabelChanged()
    {
        Handle.Name = _label ?? string.Empty;
    }
}
