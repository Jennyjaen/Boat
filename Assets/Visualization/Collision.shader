Shader "Custom/Collision"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Angle("Angle", Range(0, 360)) = 0
        _Scale ("Scale", Range(0.0, 5.0))=0
        _Collision("Collision", Range(0, 1)) = 0
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
            float _Angle;
            float _Scale;
            float _Collision;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 p = i.uv;
                float intensity = 1- ceil(_Scale) / 5;
                float a = _Scale / 5;
                if(_Angle >= 22.5 && _Angle <67.5){
                    if(p.x + p.y >= (2 - a* 2) ){
                        return fixed4(intensity, intensity, intensity, 1);
                    }
                    else{return fixed4(1, 1, 1, 1);}
                }
                else if(_Angle >= 67.5 && _Angle <112.5){
                    if(p.y >= 1 - a ){
                        return fixed4(intensity, intensity, intensity, 1);
                    }
                    else{return fixed4(1, 1, 1, 1);}
                }
                else if(_Angle >= 112.5 && _Angle <157.5){
                    if( -p.x + p.y >= 1 - 2 * a){
                        return fixed4(intensity, intensity, intensity, 1);
                    }
                    else{return fixed4(1, 1, 1, 1);}
                }
                else if(_Angle >= 157.5 && _Angle <202.5){
                    if(p.x < a ){
                        return fixed4(intensity, intensity, intensity, 1);
                    }
                    else{return fixed4(1, 1, 1, 1);}
                }
                else if(_Angle >= 202.5 && _Angle <247.5){
                    if(p.x + p.y <= 2 *a){
                        return fixed4(intensity, intensity, intensity, 1);
                    }
                    else{return fixed4(1, 1, 1, 1);}
                }
                else if(_Angle >= 247.5 && _Angle <292.5){
                    if(p.y < a ){
                        return fixed4(intensity, intensity, intensity, 1);
                    }
                    else{return fixed4(1, 1, 1, 1);}
                }
                else if(_Angle >= 292.5 && _Angle <337.5){
                    if(p.x - p.y >= 1 - 2 *a ){
                        return fixed4(intensity, intensity, intensity,1);
                    }
                    else{return fixed4(1, 1, 1, 1);}
                }
                else{
                    if(p.x >= 1- a ){
                        return fixed4(intensity, intensity, intensity, 1);
                    }
                    else{return fixed4(1, 1, 1, 1);}
                }


                return fixed4(1, 1, 1, 1);

            }
            ENDCG
        }
    }
}
