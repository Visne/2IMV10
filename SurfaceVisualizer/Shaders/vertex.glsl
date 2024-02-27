#version 450 core
in vec3 position;
in vec3 color;
layout (location = 0) out vec3 outColor;

void main()
{
    gl_Position = vec4(position, 1.0);
    outColor = color;
}
