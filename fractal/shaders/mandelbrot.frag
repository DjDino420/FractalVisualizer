#version 330 core

out vec4 FragColor;

uniform vec2 resolution;
uniform vec2 center;
uniform float zoom;
uniform int maxIterations;

void main()
{

    vec2 c = (gl_FragCoord.xy * 2.0 - resolution) / resolution.y * zoom + center;


    int dynamicIterations = maxIterations + int(log(zoom + 1.0) * 100.0);

    vec2 z = vec2(0.0);
    int iter = 0;
    while (iter < dynamicIterations && dot(z, z) < 4.0)
    {
        z = vec2(z.x * z.x - z.y * z.y, 2.0 * z.x * z.y) + c;
        iter++;
    }

    float color = float(iter) / float(dynamicIterations);
    FragColor = vec4(vec3(color), 1.0);
}