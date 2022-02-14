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
        using GraphicsDevice device = GraphicsDevice.Create();
        Assert.IsTrue(device is not null);
    }

    [TestMethod]
    public void IsValidationSupported()
    {
        GPUDeviceDescriptor descriptor = new()
        {
            ValidationMode = ValidationMode.Enabled
        };

        using GraphicsDevice device = GraphicsDevice.Create(descriptor);
        Assert.IsTrue(device is not null);
    }

    [TestMethod]
    public void D3D11IsSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            GPUDeviceDescriptor descriptor = new()
            {
                PreferredBackend = GPUBackend.D3D11
            };

            using GraphicsDevice device = GraphicsDevice.Create(descriptor);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.D3D11);
        }
    }

    [TestMethod]
    public void D3D12IsSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            GPUDeviceDescriptor descriptor = new()
            {
                PreferredBackend = GPUBackend.D3D12
            };

            using GraphicsDevice device = GraphicsDevice.Create(descriptor);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.D3D12);
        }
    }

    [TestMethod]
    public void VulkanIsSupported()
    {
        if (GraphicsDevice.IsBackendSupported(GPUBackend.Vulkan))
        {
            GPUDeviceDescriptor descriptor = new()
            {
                PreferredBackend = GPUBackend.Vulkan
            };

            using GraphicsDevice device = GraphicsDevice.Create(descriptor);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.Vulkan);
        }
    }
}
