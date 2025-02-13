Shader "Custom/Water2D"
{
Properties
{
[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
_Color ("Water Color", Color) = (0.0, 0.4, 1.0, 0.8)
_FoamColor ("Foam Color", Color) = (1,1,1,1)
_WaveSpeed ("Wave Speed", Range(0,5)) = 1
_WaveHeight ("Wave Height", Range(0,1)) = 0.1
_WaveFrequency ("Wave Frequency", Range(0,10)) = 2
_WaveDirection ("Wave Direction", Vector) = (1,0,0,0)
_FoamScale ("Foam Scale", Range(1,50)) = 20
_FoamSpeed ("Foam Speed", Range(0,5)) = 1
_FoamIntensity ("Foam Intensity", Range(0,10)) = 3
_FoamThreshold ("Foam Threshold", Range(0,1)) = 0.7
_FoamStretch ("Foam Stretch", Range(0,1)) = 0.3
_FoamTexture ("Foam Texture", 2D) = "white" {}
_FoamTextureScale ("Foam Texture Scale", Range(1,50)) = 10
_FoamTextureSpeed ("Foam Texture Speed", Range(0,5)) = 1
_FoamTextureIntensity ("Foam Texture Intensity", Range(0,1)) = 0.5
_FoamNoise1Scale ("Foam Noise 1 Scale", Range(1,50)) = 10
_FoamNoise1Speed ("Foam Noise 1 Speed", Range(0,5)) = 1
_FoamNoise2Scale ("Foam Noise 2 Scale", Range(1,50)) = 20
_FoamNoise2Speed ("Foam Noise 2 Speed", Range(0,5)) = 1.5
}
SubShader
{
Tags
{
"Queue"="Transparent"
"IgnoreProjector"="True"
"RenderType"="Transparent"
"PreviewType"="Plane"
"CanUseSpriteAtlas"="True"
}
Cull Off
Lighting Off
ZWrite Off
Blend SrcAlpha OneMinusSrcAlpha
Pass
{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata_t
{
float4 vertex : POSITION;
float2 texcoord : TEXCOORD0;
float4 color : COLOR;
};
struct v2f
{
float4 vertex : SV_POSITION;
float2 texcoord : TEXCOORD0;
float4 color : COLOR;
float3 localPos : TEXCOORD1;
float height : TEXCOORD2;
};
sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color;
float4 _FoamColor;
float _WaveSpeed;
float _WaveHeight;
float _WaveFrequency;
float4 _WaveDirection;
float _FoamScale;
float _FoamSpeed;
float _FoamIntensity;
float _FoamThreshold;
float _FoamStretch;
sampler2D _FoamTexture;
float4 _FoamTexture_ST;
float _FoamTextureScale;
float _FoamTextureSpeed;
float _FoamTextureIntensity;
float _FoamNoise1Scale;
float _FoamNoise1Speed;
float _FoamNoise2Scale;
float _FoamNoise2Speed;
// Функція для генерації псевдовипадкового вектору на основі координат
float2 random2(float2 st)
{
st = float2(dot(st, float2(127.1,311.7)),
dot(st, float2(269.5,183.3)));
return -1.0 + 2.0 * frac(sin(st) * 43758.5453123);
}
// Класичний шум на основі 2D координат
float noise(float2 st)
{
float2 i = floor(st);
float2 f = frac(st);
float2 u = f * f * (3.0 - 2.0 * f);
return lerp(lerp(dot(random2(i + float2(0.0, 0.0)), f - float2(0.0, 0.0)),
dot(random2(i + float2(1.0, 0.0)), f - float2(1.0, 0.0)), u.x),
lerp(dot(random2(i + float2(0.0, 1.0)), f - float2(0.0, 1.0)),
dot(random2(i + float2(1.0, 1.0)), f - float2(1.0, 1.0)), u.x), u.y);
}
// Функція перлин-шум (noise) для згладженого переходу значень
float pnoise(float2 st)
{
float2 i = floor(st);
float2 f = frac(st);
float2 u = f * f * (3.0 - 2.0 * f);
float a = noise(i);
float b = noise(i + float2(1.0, 0.0));
float c = noise(i + float2(0.0, 1.0));
float d = noise(i + float2(1.0, 1.0));
return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}
// Функція вершин: вона використовує лише локальні координати,
// після чого додає коливання (хвилі) за допомогою налаштовуваних параметрів.
v2f vert(appdata_t IN)
{
v2f OUT;
// Беремо лише геометрію з об'єкта (локальні координати)
float3 localPos = IN.vertex.xyz;
// Формування хвиль: на підставі локальних координат,
// частоти, швидкості та напрямку
float2 waveUV = localPos.xy * _WaveFrequency;
float2 waveOffset = _WaveDirection.xy * _Time.y * _WaveSpeed;
float wave = sin(dot(waveUV + waveOffset, normalize(_WaveDirection.xy)));
localPos.y += wave * _WaveHeight;
// Обчислення остаточної позиції вершини
OUT.vertex = UnityObjectToClipPos(float4(localPos, 1.0));
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
// Передаємо оновлені локальні координати для використання у фрагментній функції
OUT.localPos = localPos;
OUT.height = wave;
return OUT;
}
// Функція фрагментів
fixed4 frag(v2f IN) : SV_Target
{
    fixed4 texColor = tex2D(_MainTex, IN.texcoord);
    
    // Базові UV координати для піни
    float2 foamUV = IN.localPos.xy;
    
    // Створюємо кілька шарів шуму з різними напрямками та швидкостями
    float2 noise1UV = foamUV * _FoamNoise1Scale + float2(
        _Time.y * _FoamNoise1Speed,
        _Time.y * _FoamNoise1Speed * 0.7
    );
    
    float2 noise2UV = foamUV * _FoamNoise2Scale + float2(
        sin(_Time.y * 0.5) * _FoamNoise2Speed,
        cos(_Time.y * 0.7) * _FoamNoise2Speed
    );
    
    float2 noise3UV = foamUV * (_FoamNoise1Scale * 0.7) + float2(
        cos(_Time.y * 0.4) * _FoamNoise1Speed,
        sin(_Time.y * 0.6) * _FoamNoise1Speed
    );
    
    float2 noise4UV = foamUV * (_FoamNoise2Scale * 1.3) + float2(
        sin(_Time.y * 0.8) * _FoamNoise2Speed,
        -cos(_Time.y * 0.9) * _FoamNoise2Speed
    );
    
    // Генеруємо шуми з різними характеристиками
    float noise1 = noise(noise1UV);
    float noise2 = noise(noise2UV);
    float noise3 = noise(noise3UV);
    float noise4 = noise(noise4UV);
    
    // Створюємо різкі переходи для різних форм
    float foam1 = step(0.65, noise1) * step(noise2, 0.35);
    float foam2 = step(0.6, noise3) * step(noise4, 0.4);
    float foam3 = step(0.55, abs(noise1 - noise2));
    float foam4 = step(0.5, abs(noise3 - noise4));
    
    // Комбінуємо всі шари піни з різними вагами
    float foamMask = saturate((foam1 * 0.4 + foam2 * 0.3 + foam3 * 0.2 + foam4 * 0.1) * _FoamIntensity);
    
    // Додаємо хаотичні варіації
    float timeVar = _Time.y * 0.1;
    float2 varUV = foamUV * 0.3 + float2(sin(timeVar * 1.1), cos(timeVar * 0.9));
    float variation = pnoise(varUV) * 0.2;
    foamMask = saturate(foamMask + variation);
    
    // Застосовуємо поріг з більш різким переходом
    foamMask = smoothstep(_FoamThreshold, _FoamThreshold + _FoamStretch * 0.5, foamMask);
    
    // Змішуємо кольори води та піни
    float3 waterColor = _Color.rgb;
    float3 foamColor = _FoamColor.rgb;
    float3 finalColor = lerp(waterColor, foamColor, foamMask);
    
    // Зберігаємо прозорість
    float alpha = texColor.a * _Color.a;
    return fixed4(finalColor, alpha);
}
ENDCG
}
}
}