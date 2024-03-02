#version 450 core
layout (location = 0) in vec3 color;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 fragmentPos;
layout (location = 0) out vec4 outColor;

void main()
{
    float ambientStrength = 0.3;
    vec3 lightColor = vec3(1);
    vec3 objectColor = vec3(0.5);
    vec3 lightPos = vec3(1, 4, 1);

    vec3 ambient = ambientStrength * lightColor * objectColor;

    vec3 lightDir = normalize(lightPos - fragmentPos);
    vec3 diffuse = max(dot(normal, lightDir), 0) * lightColor;

    outColor = vec4(ambient + diffuse, 1);
}
