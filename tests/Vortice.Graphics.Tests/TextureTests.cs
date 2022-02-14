// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vortice.Graphics.Tests;

[TestClass]
[TestCategory("Texture")]
public partial class TextureTests
{
    [TestMethod]
    public void CreateTexture2D()
    {
        using GraphicsDevice device = GraphicsDevice.Create();
        using Texture texture = device.CreateTexture(TextureDescriptor.Texture2D(TextureFormat.RGBA8UNorm, 256, 256));
        Assert.IsNotNull(texture);
        Assert.AreSame(texture.Device, device);
        Assert.AreEqual(texture.Format, TextureFormat.RGBA8UNorm);
        Assert.AreEqual(texture.Width, 256);
        Assert.AreEqual(texture.Height, 256);
        Assert.AreEqual(texture.Depth, 1);
        Assert.AreEqual(texture.ArraySize, 1);
        Assert.AreEqual(texture.MipLevels, 1);
        Assert.AreEqual(texture.SampleCount, TextureSampleCount.Count1);
        Assert.AreEqual(texture.Usage, TextureUsage.ShaderRead);
    }

    [TestMethod]
    public void CreateTexture2DWithMipLevels()
    {
        using GraphicsDevice device = GraphicsDevice.Create();
        using Texture texture = device.CreateTexture(TextureDescriptor.Texture2D(TextureFormat.RGBA8UNorm, 256, 256, 0));
        Assert.IsNotNull(texture);
        Assert.AreSame(texture.Device, device);
        Assert.AreEqual(texture.MipLevels, 9);
    }
}
