// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace Vortice.GPU;

internal static class PlatformInfo
{
    public static bool IsUnix { get; }

    public static bool IsWindows { get; }

    public static bool IsMacOS { get; }

    public static bool IsLinux { get; }

    public static bool IsAndroid { get; }

    public static bool IsArm { get; }

    public static bool Is64BitOperatingSystem { get; }

    static PlatformInfo()
    {
#if WINDOWS_UWP
		IsMac = false;
		IsLinux = false;
		IsUnix = false;
		IsWindows = true;

		var arch = Package.Current.Id.Architecture;
		const ProcessorArchitecture arm64 = (ProcessorArchitecture)12;
		IsArm = arch == ProcessorArchitecture.Arm || arch == arm64;
        Is64BitOperatingSystem = IntPtr.Size == 8;
#elif NET6_0_OR_GREATER
        IsMacOS = OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst();
        IsLinux = OperatingSystem.IsLinux();
        IsUnix = IsMacOS || IsLinux;
        IsWindows = OperatingSystem.IsWindows();
        IsAndroid = OperatingSystem.IsAndroid();

        var arch = RuntimeInformation.ProcessArchitecture;
        IsArm = arch == Architecture.Arm || arch == Architecture.Arm64;
        Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
#else
        IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        IsUnix = IsMacOS || IsLinux;
        IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        IsAndroid = RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"));

        var arch = RuntimeInformation.ProcessArchitecture;
        IsArm = arch == Architecture.Arm || arch == Architecture.Arm64;
        Is64BitOperatingSystem = IntPtr.Size == 8;
#endif
    }
}
