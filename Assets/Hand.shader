Shader "Custom/Compare"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 1))=  0.5
        _Angle ("Angle", Range(0, 360)) = 0.0
        _Scale ("Scale", Range(0.0, 1.0)) = 1.0
        _IsLeft("Left", Range(0, 1))=0
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
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float4 startColor = lerp(float4(0.5, 0.5, 0.5, 1), float4(1, 1, 1, 1), _Intensity);
                float4 endColor = lerp(float4(0.5, 0.5, 0.5, 1), float4(0, 0, 0, 1), _Intensity);
                float direction;
                float adjust_x;
                float adjust_y;

                if(_IsLeft <0.5){
                    adjust_x = (i.uv.x - (1 - _Scale))/ _Scale * 0.5;
                    adjust_x = clamp(adjust_x, 0.0, 1.0);
                    adjust_y = (i.uv.y - (1- _Scale)* 0.5) / _Scale;
                    adjust_y = clamp(adjust_y, 0.0, 1.0);
                }
                else{
                    adjust_x = (i.uv.x)* 0.5 / _Scale + 0.5;
                    adjust_x = clamp(adjust_x, 0.0, 1.0);
                    adjust_y = (i.uv.y + (1- _Scale)* 0.5) / _Scale;
                    adjust_y = clamp(adjust_y, 0.0, 1.0);
                }
                /*
                float adjust_x = (i.uv.x - ((1 - _Scale) * 0.5))/ _Scale;
                adjust_x = clamp(adjust_x, 0.0, 1.0);
                float adjust_y = (i.uv.y - ((1 - _Scale)* 0.5)) / _Scale;
                adjust_y = clamp(adjust_y, 0.0, 1.0);*/

                
                if(_Angle >= 337.5 || _Angle < 22.5){
                    direction = adjust_x;
                }
                else if(_Angle >=22.5 && _Angle <67.5){
                    direction = (adjust_x + adjust_y) * 0.5;
                }
                else if(_Angle >=67.5 && _Angle <112.5){
                    direction = adjust_y;
                }
                else if(_Angle >=112.5 && _Angle <157.5){
                    direction = (1.0 - adjust_x + adjust_y)*0.5;
                }
                else if(_Angle >=157.5 && _Angle < 202.5){
                    direction  = 1.0 - adjust_x;
                }
                else if(_Angle >=202.5 && _Angle <247.5){
                    direction = (2.0 - adjust_x - adjust_y) * 0.5;
                }
                else if(_Angle >=247.5 && _Angle < 292.5){
                    direction = 1.0 - adjust_y;
                }
                else if(_Angle >=292.5 && _Angle < 337.5){
                    direction = (1.0 + adjust_x - adjust_y ) * 0.5;
                }


                if(i.uv.x >= 0 && i.uv.x <= (1-_Scale) && _IsLeft < 0.5){
                    return fixed4(1, 1, 1, 1);
                } 
                if (i.uv.x <= 1 && i.uv.x >=  _Scale && _IsLeft >= 0.5){
                    return fixed4(1, 1, 1, 1);
                }
                if(i.uv.y >= 0 && i.uv.y <= (1- _Scale)/2){
                    return fixed4(1, 1, 1, 1);
                }
                if(i.uv.y <= 1 && i.uv.y >= (1 + _Scale)/2)
                {
                    return fixed4(1, 1, 1, 1);
                }
                

                return lerp(endColor, startColor, direction) ;
            }
            ENDCG
        }
    }
}
