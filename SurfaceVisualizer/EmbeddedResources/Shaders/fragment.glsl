#version 450 core
layout (location = 0) in vec3 color;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec3 fragmentPos;
layout (location = 0) out vec4 outColor;

uniform float ambientStrength;
uniform float diffuseStrength;
uniform vec3 lightColor;
uniform vec3 objectColor;
uniform vec3 lightPos;
uniform int drawingFront;

void main()
{
    vec3 ambient = ambientStrength * lightColor * objectColor;

    vec3 lightDir = normalize(lightPos - fragmentPos);
    
    vec3 correctedNormal = drawingFront == 1 ? normal : -normal;
    vec3 diffuse = diffuseStrength * max(dot(correctedNormal, lightDir), 0) * lightColor * objectColor;

    outColor = vec4(ambient + diffuse, 1);
}
