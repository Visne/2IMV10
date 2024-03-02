#version 450 core
layout (location = 0) in vec3 color;
layout (location = 1) in vec3 normal;
layout (location = 0) out vec4 outColor;

void main()
{
    outColor = vec4(normal, 1);
}
