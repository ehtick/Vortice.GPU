// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.GPU.Samples;

public class HelloWorldApp : Application
{
    protected override void Initialize()
    {
        using Texture texture = Device.CreateTexture(TextureDescriptor.Texture2D(TextureFormat.RGBA8UNorm, 256, 256));
    }
}
