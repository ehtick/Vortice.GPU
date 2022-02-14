// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.Graphics.Samples;

class Program
{
    static void Main()
    {
        ValidationMode validationMode = ValidationMode.Disabled;
#if DEBUG
        validationMode = ValidationMode.Enabled;
#endif
        GPUBackend preferredBackend = GPUBackend.Count;

        using HelloWorldApp app = new(preferredBackend, validationMode);
        app.Run();
    }
}
