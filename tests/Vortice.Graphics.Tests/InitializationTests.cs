// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vortice.Graphics.Tests;

[TestClass]
[TestCategory("Initialization")]
public partial class InitializationTests
{
    [TestMethod]
    public void IsSupported()
    {
        using GraphicsDevice device = GraphicsDevice.CreateDefault(GPUBackend.Count);
        Assert.IsTrue(device is not null);
    }

    [TestMethod]
    public void IsValidationSupported()
    {
        using GraphicsDevice device = GraphicsDevice.CreateDefault(GPUBackend.Count, ValidationMode.Enabled);
        Assert.IsTrue(device is not null);
    }

    [TestMethod]
    public void D3D11IsSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            using GraphicsDevice device = GraphicsDevice.CreateDefault(GPUBackend.D3D11);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.D3D11);
        }
    }

    [TestMethod]
    public void D3D12IsSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            using GraphicsDevice device = GraphicsDevice.CreateDefault(GPUBackend.D3D12);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.D3D12);
        }
    }

    [TestMethod]
    public void VulkanIsSupported()
    {
        if (GraphicsDevice.IsBackendSupported(GPUBackend.Vulkan))
        {
            using GraphicsDevice device = GraphicsDevice.CreateDefault(GPUBackend.Vulkan);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.Vulkan);
        }
    }
}
