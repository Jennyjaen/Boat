Shader "Custom/Hand"
{
    Properties
    {
       // _MainTex ("Texture", 2D) = "white" {}
       _Color ("Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 1))=  0.5
        _Angle ("Angle", Range(0, 360)) = 0.0
        _Scale ("Scale", Range(0.0, 1.0)) = 1.0
        _IsLeft("Left", Range(0.0, 1.0))=0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            float _Intensity;
            float _Angle;
            float _Offset;
            float _Scale;
            float _IsLeft;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float radian = _Angle * 3.141592 / 180.0;
                float2x2 rotMat = float2x2(
                    cos(radian), -sin(radian),
                    sin(radian), cos(radian)
                );

                float2 rotateuv = mul(rotMat, v.uv - 0.5) + 0.5;

                o.uv = rotateuv;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                //return col;
                float gradient;
                gradient = i.uv.x * _Intensity;
                return fixed4(gradient, gradient, gradient, 1.0);
            }
            ENDCG
        }
    }
}
