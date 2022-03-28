// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.Serialization;

namespace Vortice.Graphics;

public sealed class GraphicsException : Exception
{
    public GraphicsException()
    {
    }

    public GraphicsException(string message) : base(message)
    {

    }

    protected GraphicsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public GraphicsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
