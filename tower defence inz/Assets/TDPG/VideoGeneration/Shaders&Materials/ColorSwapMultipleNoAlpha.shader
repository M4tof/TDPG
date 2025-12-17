Shader "TDPG/MultiColorSwap_Instanced_16_PerColor"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        
        [HideInInspector] _Count("Count", Int) = 0
        
        // _Orig now stores (R, G, B, Tolerance)
        [HideInInspector][Gamma] _Orig0("O0", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ0("T0", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig1("O1", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ1("T1", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig2("O2", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ2("T2", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig3("O3", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ3("T3", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig4("O4", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ4("T4", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig5("O5", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ5("T5", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig6("O6", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ6("T6", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig7("O7", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ7("T7", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig8("O8", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ8("T8", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig9("O9", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ9("T9", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig10("OA", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ10("TA", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig11("OB", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ11("TB", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig12("OC", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ12("TC", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig13("OD", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ13("TD", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig14("OE", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ14("TE", Color)=(0,0,0,0)
        [HideInInspector][Gamma] _Orig15("OF", Color)=(0,0,0,0) [HideInInspector][Gamma] _Targ15("TF", Color)=(0,0,0,0)
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
                UNITY_DEFINE_INSTANCED_PROP(int, _Count)
                // Removed Global Tolerance
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig0) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ0)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig1) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ1)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig2) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ2)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig3) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ3)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig4) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ4)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig5) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ5)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig6) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ6)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig7) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ7)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig8) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ8)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig9) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ9)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig10) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ10)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig11) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ11)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig12) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ12)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig13) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ13)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig14) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ14)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Orig15) UNITY_DEFINE_INSTANCED_PROP(float4, _Targ15)
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

            // MACRO UPDATE:
            // 1. Get the Orig property as a float4
            // 2. Compare col.rgb vs Orig.rgb
            // 3. Use Orig.a as the tolerance check
            #define CHECK_COLOR(idx) \
                float4 o##idx = UNITY_ACCESS_INSTANCED_PROP(Props, _Orig##idx); \
                if (count > idx && length(col.rgb - o##idx.rgb) < o##idx.a) \
                    return half4(UNITY_ACCESS_INSTANCED_PROP(Props, _Targ##idx).rgb, col.a);

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); 
                half4 col = tex2D(_MainTex, i.uv);
                
                if (col.a == 0) return half4(0, 0, 0, 0);
 
                int count = UNITY_ACCESS_INSTANCED_PROP(Props, _Count);

                CHECK_COLOR(0)
                CHECK_COLOR(1)
                CHECK_COLOR(2)
                CHECK_COLOR(3)
                CHECK_COLOR(4)
                CHECK_COLOR(5)
                CHECK_COLOR(6)
                CHECK_COLOR(7)
                CHECK_COLOR(8)
                CHECK_COLOR(9)
                CHECK_COLOR(10)
                CHECK_COLOR(11)
                CHECK_COLOR(12)
                CHECK_COLOR(13)
                CHECK_COLOR(14)
                CHECK_COLOR(15)

                return col;
            }
            ENDCG
        }
    }
}