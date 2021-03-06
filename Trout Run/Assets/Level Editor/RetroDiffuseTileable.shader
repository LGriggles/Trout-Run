﻿Shader "Alex's Banging Shaders/Retro Diffuse Tileable" 
{
	Properties
	{
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Diffuse Texture", 2D) = "white" {}
		_OffsetNudge ("Nudge Offset", vector) = (0.0, 0.0, 0.0, 0.0)
	}
	
	SubShader
	{
		Pass
		{
			Tags{"LightMode" = "ForwardBase"}
		
			CGPROGRAM

			// pragmas
			#pragma vertex Vert
			#pragma fragment Frag

			// user defined variables
			uniform float4 _Color;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _OffsetNudge;
			
			// unity defined variables
			uniform float4 _LightColor0;
			 
			// base input structs
			struct vertexInput
			{
				float4 vertex : POSITION; // the "semantic" is the thing we're reading from or writing to
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			};
			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 col : COLOR; // semantic COLOR = we will write over the vert's colour with speciified colour
				float4 tex : TEXCOORD0;
				float2 offs : TEXCOORD1;
			};

			// vertex function - all vertex manipulation here and return as output
			vertexOutput Vert(vertexInput input)
			{
				float3 lightDirection;
				float atten;
			
				if(_WorldSpaceLightPos0.w == 0) // directional light
				{
					lightDirection = normalize(_WorldSpaceLightPos0.xyz); // Another unity defined var, the position of the light
					atten = 1.0; // based on distance to light
				}
				else // point light
				{
					float3 vertToLight = _WorldSpaceLightPos0.xyz - mul(_Object2World, input.vertex).xyz; // Vector from vert to light in world space
					float mag = length(vertToLight); // magnitude of that vector, e.g. distance to light
					atten = 1.0 / mag; // what if mag == 0? Does it ever?
					lightDirection = normalize(vertToLight);
				}
			
				// Work out shit based on normals, boy
				float3 normalDirection = normalize(mul(float4(input.normal, 0.0), _World2Object).xyz);// normal in object space
				
				
				//float3 diffuseReflection = max(0.0, dot(normalDirection, lightDirection)); // i think this returns dot product and sets to all 3 components of float3
				//diffuseReflection *= atten * _Color.xyz * _LightColor0.xyz; // factor in atten and diffuse colour adn light colour
				//diffuseReflection = UNITY_LIGHTMODEL_AMBIENT.xyz;
				float3 diffuseReflection = (max(0.0, dot(normalDirection, lightDirection)) * atten * _LightColor0.xyz) + UNITY_LIGHTMODEL_AMBIENT.xyz;
				diffuseReflection *= _Color.xyz;
				
				// The output properties
				vertexOutput output;
				output.pos = mul(UNITY_MATRIX_MVP, input.vertex); // model * view * projection matrix
				output.col = float4(diffuseReflection, 1.0);
	
				output.offs = input.texcoord1; // debug set to an offset to see if it works
				output.tex = input.texcoord;// * input.texcoord1;
				
				return output;
			}
			
			
			//fragment function - takes vertex output and all fragement shading here
			float4 Frag(vertexOutput vert) : COLOR
			{
				// WHAT'S GOING ON???
				// Tex2D looks up  a colour value from a sampler (first arg) and returns that colour as a float4
				// The argument specifies the colour in the same way a UV would - from 0,0 to 1,1
				// Presumably by using the texcoord value it automatically interpolates to find correct pixel (based on UV coordinates at verts)
				
				// fmod(x, y) is the same as doing x % y - modulus operator
				// So it's basically saying if first value is greater than second it will be itself - second value
				// E.g. (0.6, 0.5) = 0.1, (1.1, 0.5) also = 0.1 (like how you use %, confusing as decimals though!!)
				// MainTex_ST is the tiling and offset - tiling will be worked out by editor script but both can be messed about with manually if editor disabled
				
				
				
				
				// Work out coord as a tex coord as a scaled, repeating square at 0,0. So if the tile scale is 0.5, 0.5 (2X2 tiles)
				// then this square would be 0,0 -> 0.5,0.5 (tile scale = _MainTex_ST.xy) Tile Offset also applied (ST.zw)
				float2 coord;
				coord.x = fmod(abs(vert.tex.x - _MainTex_ST.z), _MainTex_ST.x);
				coord.y = fmod(abs(vert.tex.y - _MainTex_ST.w), _MainTex_ST.y);
				
				// Now add offset to coord. We clamp to offset plus nudge on either side so we can manually cater for overspill
				coord.x = clamp(coord.x + vert.offs.x, vert.offs.x + _OffsetNudge.x, vert.offs.x + _MainTex_ST.x - _OffsetNudge.z); // must be at least xMin
				coord.y = clamp(coord.y + vert.offs.y, vert.offs.y + _OffsetNudge.y, vert.offs.y + _MainTex_ST.y - _OffsetNudge.w); // must be at least xMin
				
				// Finally, look up coord in the texture to find colour value
				float4 tex = tex2D(_MainTex, coord); // now look up that square + tilemap offset, e.g. to find specific tile as specified in editor
				
				return tex * vert.col * _Color;
			}
			
			
			ENDCG
		}
		
	}
}