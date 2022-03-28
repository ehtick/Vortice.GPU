// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics.Samples;

public class HelloWorldApp : Application
{
    public HelloWorldApp(BackendType preferredBackend = BackendType.Count, ValidationMode validationMode = ValidationMode.Disabled)
        : base(preferredBackend, validationMode)
    {
    }

    protected override void Initialize()
    {
        using Texture texture = GraphicsDevice.CreateTexture(TextureDescriptor.Texture2D(TextureFormat.RGBA8UNorm, 256, 256));
    }
}
