Shader "Fran/FloorCloudShadow"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                
                //Clouds
                float4 clouds : TEXCOORD3;
            };
            
            half4 _Color;
            
            //Samples
            sampler2D _WorldMask;
            
            //WorldScroll
            float4 _WorldScrollTillingA;
            float4 _WorldScrollTillingB;
            float _CloudStepA;
            float _CloudStepB;
            float _WorldCloudIntensity;
            float _WorldShadowCloudIntensity;
            float _WorldLightCloudIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                //Cloud Influence
                float4 cacheVertex = mul(unity_ObjectToWorld, v.vertex);
                half2 cloudsUVA = half2(cacheVertex.x + (_Time.y * _WorldScrollTillingA.z), cacheVertex.z + (_Time.y * _WorldScrollTillingA.w)) * half2(_WorldScrollTillingA.x * 0.01, _WorldScrollTillingA.y * 0.01);
                half2 cloudsUVB = half2(cacheVertex.x + (_Time.y * _WorldScrollTillingB.z), cacheVertex.z + (_Time.y * _WorldScrollTillingB.w)) * half2(_WorldScrollTillingB.x * 0.01, _WorldScrollTillingB.y * 0.01);
                o.clouds.xy = cloudsUVA; 
                o.clouds.zw = cloudsUVB;      
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                //Sample Clouds
                float cloudsA = tex2D(_WorldMask, i.clouds.xy).r;
                float cloudsB = tex2D(_WorldMask, i.clouds.zw).r;
                cloudsA = saturate(cloudsA);
                cloudsB = saturate(cloudsB);
                float clouds = cloudsA * cloudsB;
                clouds = saturate(clouds);
                clouds = smoothstep(_CloudStepA, _CloudStepB, clouds);
                
                half4 colorOut = _Color;
                return colorOut * clouds;
            }
            ENDCG
        }
    }
}
