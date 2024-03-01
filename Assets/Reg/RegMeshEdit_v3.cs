using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace RegUtil
{
    namespace RegMeshEdit_v3
    {
        public class MeshCut
        {
            /// <summary>
            /// PolygonCollider2D���A�^�b�`����Ă���I�u�W�F�N�g�𒼐��Őؒf����B
            /// </summary>
            /// <param name="obj">�ؒf����Ώۂ̃I�u�W�F�N�g</param>
            /// <param name="lineStart">�ؒf���̎n�_�@���K���I�u�W�F�N�g�O�ł��邱��</param>
            /// <param name="lineEnd">�ؒf���̏I�_</param>
            /// <returns>�ؒf�����ꍇ��GameObject��List�A���Ȃ������ꍇ��null��Ԃ��B</returns>
            public static List<GameObject> Cut(GameObject obj, Vector3 lineStart, Vector3 lineEnd)
            {
                var line = new Line_P() { start = lineStart, end = lineEnd };
                var results = new List<GameObject>();

                var polygon = obj.GetComponent<PolygonCollider2D>();
                var polygons = CutPolygon(polygon, line);

                foreach (var part in polygons)
                {
                    var obj_ = CreateMeshObject.Create2D(part.ToArray(),
                                                         obj.transform,
                                                         obj.GetComponent<MeshRenderer>().material);
                    results.Add(obj_);
                }

                if (results.Count == 1)
                {
                    GameObject.Destroy(results[0]);
                    return null;
                }

                GameObject.Destroy(obj);

                return results;
            }

            //========================================================================================================
            public class MeshDataLists
            {
                public List<Vector3> verticles = new List<Vector3>();
                public List<int> indexes = new List<int>();

                /// <summary>
                /// ���X�g�ɎO�p�`��ǉ�
                /// </summary>
                /// <param name="positions">�O�p�`�̊e���_( ���v���ɁI�I�I )</param>
                public void AddTriangle(params Vector3[] positions)
                {
                    if (positions.Length != 3) Debug.LogError("Not Triangle!!");

                    //�����v���̏ꍇ�A�z��𔽓]������
                    if (IsClockWise(positions[0], positions[1], positions[2])) Array.Reverse(positions);

                    foreach (var pos in positions)
                    {
                        if (!verticles.Contains(pos)) verticles.Add(pos);

                        indexes.Add(verticles.IndexOf(pos));
                    }
                }

                public Mesh toMesh()
                {
                    var mesh = new Mesh();

                    mesh.vertices = verticles.ToArray();
                    mesh.triangles = indexes.ToArray();

                    return mesh;
                }
            }

            //========================================================================================================

            /// <summary>
            /// ����a�Ɛ���b���������Ă��邩�ǂ����𔻒肷��
            /// </summary>
            /// <param name="a">����a</param>
            /// <param name="b">����b</param>
            /// <returns></returns>
            static bool IsCrossing_Separate(Line_P a, Line_P b)
            {
                bool crossA = IsClockWise(a.start, a.end, b.start) != IsClockWise(a.start, a.end, b.end);
                bool crossB = IsClockWise(b.start, b.end, a.start) != IsClockWise(b.start, b.end, a.end);

                return crossA && crossB;
            }

            /// <summary>
            /// ����a�Ɛ���b�̌�_(�������Ȃ��ꍇ��Vector2.zero��Ԃ�)
            /// </summary>
            /// <param name="a">����a</param>
            /// <param name="b">����b</param>
            /// <returns></returns>
            static Vector2 GetCrossingPoint(Line_P a, Line_P b)
            {
                if (!IsCrossing_Separate(a, b)) { Debug.LogWarning("not crossing!"); return Vector2.zero; }

                var s1 = Mathf.Abs(CrossFrom3Point(a.start, a.end, b.end));

                var s2 = Mathf.Abs(CrossFrom3Point(a.start, a.end, b.start));

                float sum = s1 + s2;

                //Debug.Log("s1=" + s1 + " | s2=" + s2 + " | sum=" + sum + " ratio=" + (s1 / sum));

                return Vector2.Lerp(b.start, b.end, s2 / sum);
            }

            /// <summary>
            /// PolygonCollider������Ő؂�
            /// �����̏I�_�͐}�`���ł����������̎n�_�͕K���}�`�O�ł��邱��
            /// </summary>
            /// <param name="polygonCol">PolygonCollider2D</param>
            /// <param name="line">����</param>
            /// <returns>points�ɂ�����Vector2�̓񎟌����X�g</returns>
            static public List<List<Vector2>> CutPolygon(PolygonCollider2D polygonCol, Line_P line)
            {
                var tra = polygonCol.transform;
                var resultsLocal = new List<List<Vector2>>();

                //���[���h���W�ɕϊ��������_�f�[�^�z��
                var polygonWorld = Array.ConvertAll(polygonCol.points, (vec2) => (Vector2)tra.TransformPoint(vec2));


                var crossingPairIndexList = new List<(int a, int b, int count)>();
                var polygonWorldWithCrossingPoints = new List<Vector2>();
                var crossingPointsIndex = new List<int>();


                var side = new Line_P();//�ЂƂO�̓_�����݂̓_�@�̐���
                for (int i = 0; i < polygonWorld.Length; i++)
                {
                    side.end = polygonWorld[i];
                    side.start = (i == 0) ? polygonWorld[polygonWorld.Length - 1] : polygonWorld[i - 1];

                    if (IsCrossing_Separate(line, side))
                    {
                        polygonWorldWithCrossingPoints.Add(GetCrossingPoint(line, side));
                        crossingPointsIndex.Add(polygonWorldWithCrossingPoints.Count - 1);
                    }
                    //��_�͑O�̓_���獡�̓_�̊Ԃɂ���̂�[��_�����̓_]�̏��Œǉ����ׂ�
                    polygonWorldWithCrossingPoints.Add(polygonWorld[i]);
                }//��_��}���������_�z��ƌ�_�̃C���f�b�N�X�ԍ����X�g���쐬

                //Debug.Log("�������_���F" + polygonWorldWithCrossingPoints.Count);
                //Debug.Log("��_���F" + crossingPointsIndex.Count);
                //Debug.Log(string.Join(",", crossingPointsIndex.Select(n => n.ToString())));

                if (crossingPointsIndex.Count == 0)
                {
                    resultsLocal.Add(new List<Vector2>(polygonCol.points));
                    return resultsLocal;
                }//�������Ȃ��ꍇ


                Func<float, float, int> CompareFloat = (a, b) => a.CompareTo(b);
                Func<int, float> GetLerpValue = (targetIndex)
                    => Vector2.Distance(line.start, polygonWorldWithCrossingPoints[targetIndex]) / Vector2.Distance(line.start, line.end);
                //��_�C���f�b�N�X�ԍ����X�g��ؒf�����start��end�̏��ɕ��בւ���
                crossingPointsIndex.Sort((a, b) => CompareFloat(GetLerpValue(a), GetLerpValue(b)));

                if (crossingPointsIndex.Count % 2 != 0) crossingPointsIndex.RemoveAt(crossingPointsIndex.Count - 1);

                //��_�̃y�A(�}�`�ɓ��o�̂Q�_)������
                for (int i = 0; i < crossingPointsIndex.Count; i += 2)
                    crossingPairIndexList.Add((crossingPointsIndex[i], crossingPointsIndex[i+1], 0));

                //Debug.Log(string.Join(",", crossingPairIndexList.Select(n => n.ToString())));


                var resultsWorld = new List<List<Vector2>>();
                var currentResult = new List<int>();
                var clearIndex = new List<int>();//�����ς݂̒��_�̃C���f�b�N�X�ԍ����X�g
                Func<int, (int a,int b,int count)> FindPair = (n) =>
                 {
                     for (int i=0; i<crossingPairIndexList.Count;i++)
                     {
                         var point = crossingPairIndexList[i];

                         if (point.a == n || point.b == n)
                         {
                             //Debug.Log("pair founded." + point);
                             return point;
                         }
                     }

                     //Debug.Log("pair not founded: "+n);

                     return (-1,-1,-1);
                 };
                Func<int, int> GetOtherSidePair = (n) =>
                 {
                     for (int i=0; i<crossingPairIndexList.Count;i++)
                     {
                         var point = crossingPairIndexList[i];

                         if (point.a == n || point.b == n)
                         {
                             crossingPairIndexList[i] = (point.a, point.b, point.count + 1);

                             //Debug.Log("pair otherside founded. "+ n + "=>" + crossingPairIndexList[i]);

                             if (point.a == n) return point.b;
                             if (point.b == n) return point.a;
                         }
                     }

                     //Debug.Log("pair otherside not founded.");

                     return -1;
                 };
                Func<bool> clossingPairRemaining = () =>
                {
                    bool remains = false;

                    foreach (var sLine in crossingPairIndexList)
                    {
                        if (sLine.count < 2){
                            remains = true;
                            break;
                        }//�ؒf���A�y�A�͕K���ŏI�I��2��g����
                    }
                    return remains;
                };
                Action AddResult = () =>
                {
                    if (currentResult.Count < 3) return;

                    resultsWorld.Add(new List<Vector2>(Array.ConvertAll(currentResult.ToArray(), (n) => polygonWorldWithCrossingPoints[n])));
                    //Debug.LogWarning("result" + resultsWorld.Count + ": " + string.Join(",", currentResult.Select(n => n.ToString())));

                    clearIndex.AddRange(currentResult.Where(n =>  
                                                                     !crossingPointsIndex.Contains(n)
                                                                  && !clearIndex.Contains(n)
                                                            ).ToList());
                };


                int loopCount = 0;
                bool lastLoopWentOtherSideOfPair = false;//�O��̎��Ƀy�A�Ԉړ���������

                for (int i = 0; clearIndex.Count < polygonWorldWithCrossingPoints.Count;)
                {

                    var point = polygonWorldWithCrossingPoints[i];
                    var pair = FindPair(i);

                    //Debug.Log(i+" , "+point+"\r\n"+Time.deltaTime);

                    //�ւ��Ȃ������Ƃ��̏���
                    if (currentResult.Contains(i))
                    {
                        //Debug.Log(i + " is contain in currentresult.");

                        AddResult();

                        currentResult = new List<int>();
                    }
                    else  currentResult.Add(i);




                    if (loopCount++ >= 1000) { Debug.LogError("endress detected."); break; }



                    if (!lastLoopWentOtherSideOfPair && pair.count>=0 && pair.count< 2 && currentResult.Count != 1)//��_�������ꍇ
                    {
                        i = GetOtherSidePair(i);
                        lastLoopWentOtherSideOfPair = true;
                    }
                    else
                    {

                        if (pair.count >= 2)
                        {
                            //Debug.Log("pair cleared." + pair);
                            //Debug.DrawLine(polygonWorldWithCrossingPoints[pair.a], polygonWorldWithCrossingPoints[pair.b], Color.cyan,1000f,false);

                            if (!clearIndex.Contains(pair.a)) clearIndex.Add(pair.a);
                            if (!clearIndex.Contains(pair.b)) clearIndex.Add(pair.b);
                            crossingPointsIndex.Remove(pair.a);
                            crossingPointsIndex.Remove(pair.b);
                            crossingPairIndexList.Remove(pair);

                        }//�Q��g�����y�A�����O����

                        

                        if (clearIndex.Count < polygonWorldWithCrossingPoints.Count)
                        {
                            do
                            {
                                i++;
                                if (i >= polygonWorldWithCrossingPoints.Count) i = 0;//�C���f�b�N�X�����[�v�����鏈��

                                point = polygonWorldWithCrossingPoints[i];

                            } while (clearIndex.Contains(i));//�����ς݂̒��_�̓X���[
                        }

                        if(!crossingPointsIndex.Contains(i)) lastLoopWentOtherSideOfPair = false;
                    }

                    
                }//�؂蕪����

                AddResult();

                clearIndex.Sort();
                //Debug.LogWarning("clears: " + string.Join(",", clearIndex.Select(n => n.ToString())));
                //Debug.LogWarning("clearsCount: " + clearIndex.Count);
                //Debug.LogWarning("polygonCount: "+polygonWorldWithCrossingPoints.Count);


                foreach (var part in resultsWorld)
                {
                    var localArray = Array.ConvertAll(part.ToArray(), (world) => (Vector2)tra.InverseTransformPoint(world));
                    resultsLocal.Add(new List<Vector2>(localArray));
                }//���[�J�����W�ɕϊ�

                return resultsLocal;
            }


            //========================================================================================================


            /// <summary>
            /// �����̎n�_�ƏI�_�A�ڕW�̍��W����ڕW�������ɑ΂��Ď��v���̈ʒu�ɂ��邩�ǂ����𔻒肷��
            /// </summary>
            /// <param name="start"></param>
            /// <param name="end"></param>
            /// <param name="target"></param>
            /// <returns></returns>
            static public bool IsClockWise(Vector2 start, Vector2 end, Vector2 target)
            {
                return CrossFrom3Point(start, end, target) > 0;
            }

            /// <summary>
            /// 3�̓_����x�N�g���̊O�ς����߂�
            /// a��b��c
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <param name="c"></param>
            /// <returns></returns>
            static float CrossFrom3Point(Vector2 a, Vector2 b, Vector2 c)
            {
                var A = b - a;
                var B = c - b;

                return CrossFrom2Vec2(A, B);
            }

            /// <summary>
            /// 2�̓񎟌��x�N�g������O�ς̑傫�������߂�
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            static public float CrossFrom2Vec2(Vector2 a, Vector2 b)
            {
                return a.x * b.y - a.y * b.x;
            }

            //========================================================================================================

            public struct Line_P
            {
                public Vector3 start, end;
            }
        }

        public class CreateMeshObject
        {
            public static GameObject Create2D(Vector2[] polygon, Transform tra, Material material, params System.Type[] components)
            {
                return Create2D(polygon, tra, "meshObject", material, components);
            }
            public static GameObject Create2D(Vector2[] polygon, Transform tra, string name, Material material, params System.Type[] components)
            {
                if (polygon.Length < 3) return null;

                var obj = new GameObject(name);
                obj.name = name;

                obj.AddComponent<MeshFilter>().mesh = Convert.PolygonToMesh(polygon);
                obj.AddComponent<MeshRenderer>().material = material;
                obj.AddComponent<PolygonCollider2D>().points = polygon;

                obj.transform.position = tra.position;
                obj.transform.rotation = tra.rotation;
                obj.transform.localScale = tra.localScale;

                foreach (var comp in components) obj.AddComponent(comp);

                return obj;
            }
        }

        public class Convert
        {
            /// <summary>
            /// ���p�`���O�p�`�̏W���ɕ�����mesh�ɂ��ĕԂ�
            /// </summary>
            /// <param name="polygon">���p�`�̒��_���X�g(���v���)</param>
            static public Mesh PolygonToMesh(List<Vector2> polygon)
            {
                return PolygonToMesh(polygon.ToArray());
            }

            /// <summary>
            /// ���p�`���O�p�`�̏W���ɕ�����mesh�ɂ��ĕԂ�
            /// </summary>
            /// <param name="polygon">���p�`�̒��_���X�g(���v���)</param>
            static public Mesh PolygonToMesh(Vector2[] polygon)
            {
                var list = new List<Vector2>(polygon);
                var meshdata = new MeshCut.MeshDataLists();

                var origin = Vector2.zero;
                while (list.Count > 2)
                {
                    var far = (index: -1, distance: 0f);
                    foreach (var point in list)
                    {
                        float distance = Vector2.Distance(origin, point);
                        if (distance > far.distance)
                            far = (list.IndexOf(point), distance);
                    }//�ł����_���牓���_��T��

                    var (a, b, c) = (Vector2.zero, Vector2.zero, Vector2.zero);
                    Action SetTriangle = () =>
                    {
                        //Debug.Log("far.index=" + far.index + "  list.count=" + list.Count);

                        if (far.index >= list.Count)
                        {
                            far.index = 0;
                            (a, b, c) = (list[list.Count - 1], list[far.index], list[far.index + 1]);
                        }
                        else if (far.index == list.Count - 1)
                        {
                            (a, b, c) = (list[far.index - 1], list[far.index], list[0]);
                        }
                        else if (far.index <= 0)
                        {
                            (a, b, c) = (list[list.Count - 1], list[far.index], list[far.index + 1]);
                        }
                        else (a, b, c) = (list[far.index - 1], list[far.index], list[far.index + 1]);
                    };
                    SetTriangle();
                    var lastTriangleDirection = MeshCut.CrossFrom2Vec2(a - b, c - b) > 0;

                    int loopCount = 0;
                    while (true)
                    {
                        bool noPointsInTriangle = true;

                        foreach (var point in list)
                        {
                            if (IsinTriangle(a, b, c, point))
                            {
                                noPointsInTriangle = false;

                                lastTriangleDirection = MeshCut.CrossFrom2Vec2(a - b, c - b) > 0;

                                far.index++;
                                SetTriangle();

                                break;
                            }//�O�p�`�ɓ���_������ꍇ�A�̗p�_��1���炷
                        }//�O�p�`�̒��ɓ���_���Ȃ��ꍇ�A���̂܂ܑ��s

                        //�̗p�_�����炵���ꍇ���̗̍p�_�̂܂܎��̃��[�v�ɓ���(noPointsInTriangle��false�ɂȂ��Ă��邽��)

                        if (noPointsInTriangle)
                        {
                            if (lastTriangleDirection == MeshCut.CrossFrom2Vec2(a - b, c - b) > 0)
                            {
                                //Debug.Log("�O�p�`�m��(loop:"+loopCount+")");
                                break;
                            }

                            //Debug.Log("no points in triangle(loop:"+loopCount+")");
                        }

                        if (loopCount++ >= polygon.Length + 5)
                        {
                            //Debug.LogError("�������[�v�Ɋׂ������ߋ����I�ɏ����𒆒f���܂���");
                            break;
                        }

                    }//����_�`�F�b�N���O�p�`�m��

                    meshdata.AddTriangle(a, b, c);
                    list.RemoveAt(far.index);
                }

                return meshdata.toMesh();
            }

            static bool IsinTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 target)
            {
                var abt = MeshCut.IsClockWise(a, b, target);
                var bct = MeshCut.IsClockWise(b, c, target);
                var cat = MeshCut.IsClockWise(c, a, target);

                return (abt == bct && bct == cat);
            }
        }
    }
}