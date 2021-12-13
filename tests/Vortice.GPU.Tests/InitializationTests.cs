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
        Assert.IsTrue(GPUDevice.Default is not null);
    }
}
