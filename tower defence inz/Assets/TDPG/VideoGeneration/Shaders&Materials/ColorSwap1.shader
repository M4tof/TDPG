Shader "TDPG/DominantColorSwap_Instanced"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _OriginalColor("Original Color 1", Color) = (1,1,1,1)
        _TargetColor("Target Color 1", Color) = (1,1,1,1)
        _Tolerance("Tolerance", Range(0, 10)) = 1
    }
 
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" } 
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // 1. Enable Instancing
            #pragma multi_compile_instancing 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                // 2. Add Instance ID to input
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                // 3. Add Instance ID to v2f to pass it to pixel shader
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
 
            // 4. Wrap properties in an Instancing Buffer
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _OriginalColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _TargetColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _Tolerance)
            UNITY_INSTANCING_BUFFER_END(Props)
 
            v2f vert(appdata v)
            {
                v2f o;
                // 5. Setup instancing in vertex
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); 
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
 
            half4 frag(v2f i) : SV_Target
            {
                // 6. Setup instancing in fragment
                UNITY_SETUP_INSTANCE_ID(i); 

                half4 col = tex2D(_MainTex, i.uv);
 
                if (col.a == 0)
                {
                    return half4(0, 0, 0, 0);
                }
 
                // 7. Access properties via the accessor macro
                float4 original = UNITY_ACCESS_INSTANCED_PROP(Props, _OriginalColor);
                float4 target = UNITY_ACCESS_INSTANCED_PROP(Props, _TargetColor);
                float tol = UNITY_ACCESS_INSTANCED_PROP(Props, _Tolerance);

                if (length(col - original) < tol)
                {
                    return half4(target.rgb, col.a);
                }
 
                return col;
            }
 
            ENDCG
        }
    }
}