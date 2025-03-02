Shader "Unlit/notSlashableShader"
{
    Properties
    {
        //パスでの変数にここで代入する
        _Color_Base ("Base Color", Color) = (0,0,0,1)
        _Color_Line ("Line Color", Color) = (1,1,1,1)
        _Color_Editor ("OnEditor Color", Color) = (0.8,0.8,0.8,1)
        _LineDepth("Line Depth",float) = 0.1
        _LineThickness("Line Thickness",float) = 0.1
    }

    SubShader
    {
        Tags{
            "RenderType" = "Opaque"
        }

        
        CGINCLUDE

        // 関数宣言
        #pragma vertex vert    // vert関数を頂点シェーダーとして使用する宣言
        #pragma fragment frag  // frag関数をフラグメントシェーダーとして使用する宣言

        // 変数宣言
        // Propertiesからここで受け取る
        fixed4 _Color_Base;
        fixed4 _Color_Line;
        fixed4 _Color_Editor;
        float _LineDepth;
        float _LineThickness;

        StructuredBuffer<float2> _Verticles;
        int _VertexCount;

        float2 GetOutlineNormal(uint id){
            int prevId = (id-1) % _VertexCount;
            int nextId = (id+1) % _VertexCount;

            //頂点
            float2 p0 = _Verticles[prevId];
            float2 p1 = _Verticles[id];
            float2 p2 = _Verticles[nextId];

            //辺方向
            float2 edge1 = normalize(p1 - p0);
            float2 edge2 = normalize(p2 - p1);

            //(x, y) => (y, -x) で90度回転
            float2 normal1 = float2(edge1.y, -edge1.x);
            float2 normal2 = float2(edge2.y, -edge2.x);

            //それぞれの辺から導出した法線の平均
            return normalize(normal1 + normal2);
        }

        ENDCG
        


        //エディタ(頂点情報代入前)の表示
        Pass
        {
            // プログラムを書き始めるという宣言
            CGPROGRAM

            // 頂点シェーダー
            float4 vert (float4 pos : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(pos);
            }

            // フラグメントシェーダー
            fixed4 frag () : SV_Target
            {
                return _Color_Editor;
            }

            ENDCG
            // プログラムを書き終わるという宣言
        }

        //一番外側
        Pass
        {
            CGPROGRAM

            float4 vert (uint id : SV_VertexID) : SV_POSITION
            {
                return UnityObjectToClipPos(float4(_Verticles[id],0,1));
            }

            fixed4 frag () : SV_Target
            {
                return _Color_Base;
            }

            ENDCG
        }
        
        //線の部分
        Pass
        {
            CGPROGRAM

            float4 vert (uint id : SV_VertexID) : SV_POSITION
            {
                float2 pos = _Verticles[id];
                float2 normal = GetOutlineNormal(id);
                pos += normal * _LineDepth;

                return UnityObjectToClipPos(float4(pos,0,1));
            }

            // フラグメントシェーダー
            fixed4 frag () : SV_Target
            {
                return _Color_Line;
            }

            ENDCG   // プログラムを書き終わるという宣言
        }

        

        //内側の部分
        Pass
        {
            CGPROGRAM

            float4 vert (uint id : SV_VertexID) : SV_POSITION
            {
                float2 pos = _Verticles[id];
                float2 normal = GetOutlineNormal(id);
                pos += normal * (_LineDepth + _LineThickness);

                return UnityObjectToClipPos(float4(pos,0,1));
            }

            // フラグメントシェーダー
            fixed4 frag () : SV_Target
            {
                return _Color_Base;
            }

            ENDCG   // プログラムを書き終わるという宣言
        }
        
    }
}