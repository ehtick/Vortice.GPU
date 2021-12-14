// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using Vortice.Vulkan;
using static Vortice.Vulkan.Vulkan;

namespace Vortice.GPU.Vulkan;

internal struct VulkanPhysicalDeviceExtensions
{
    public bool Swapchain;
    public bool depth_clip_enable;
    public bool memory_budget;
    public bool performance_query;
    public bool deferred_host_operations;
    public bool renderPass2;
    public bool accelerationStructure;
    public bool raytracingPipeline;
    public bool rayQuery;
    public bool fragment_shading_rate;
    public bool NV_mesh_shader;
    public bool EXT_conditional_rendering;
    public bool win32_full_screen_exclusive;
    public bool vertex_attribute_divisor;
    public bool extended_dynamic_state;
    public bool vertex_input_dynamic_state;
    public bool extended_dynamic_state2;
    public bool dynamic_rendering;
}
