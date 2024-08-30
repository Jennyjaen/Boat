Shader "Custom/Hand"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 1))=  0.5
        _Angle ("Angle", Range(0, 360)) = 0.0
        _Scale ("Scale", Range(0.0, 1.0)) = 1.0
        _IsLeft("Left", Range(0, 1))=0
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
            float _Intensity;
            float _Angle;
            float _Offset;
            float _Scale;
            float _IsLeft;
            float _Collision;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 CellCenter(float x, float y, float2 gridSize){
                float2 cellSize = 1.0 / gridSize;
                float2 cellIndex = floor(float2(x, y) * gridSize);
                float2 cell = (cellIndex + 0.5) * cellSize;
                return cell;
            }

            float2 CellCoord(float x, float y){
                float2 gridSize = float2(12.0, 18.0);
                float2 cellSize = 1.0 / gridSize;
                float2 cellIndex = floor(float2(x, y) * gridSize);
                return cellIndex;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center;
                if(_Collision < 0.5){
                    float4 startColor = lerp(float4(0.5, 0.5, 0.5, 1), float4(1, 1, 1, 1), _Intensity);
                    float4 endColor = lerp(float4(0.5, 0.5, 0.5, 1), float4(0, 0, 0, 1), _Intensity);
                    float garo = round(12 * _Scale);
                    float sero = round(18 * _Scale);

                    float direction;
                    float adjust_x;
                    float adjust_y;

                    float2 origin_center = CellCoord(i.uv.x, i.uv.y);
                    if(origin_center.x >= 0 && origin_center.x <(12 - garo) && _IsLeft < 0.5){
                        return fixed4(1, 1, 1, 1);
                    } 
                    else if (origin_center.x <= 11 && origin_center.x > garo  && _IsLeft >= 0.5){
                        return fixed4(1, 1, 1, 1);
                    }
                    else if(origin_center.y >= 0 && origin_center.y < floor((18- sero)/2)){
                        return fixed4(1, 1, 1, 1);
                    }
                    else if(origin_center.y <= 18 && origin_center.y >= 18 - ceil((18 - sero)/2))
                    {
                        return fixed4(1, 1, 1, 1);
                    }

                    else{
                        float2 center = CellCenter(i.uv.x, i.uv.y, float2(12, 18));
                        if(_IsLeft <0.5){
                            adjust_x = (center.x - (1 - _Scale)* 0.5)/ _Scale * 0.5;
                            adjust_x = clamp(adjust_x, 0.0, 1.0);
                            adjust_y = (center.y - (1- _Scale)* 0.5) / _Scale;
                            adjust_y = clamp(adjust_y, 0.0, 1.0);

                            //center = CellCenter(adjust_x, adjust_y, float2(garo, sero));
                        }
                        else{
                            adjust_x = (center.x)* 0.5 / _Scale +0.5;
                            adjust_x = clamp(adjust_x, 0.0, 1.0);
                            adjust_y = (center.y - (1- _Scale)* 0.5) / _Scale;
                            adjust_y = clamp(adjust_y, 0.0, 1.0);
                            //center = CellCenter(adjust_x, adjust_y, float2(garo, sero)); 
                        }

                        
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
                        direction = direction * 6;
                        direction = floor(direction);
                        if(direction==6){direction = 5;}
                        direction /= 5;

                        return lerp(endColor, startColor, direction) ;

                    }

                    
                }
                else if(_Collision == 0.5){
                    return fixed4(1, 1, 1, 1);
                }
                else{
                    float2 p = i.uv;
                    if(_IsLeft< 0.5){
                        p.x = p.x * 0.5f;
                    }
                    else{
                        p.x = p.x * 0.5f + 0.5f;
                    }
                    center = CellCenter(p.x, p.y, float2(12, 18));
                    float intensity = 1- ceil(_Scale * 5) / 5;
                    intensity *= 6;
                    intensity = floor(intensity);
                    if(intensity ==6){
                        intensity = 5;
                    }
                    intensity /=5;
                    float a = _Scale;
                    if(_Angle >= 22.5 && _Angle <67.5){
                        if(center.x + center.y >= (2 - a* 2) ){
                            return fixed4(intensity, intensity, intensity, 1);
                        }
                        else{return fixed4(1, 1, 1, 1);}
                    }
                    else if(_Angle >= 67.5 && _Angle <112.5){
                        if(center.y >= 1 - a ){
                            return fixed4(intensity, intensity, intensity, 1);
                        }
                        else{return fixed4(1, 1, 1, 1);}
                    }
                    else if(_Angle >= 112.5 && _Angle <157.5){
                        if( -center.x + center.y >= 1 - 2 * a){
                            return fixed4(intensity, intensity, intensity, 1);
                        }
                        else{return fixed4(1, 1, 1, 1);}
                    }
                    else if(_Angle >= 157.5 && _Angle <202.5){
                        if(center.x < a ){
                            return fixed4(intensity, intensity, intensity, 1);
                        }
                        else{return fixed4(1, 1, 1, 1);}
                    }
                    else if(_Angle >= 202.5 && _Angle <247.5){
                        if(center.x + center.y <= 2 *a){
                            return fixed4(intensity, intensity, intensity, 1);
                        }
                        else{return fixed4(1, 1, 1, 1);}
                    }
                    else if(_Angle >= 247.5 && _Angle <292.5){
                        if(center.y < a ){
                            return fixed4(intensity, intensity, intensity, 1);
                        }
                        else{return fixed4(1, 1, 1, 1);}
                    }
                    else if(_Angle >= 292.5 && _Angle <337.5){
                        if(center.x - center.y >= 1 - 2 *a ){
                            return fixed4(intensity, intensity, intensity,1);
                        }
                        else{return fixed4(1, 1, 1, 1);}
                    }
                    else{
                        if(center.x >= 1- a ){
                            return fixed4(intensity, intensity, intensity, 1);
                        }
                        else{return fixed4(1, 1, 1, 1);}
                    }


                    return fixed4(1, 1, 1, 1);
                }
                
            }
            ENDCG
        }
    }
}
