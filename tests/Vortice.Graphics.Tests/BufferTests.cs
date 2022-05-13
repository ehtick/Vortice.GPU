// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vortice.Graphics.Tests;

[TestClass]
[TestCategory("Buffer")]
public partial class BufferTests
{
    [TestMethod]
    public void CreateBufferDefault()
    {
        ulong bufferSize = 256;
        using GraphicsDevice device = GraphicsDevice.Create();
        using Buffer buffer = device.CreateBuffer(new BufferDescriptor(bufferSize));
        Assert.IsNotNull(buffer);
        Assert.AreSame(buffer.Device, device);
        Assert.AreEqual(buffer.Usage, BufferUsage.ShaderReadWrite);
        Assert.AreEqual(buffer.Size, bufferSize);
        Assert.AreEqual(buffer.Access, CpuAccess.None);
        Assert.AreEqual(buffer.SharedResourceFlags, SharedResourceFlags.None);
    }
}
