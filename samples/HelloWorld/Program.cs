// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace Vortice.GPU.Samples;

class Program
{
    static void Main()
    {
#if DEBUG
        //GPUDevice.ValidationMode = ValidationMode.Enabled;
#endif
        GPUBackend preferredBackend = GPUBackend.Direct3D11;

        using HelloWorldApp app = new HelloWorldApp(preferredBackend);
        app.Run();
    }
}
