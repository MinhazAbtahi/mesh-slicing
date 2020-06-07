Shader "Custom/Roll"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_StartAngle("Start Angle (Radians)", float) = 60.0
		_AnglePerUnit("Radians per Unit", float) = 0.2
		_Pitch("Pitch", float) = 0.02
		_UnrolledAngle("Unrolled Angle", float) = 1.5
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			Cull Off

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows vertex:vert

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
				float3 viewDir;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			float _StartAngle;
			float _AnglePerUnit;
			float _Pitch;
			float _UnrolledAngle;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			float arcLengthToAngle(float angle) {
				float radical = sqrt(angle * angle + 1.0f);
				return _Pitch * 0.5f * (angle * radical + log(angle + radical));
			}

			void vert(inout appdata_full v) {
				float fromStart = v.vertex.z * _AnglePerUnit;

				float fromOrigin = _StartAngle - fromStart;
				float lengthToHere = arcLengthToAngle(fromOrigin);
				float lengthToStart = arcLengthToAngle(_StartAngle);

				v.texcoord.y = lengthToStart - lengthToHere;

				if (fromStart < _UnrolledAngle) {
					float lengthToSplit = arcLengthToAngle(_StartAngle - _UnrolledAngle);
					v.vertex.z = lengthToSplit - lengthToHere;
					v.vertex.y = 0.0f;
					v.normal = float3(0, 1, 0);
				}
				else {
					float radiusAtSplit = _Pitch * (_StartAngle - _UnrolledAngle);
					float radius = _Pitch * fromOrigin;

					float shifted = fromStart - _UnrolledAngle;

					v.vertex.y = radiusAtSplit - cos(shifted) * radius;
					v.vertex.z = sin(shifted) * radius;

					v.normal = float3(0, cos(shifted), -sin(shifted));
				}
			}


			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;

				o.Normal *= sign(dot(IN.viewDir, o.Normal));
			}
			ENDCG
		}
			FallBack "Diffuse"
}