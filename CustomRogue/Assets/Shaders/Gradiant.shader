Shader "Custom/RedLit"
{
    Properties
    {
        _ColorBottom ("Bottom Color", Color) = (1,1,0,1)
        _ColorTop ("Top Color", Color) = (1,0,0,1)

        _MinY ("Min Y", Float) = 0.0
        _MaxY ("Max Y", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos         : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            fixed4 _ColorBottom;
            fixed4 _ColorTop;
            float _MinY;
            float _MaxY;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // transform normal from object space to world space
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // normalize world normal
                float3 n = normalize(i.worldNormal);

                // main directional light direction (world space)
                float3 l = normalize(_WorldSpaceLightPos0.xyz);

                // Lambert: N · L
                float NdotL = max(0.0, dot(n, l));

                // diffuse + ambient
                float3 diffuse = _LightColor0.rgb * NdotL;
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;

                float3 lighting = diffuse + ambient;

                // compute gradient factor from world Y position
                float t = saturate((i.worldPos.y - _MinY) / (_MaxY - _MinY));

                // create gradient
                float3 grad = lerp(_ColorBottom.rgb, _ColorTop.rgb, t);

                // apply lighting
                return fixed4(grad * lighting, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
