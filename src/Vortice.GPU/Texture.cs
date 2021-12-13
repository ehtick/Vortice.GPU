// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

namespace Vortice.GPU;

public abstract class Texture : GPUResource
{
    protected Texture(GPUDevice device) : base(device)
    {
    }
}
