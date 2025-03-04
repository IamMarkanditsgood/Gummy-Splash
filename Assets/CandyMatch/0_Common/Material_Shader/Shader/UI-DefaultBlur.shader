// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/DefaultBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_GradTex("Grad Texture", 2D) = "white" {}
		_blurSizeXY("BlurSizeXY", Range(0.01,5)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

		GrabPass {}

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

			sampler2D _GrabTexture : register(s0);
			float _blurSizeXY;
			float4 _GrabTexture_TexelSize;
			sampler2D _MainTex;
			sampler2D _GradTex;
			float4 _MainTex_ST; 

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex); //make sure texture scale and offset is applied correctly.
                OUT.color = v.color * _Color;
				OUT.screenPos = ComputeGrabScreenPos(OUT.vertex);
                return OUT;
            }


            float4 frag(v2f IN) : SV_Target
            {
				float4 sum = half4(0.0f,0.0f,0.0f,0.0f);
				float a = tex2D(_GradTex, float2(0.5f, IN.texcoord.y)).r;
				float si = _blurSizeXY;
				float sqr2si = 2.0f * si * si;
				float k_sum = 0.0f;
				for (int r = -2; r < 3; r++)
				{
					for (int c = -2; c < 3; c++)
					{
						k_sum += exp((-(c*c) - (r*r)) / sqr2si);
					}
				}
				k_sum = 1.0 / k_sum;
				for (int i_r = -2; i_r < 3; i_r++)
				{
					float i_rf = (float)i_r;
					float sq_ir = i_rf*i_rf;
					for (int i_c = -2; i_c < 3; i_c++)
					{
						float i_cf = (float)i_c;
						float sq_ic = i_cf*i_cf;
						// sum += tex2Dproj(_GrabTexture, float4(IN.screenPos.x + i_rf *_GrabTexture_TexelSize.x * _blurSizeXY, IN.screenPos.y + i_cf *_GrabTexture_TexelSize.y * _blurSizeXY, IN.screenPos.z, IN.screenPos.w)) * exp((-sq_ir - sq_ic) / sqr2si);
						//sum += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(float4(IN.uvgrab.x + i_rf *_GrabTexture_TexelSize.x * _blurSizeXY, IN.uvgrab.y + i_cf *_GrabTexture_TexelSize.y*_blurSizeXY, IN.uvgrab.z, IN.uvgrab.w))) * exp((-sq_ir - sq_ic) / sqr2si)*k_sum;
						 sum += tex2D(_GrabTexture, float2(IN.screenPos.x + i_rf *_GrabTexture_TexelSize.x *a*_blurSizeXY, IN.screenPos.y  + i_cf *_GrabTexture_TexelSize.y *a*_blurSizeXY)) * exp((-sq_ir - sq_ic) / sqr2si);
					}
				}
				sum.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				return sum*k_sum;
            }
        ENDCG
        }
    }
}
