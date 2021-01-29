// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/GrassWithCubeShadowOpaqueParticle"
{
    Properties
    {
        //Color Variation
        _BaseMap ("Base Texture", 2D) = "white" {} 
        _ColorDown("Color Down", Color) = (0,0,0,0)
        
        _GroundColorIntensity("Ground Color Intensity", Float) = 1
        _ParticleColorIntensity("Particle Color Intensity", Float) = 1
        _FixedObjectColor("Fixed Object Color", Color) = (0,0,0,0)
        _OverrideWindSpeed("Override Wind Speed", Float) = 0 
        
        //PlayerMask
        [Toggle]_PlayerMask("Use Player Influence", Int) = 0
        
        //Cube Shadow - Global Values
        //_CubeShadow ("Texture", 2D) = "white" {}
        
        //_WindDirection("Wind Direction", Vector) = (0,0,0,0)
        //_WindForce("Wind Force", Float) = 1
        //_WindSpeed("Wind Speed", Float) = 1
        //_FallDistance("Fall Distance", Float) = 1
        
        //Cloud Mask - Global Values
        //_WorldMask ("World Mask", 2D) = "white" {}
        //_WorldScrollTilling ("World Scroll Tilling", Vector) = (0,0,0,0)    
        //_WorldCloudIntensity("World Cloud Intensity", Float) = 0    
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry"}

        Pass
        {
            ZWrite On
            Cull Off
            //Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normal : NORMAL;
                float4 tangetID : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                
                //Color Variation
                float4 colorVariation : TEXCOORD1;
                
                half type : TEXCOORD2;

                //Clouds
                float4 clouds : TEXCOORD3;
                
                //CubeShadow
                float4 texCoordProj : TEXCOORD4;
            };
            
            //Color Variation
            float4 _ColorDown;
            half _GroundColorIntensity;
            half _ParticleColorIntensity;
            float4 _FixedObjectColor;

            //Grass
            float4 _WindDirection;
            float _WindForce;
            float _WindSpeed;
            float _FallDistance;
            
            //WorldScroll
            float4 _WorldScrollTillingA;
            float4 _WorldScrollTillingB;
            float _CloudStepA;
            float _CloudStepB;
            float _WorldCloudIntensity;
            float _WorldShadowCloudIntensity;
            float _WorldLightCloudIntensity;
            
            //Samples
            sampler2D _BaseMap;
            sampler2D _WorldMask;
            sampler2D _CubeShadow;
            
            //Projection Matrix
            float4x4 _ProjectionMatrix;
            float4x4 _CameraMatrix;
            
            //Others
            half _PlayerMask;
            half _OverrideWindSpeed;
            
            float4 TextureProjection(float4 v)
            {
                float4x4 modelMatrix = unity_ObjectToWorld;
                float4x4 constantMatrix = {0.5, 0, 0, 0.5, 0, 0.5, 0, 0.5, 0, 0, 0.5, 0.5, 0, 0, 0, 1};
                float4x4 textureMatrix;
                float4 modelForTexture = mul(modelMatrix, v);
                textureMatrix = mul(mul(constantMatrix, _ProjectionMatrix), _CameraMatrix);
                
                return  mul(textureMatrix, modelForTexture);
            }

            v2f vert (appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                
                //Cloud Influence
                float4 cacheVertex = mul(unity_ObjectToWorld, v.vertex);
                half2 cloudsUVA = half2(cacheVertex.x + (_Time.y * _WorldScrollTillingA.z), cacheVertex.z + (_Time.y * _WorldScrollTillingA.w)) * half2(_WorldScrollTillingA.x * 0.01, _WorldScrollTillingA.y * 0.01);
                half2 cloudsUVB = half2(cacheVertex.x + (_Time.y * _WorldScrollTillingB.z), cacheVertex.z + (_Time.y * _WorldScrollTillingB.w)) * half2(_WorldScrollTillingB.x * 0.01, _WorldScrollTillingB.y * 0.01);
                o.clouds.xy = cloudsUVA; 
                o.clouds.zw = cloudsUVB;        
                half cloudInfluence = tex2Dlod(_WorldMask, float4(o.clouds.xy,0,0)).r;

                o.uv = v.uv;

                //Players Mask
                o.texCoordProj = TextureProjection(v.vertex);
                float projection = 1-tex2Dlod(_CubeShadow, float4(o.texCoordProj.xy/o.texCoordProj.w,0,3)).r;
                float3 playerMask = float3(0, projection, 0);

                //Variation
                o.colorVariation = v.color;

                half vertexColorOffset = v.color.a;
                half vertexColorGround = v.uv.y;
                
                //Wind Direction
                float3 windDirection = mul(normalize(_WindDirection), unity_ObjectToWorld); 
                
              
                
                //Type of effect is store in the tangent X channel
                //grass = 0, particle = 1, particleFall = 2, fixedObject = 3
                //Tange YZW is used for pivot point
                //Remove if for lerp, same as fragment version
 
                if(v.tangetID.x == 0)
                {
                    v.vertex.y += 0;
                    //Floor
                    //Wind
                    float sinWind = sin((vertexColorOffset * 2 -1) + (_Time.y * _WindSpeed));
                    //Remap -1 1 to -0.1 1
                    windDirection = windDirection * sinWind;
                    windDirection = vertexColorGround * windDirection;
                    windDirection = windDirection * _WindForce;
                    windDirection *= max(0.5,cloudInfluence);
                    v.vertex.xyz = v.vertex.xyz + (vertexColorGround * windDirection);
                    
                    //Apply Player Mask
                    v.vertex.xyz -= (playerMask * _PlayerMask);
                }
                 
                if(v.tangetID.x == 1)
                {
                    v.vertex.y += 1;
                    //Particles
                    //Offset
                    half r = vertexColorOffset * 4 -4;

                    //Wind
                    half windDistance  = frac(((_Time.y + r) * _WindSpeed));
                    half distanceCache = windDistance;
                  
                    windDistance *= _WindForce * 2; // force
                    v.vertex.xyz += (windDirection * windDistance);
                    
                    half wave = sin(_Time.y  + r) * 2;
                    half3 yPos = half3(wave, wave, wave);
                    v.vertex.xyz += yPos;

                    half3 newPost = v.tangetID.yzw + (windDirection * windDistance);
                    newPost += yPos;
                    half scale = sin(distanceCache * 3.14);
                    v.vertex.xyz  = (v.vertex.xyz * scale) + (newPost * (1-scale));

                }
                
                if(v.tangetID.x == 2)
                {
                    v.vertex.y += 1;
                    //Offset
                    half r = vertexColorOffset * 4 -4;
                    _WindSpeed += _OverrideWindSpeed;
                    half downForce = frac((_Time.y + r) * _WindSpeed);
                    half windDistance  = frac((_Time.y + r) * 1) * _WindSpeed;
                     windDistance *= _WindForce * 2; // force
                    v.vertex.xyz += (windDirection * windDistance);
                    v.vertex.y -= downForce * _FallDistance;
                }
                
               

                o.type = v.tangetID.x; 
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //Cache Texture
                float3 baseMap = tex2D(_BaseMap, i.uv).rgb;
            
                //Sample Clouds
                float cloudsA = tex2D(_WorldMask, i.clouds.xy).r;
                float cloudsB = tex2D(_WorldMask, i.clouds.zw).r;
                cloudsA = saturate(cloudsA);
                cloudsB = saturate(cloudsB);
                float clouds = cloudsA * cloudsB;
                clouds = saturate(clouds);
                clouds = smoothstep(_CloudStepA, _CloudStepB, clouds);
                
                //Floor Gradient
                half gradientBlend = smoothstep(0.1,1, i.uv.y);
                float3 colorTop =  (i.colorVariation.rgb * i.colorVariation.a) * pow(2.0, _GroundColorIntensity);
                //make it Uniform Again
                colorTop = lerp(_ColorDown, colorTop, gradientBlend);
                
                //Floor or Particle
                half3 particleColor = lerp((i.colorVariation.rgb *  i.colorVariation.a) * pow(2.0, _ParticleColorIntensity), (i.colorVariation.rgb *  i.colorVariation.a),  max(0,i.type-1));
                half3 currentColor = lerp(colorTop,  particleColor, saturate(i.type));

                //Fall Particles 2
                currentColor = lerp(currentColor, (i.colorVariation.rgb), floor(clamp(0, 2, i.type) * 0.5));
  
                //Particle or Fixed 3
                currentColor = lerp(currentColor, (baseMap * _FixedObjectColor), max(0,i.type-2));

                half3 colH = max(currentColor, currentColor * _WorldLightCloudIntensity);
                half3 colS = min(currentColor, currentColor * (1-_WorldShadowCloudIntensity));
                
                currentColor = lerp(colS, colH, clouds);
                                
                //Cube Shadow
                half cubeShadow = tex2Dlod(_CubeShadow, float4(i.texCoordProj.xy/i.texCoordProj.w,0,3)).r;

                return half4(currentColor * cubeShadow, 1);
            }
            ENDCG
        }
    }
}
