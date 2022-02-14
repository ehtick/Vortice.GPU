// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vortice.GPU.Tests;

[TestClass]
[TestCategory("Initialization")]
public partial class InitializationTests
{
    [TestMethod]
    public void IsSupported()
    {
        using GPUDevice device = GPUDevice.CreateDefault(GPUBackend.Count);
        Assert.IsTrue(device is not null);
    }

    [TestMethod]
    public void IsValidationSupported()
    {
        using GPUDevice device = GPUDevice.CreateDefault(GPUBackend.Count, ValidationMode.Enabled);
        Assert.IsTrue(device is not null);
    }

    [TestMethod]
    public void D3D11IsSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            using GPUDevice device = GPUDevice.CreateDefault(GPUBackend.D3D11);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.D3D11);
        }
    }

    [TestMethod]
    public void D3D12IsSupported()
    {
        if (OperatingSystem.IsWindows())
        {
            using GPUDevice device = GPUDevice.CreateDefault(GPUBackend.D3D12);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.D3D12);
        }
    }

    [TestMethod]
    public void VulkanIsSupported()
    {
        if (GPUDevice.IsBackendSupported(GPUBackend.Vulkan))
        {
            using GPUDevice device = GPUDevice.CreateDefault(GPUBackend.Vulkan);
            Assert.IsTrue(device is not null);
            Assert.AreEqual(device.Backend, GPUBackend.Vulkan);
        }
    }
}
