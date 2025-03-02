Shader "Unlit/notSlashableShader"
{
    Properties
    {
        //�p�X�ł̕ϐ��ɂ����ő������
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

        // �֐��錾
        #pragma vertex vert    // vert�֐��𒸓_�V�F�[�_�[�Ƃ��Ďg�p����錾
        #pragma fragment frag  // frag�֐����t���O�����g�V�F�[�_�[�Ƃ��Ďg�p����錾

        // �ϐ��錾
        // Properties���炱���Ŏ󂯎��
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

            //���_
            float2 p0 = _Verticles[prevId];
            float2 p1 = _Verticles[id];
            float2 p2 = _Verticles[nextId];

            //�ӕ���
            float2 edge1 = normalize(p1 - p0);
            float2 edge2 = normalize(p2 - p1);

            //(x, y) => (y, -x) ��90�x��]
            float2 normal1 = float2(edge1.y, -edge1.x);
            float2 normal2 = float2(edge2.y, -edge2.x);

            //���ꂼ��̕ӂ��瓱�o�����@���̕���
            return normalize(normal1 + normal2);
        }

        ENDCG
        


        //�G�f�B�^(���_������O)�̕\��
        Pass
        {
            // �v���O�����������n�߂�Ƃ����錾
            CGPROGRAM

            // ���_�V�F�[�_�[
            float4 vert (float4 pos : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(pos);
            }

            // �t���O�����g�V�F�[�_�[
            fixed4 frag () : SV_Target
            {
                return _Color_Editor;
            }

            ENDCG
            // �v���O�����������I���Ƃ����錾
        }

        //��ԊO��
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
        
        //���̕���
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

            // �t���O�����g�V�F�[�_�[
            fixed4 frag () : SV_Target
            {
                return _Color_Line;
            }

            ENDCG   // �v���O�����������I���Ƃ����錾
        }

        

        //�����̕���
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

            // �t���O�����g�V�F�[�_�[
            fixed4 frag () : SV_Target
            {
                return _Color_Base;
            }

            ENDCG   // �v���O�����������I���Ƃ����錾
        }
        
    }
}