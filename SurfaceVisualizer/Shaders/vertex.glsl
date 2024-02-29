#version 450 core
in vec3 position;
in vec3 color;
layout (location = 0) out vec3 outColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(position, 1.0);
    outColor = color;
}
