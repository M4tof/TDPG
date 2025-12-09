Shader "TDPG/MultiColorSwap_Instanced"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tolerance("Tolerance", Range(0, 10)) = 0.05
        
        [HideInInspector] _Count("Count", Int) = 0
        
        // Slots 0-7 (Total 8)
        [HideInInspector] [Gamma] _Orig0("O0", Color) = (0,0,0,0) [HideInInspector] [Gamma] _Targ0("T0", Color) = (0,0,0,0)
        [HideInInspector] [Gamma] _Orig1("O1", Color) = (0,0,0,0) [HideInInspector] [Gamma] _Targ1("T1", Color) = (0,0,0,0)
        [HideInInspector] [Gamma] _Orig2("O2", Color) = (0,0,0,0) [HideInInspector] [Gamma] _Targ2("T2", Color) = (0,0,0,0)
        [HideInInspector] [Gamma] _Orig3("O3", Color) = (0,0,0,0) [HideInInspector] [Gamma] _Targ3("T3", Color) = (0,0,0,0)
        [HideInInspector] [Gamma] _Orig4("O4", Color) = (0,0,0,0) [HideInInspector] [Gamma] _Targ4("T4", Color) = (0,0,0,0)
        [HideInInspector] [Gamma] _Orig5("O5", Color) = (0,0,0,0) [HideInInspector] [Gamma] _Targ5("T5", Color) = (0,0,0,0)
        [HideInInspector] [Gamma] _Orig6("O6", Color) = (0,0,0,0) [HideInInspector] [Gamma] _Targ6("T6", Color) = (0,0,0,0)
        [HideInInspector] [Gamma] _Orig7("O7", Color) = (0,0,0,0) [HideInInspector] [Gamma] _Targ7("T7", Color) = (0,0,0,0)
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
            #pragma multi_compile_instancing 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
 
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Tolerance)
                UNITY_DEFINE_INSTANCED_PROP(int, _Count)
                
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig0) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ0)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig1) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ1)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig2) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ2)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig3) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ3)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig4) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ4)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig5) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ5)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig6) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ6)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig7) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ7)
            UNITY_INSTANCING_BUFFER_END(Props)
 
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); 
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
 
            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); 
                half4 col = tex2D(_MainTex, i.uv);
                
                if (col.a == 0) return half4(0, 0, 0, 0);
 
                float tol = UNITY_ACCESS_INSTANCED_PROP(Props, _Tolerance);
                int count = UNITY_ACCESS_INSTANCED_PROP(Props, _Count);

                // Check 0
                if (count > 0 && length(col.rgb - UNITY_ACCESS_INSTANCED_PROP(Props, _Orig0).rgb) < tol) 
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ0).rgb, col.a);
                // Check 1
                if (count > 1 && length(col.rgb - UNITY_ACCESS_INSTANCED_PROP(Props, _Orig1).rgb) < tol) 
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ1).rgb, col.a);
                // Check 2
                if (count > 2 && length(col.rgb - UNITY_ACCESS_INSTANCED_PROP(Props, _Orig2).rgb) < tol) 
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ2).rgb, col.a);
                // Check 3
                if (count > 3 && length(col.rgb - UNITY_ACCESS_INSTANCED_PROP(Props, _Orig3).rgb) < tol) 
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ3).rgb, col.a);
                // Check 4
                if (count > 4 && length(col.rgb - UNITY_ACCESS_INSTANCED_PROP(Props, _Orig4).rgb) < tol) 
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ4).rgb, col.a);
                // Check 5
                if (count > 5 && length(col.rgb - UNITY_ACCESS_INSTANCED_PROP(Props, _Orig5).rgb) < tol) 
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ5).rgb, col.a);
                // Check 6
                if (count > 6 && length(col.rgb - UNITY_ACCESS_INSTANCED_PROP(Props, _Orig6).rgb) < tol) 
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ6).rgb, col.a);
                // Check 7
                if (count > 7 && length(col.rgb - UNITY_ACCESS_INSTANCED_PROP(Props, _Orig7).rgb) < tol) 
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ7).rgb, col.a);

                return col;
            }
            ENDCG
        }
    }
}