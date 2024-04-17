Shader "ImageEffect/AverageBlur"
{
	Properties
	{
		//主贴图
		_MainTex ("Texture", 2D) = "black" {}	
		//上一次同步的贴图
		_LastTex ("LastTexture", 2D) = "black" {}	
		_BlurRadius("BlurRadius",float) = 2
		_LerpRate("LerpRate",float) = 0.1
	}
	CGINCLUDE
	#include "UnityCG.cginc"
	struct v2f
	{
		float4 pos:SV_POSITION;
		float2 uv:TEXCOORD0;
	};
	sampler2D _MainTex;
	
	//纹素大小
	float4 _MainTex_TexelSize;		

	//模糊采样半径								
	float _BlurRadius;		
	float _LerpRate;
	sampler2D _LastTex;	
	v2f  vert (appdata_img v)
	{
		v2f  o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv=v.texcoord.xy;
		return o;
	}
	fixed4 frag2 (v2f i) : SV_Target		
	{
		//初始化色彩为黑色
		fixed4 col = fixed4(0,0,0,0);	
		float2 offset=_BlurRadius*_MainTex_TexelSize;
		half G[9]={		//设置卷积模板   此处是3*3的高斯模板
			1,2,1,
			2,4,2,
			1,2,1
		};
		for (int x=0;x<3;x++){	//进行3*3高斯模板的卷积（加权求平均值）
			for (int y=0;y<3;y++){
				col+=tex2D(_MainTex,i.uv+fixed2(x-1,y-1)*offset)*G[x*1+y*3];
			}
		}
		col=col/16;
		return col;
	} 	
	fixed4 frag (v2f i) : SV_Target	
	{
		//初始化颜色为黑色
		fixed4 col = fixed4(0,0,0,0);	

		//用offset保存 半径对应贴图的uv偏移量
		fixed2 offset=_BlurRadius*_MainTex_TexelSize;	

		//循环遍历周围像素点
		for (int x=0;x<3;x++){			
			for (int y=0;y<3;y++){
				col.a+=tex2D(_MainTex,i.uv+fixed2(x-1,y-1)*offset).a;	//全部加和
			}
		} 
		//取平均值
		col.a=col.a/9;
		return col;		
	} 
	fixed4 lerpFrag (v2f i) : SV_Target
	{
		half4 col1=tex2D(_MainTex,i.uv);
		half4 col2=tex2D(_LastTex,i.uv);

		//将前一次求得的迷雾贴图和当前求得的迷雾贴图混合
		return lerp(col2,col1,_LerpRate);
	} 
	ENDCG
	SubShader
	{
		Tags { "Queue" = "Transparent+20" "RenderType"="Transparent""IgnoreProjector"="True"}
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM	
			#pragma vertex vert	
			#pragma fragment frag2
			ENDCG		
		}
		Pass
		{
			CGPROGRAM	
			#pragma vertex vert	
			#pragma fragment lerpFrag
			ENDCG
		}
	}
}