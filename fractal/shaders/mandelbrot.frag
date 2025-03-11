#version 330 core

out vec4 FragColor;

uniform vec2 resolution;
uniform vec2 center;
uniform float zoom;
uniform int maxIterations;
uniform int progress;
void main()
{

    vec2 c = ((gl_FragCoord.xy / resolution) * 2.0 - 1.0) * zoom + center;


    vec2 z = vec2(0.0);
    int iter = 0;
    while (iter < progress && dot(z, z) < 4.0)
    {
        z = vec2(z.x * z.x - z.y * z.y, 2.0 * z.x * z.y) + c;
        iter++;
    }

    float color = float(iter) / float(maxIterations);

    FragColor = vec4(vec3(sqrt(color)), 1.0);
}