using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class MethodCollection
{

    // [1. 피셔-예이츠 리스트 셔플]
    // 사용법 : 리스트 뒤에 .Shuffle()
    private static System.Random random = new System.Random();
    public static void Shuffle<T>(this IList<T> values)
    {
        for (int i = values.Count - 1; i > 0; i--)
        {
            int k = random.Next(i + 1);
            T value = values[k];
            values[k] = values[i];
            values[i] = value;
        }
    }
    public static T[] Shuffle<T>(T[] array, int seed)
    {
        System.Random prng = new System.Random(seed);
        for (int i = array.Length - 1; i > 0; i--)
        {
            // prng 인스턴스를 사용하여 독립적인 난수를 생성합니다.
            int k = prng.Next(0, i + 1);

            // Fisher-Yates 셔플 알고리즘
            T value = array[k];
            array[k] = array[i];
            array[i] = value;
        }
        return array;
    }
    
    // [2. length길이의 랜덤 문자열생성]
    //사용법 : string str = RandomString(8);
    private const string VALID_CHARS = "0123456789abcdefghijklmnopqrstuvwxyz";
    public static string RandomString(int length)
    {
        var sb = new System.Text.StringBuilder(length);
        var r = new System.Random();
        for (int i = 0; i < length; i++)
        {
            int pos = r.Next(VALID_CHARS.Length);
            char c = VALID_CHARS[pos];
            sb.Append(c);
        }
        return sb.ToString();
    }
    // [3. 이름앞에 ---가 붙은걸 제외한 최상위 루트를 찾아주는 메소드]
    // 사용법 : 콜라이더나 트랜스폼 뒤에 .Root()
    public static Transform Root(this Collider x)
    {
        if (x == null) return null;
        Transform result;
        if (x.transform.parent == null)
            result = x.transform;
        else
            result = x.transform.root;
        if (result == null) result = x.transform;
        if (result.name.Substring(0, 3) != "---") return result;
        List<Transform> trs = x.transform.GetComponentsInParent<Transform>().ToList();
        foreach (var tr in trs)
        {
            if (tr.parent == result)
            {
                result = tr;
                break;
            }
        }
        return result;
    }
    public static Transform Root(this Transform x)
    {
        Transform result = x.transform.root;
        if (result == null) result = x.transform;
        if (result.name.Substring(0, 3) != "---") return result;
        List<Transform> trs = x.transform.GetComponentsInParent<Transform>().ToList();
        foreach (var tr in trs)
        {
            if (tr.parent == result)
            {
                result = tr;
                break;
            }
        }
        return result;
    }
    // [4. 공간상에서 두 Ray가 만나는 교점을 찾는 메소드]
    // 리턴 값이 (-999, -999, -999) 라는건 두 Ray가 공간상에서 서로 안겹친다는 의미
    public static Vector3 RayRayIntersection(Vector3 ray1_origin, Vector3 ray1_dir, Vector3 ray2_origin, Vector3 ray2_dir, bool oneside = true)
    {
        Vector3 result = new Vector3(-999, -999, -999);
        ray1_dir.Normalize();
        ray2_dir.Normalize();
        Vector3 w0 = ray1_origin - ray2_origin;
        float a = Vector3.Dot(ray1_dir, ray1_dir); // 1
        float b = Vector3.Dot(ray1_dir, ray2_dir);
        float c = Vector3.Dot(ray2_dir, ray2_dir); // 1
        float d = Vector3.Dot(ray1_dir, w0);
        float e = Vector3.Dot(ray2_dir, w0);
        float denominator = a * c - b * b;
        // 두 직선이 거의 평행한 경우
        if (Mathf.Abs(denominator) < 1e-6f)
        {
            if ((Vector3.Cross(ray1_dir, ray2_dir).magnitude < 1e-6f)
            && (Vector3.Cross(ray1_origin - ray2_origin, ray1_dir).magnitude < 1e-6f))
            {
                return ray1_origin; // 두 직선이 완전히 포개지는경우 -> 무한히 많은 교점이 생기지만 origin 하나만 리턴시켰음
            }
            return result; // 두직선이 평행하면서 교차점 없음
        }
        float s = (b * e - c * d) / denominator;
        float t = (a * e - b * d) / denominator;
        // 단방향일 경우: 레이의 앞 방향만 유효하게 (레이의 뒤쪽으로 생기는 직선은 교차점으로 취급 안하는 설정)
        if (oneside && (s < 0 || t < 0))
            return result;
        Vector3 point1 = ray1_origin + s * ray1_dir;
        Vector3 point2 = ray2_origin + t * ray2_dir;
        if (Vector3.Distance(point1, point2) > 1e-3f)
            return result;
        return (point1 + point2) / 2f;
    }
    // [5. 선분 p1 p2 와 선분 p3 p4 사이의 최단거리]
    public static float MinDistanceBetweenSegments(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        Vector3 d1 = p2 - p1; // 선분 1의 방향 벡터
        Vector3 d2 = p4 - p3; // 선분 2의 방향 벡터
        Vector3 r = p1 - p3;  // p1에서 p3으로 향하는 벡터

        float a = Vector3.Dot(d1, d1); // d1.magnitude^2
        float e = Vector3.Dot(d2, d2); // d2.magnitude^2
        float f = Vector3.Dot(d2, r);

        // 평행한 경우를 위한 작은 값 (부동 소수점 오차 방지)
        float epsilon = 1e-6f;

        // 분모가 0에 가까우면 (두 선분이 평행하거나 한 선분이 점인 경우)
        float denom = a * e - Vector3.Dot(d1, d2) * Vector3.Dot(d1, d2);

        float s = 0.0f; // 선분 1의 매개변수
        float t = 0.0f; // 선분 2의 매개변수

        if (denom < epsilon) // 두 선분이 평행하거나 한쪽이 점인 경우
        {
            s = 0.0f; // 일단 s를 0으로 설정
                      // t를 구하여 선분 2의 범위 [0, 1]에 클램프
            t = f / e;
            t = Mathf.Clamp01(t);
        }
        else
        {
            // 최단 거리를 찾는 매개변수 s, t 계산
            s = (Vector3.Dot(d1, d2) * f - e * Vector3.Dot(d1, r)) / denom;
            t = (a * f - Vector3.Dot(d1, d2) * Vector3.Dot(d1, r)) / denom;

            // s, t를 선분 범위 [0, 1]로 클램프
            s = Mathf.Clamp01(s);
            t = Mathf.Clamp01(t);

            // 클램프 후에 다시 검증 (교차하지 않는 꼬인 위치 선분 처리)
            // 클램프 된 s, t가 최단 거리를 만들지 못하는 경우가 발생할 수 있음
            // 예를 들어, 한 선분의 끝점이 다른 선분에 가장 가까울 때
            Vector3 newR = p1 + s * d1 - (p3 + t * d2);
            float sqDist = Vector3.Dot(newR, newR);

            // 만약 s가 0이거나 1이 아니면서 t도 0이거나 1이 아니라면, 이 s, t는 유효
            if (s > epsilon && s < 1.0f - epsilon) { } // s가 중간에 있다면 괜찮음
            else if (t > epsilon && t < 1.0f - epsilon) { } // t가 중간에 있다면 괜찮음
            else // 최소 하나가 끝점에 위치하는 경우, 다시 s, t를 찾음
            {
                // 이 지점에서 다시 s, t를 계산하는 것은 복잡할 수 있으므로,
                // 간단하게 각 선분의 4가지 끝점 조합을 확인하는 방법을 사용합니다.
                // 또는 더욱 견고한 알고리즘을 사용합니다.
                // 여기서는 Graphics Gems의 알고리즘을 따릅니다.
                float s0 = Mathf.Clamp01(Vector3.Dot(d1, r) / a);
                float t0 = Mathf.Clamp01(-Vector3.Dot(d2, r) / e);

                Vector3 v0 = p1 + s0 * d1 - (p3 + t0 * d2);
                float d0 = v0.sqrMagnitude;

                Vector3 v1 = p1 + s0 * d1 - (p3 + 1.0f * d2); // p4
                float d1_ = v1.sqrMagnitude;

                Vector3 v2 = p1 + 1.0f * d1 - (p3 + t0 * d2); // p2
                float d2_ = v2.sqrMagnitude;

                Vector3 v3 = p1 + 1.0f * d1 - (p3 + 1.0f * d2); // p2, p4
                float d3_ = v3.sqrMagnitude;

                float minSqDist = Mathf.Min(d0, d1_, d2_, d3_);

                // 초기 계산된 s,t의 거리와 끝점 조합으로 구한 거리 중 최소값 사용
                if (sqDist < minSqDist)
                {
                    // 현재 s, t가 더 좋은 값 (일반적인 꼬인 위치 교차)
                }
                else
                {
                    // 끝점 조합 중 하나가 더 가깝거나 같음
                    // 다시 s, t를 계산할 필요 없이, 최소 거리를 제공하는 끝점 조합을 사용
                    // 하지만 현재 목표는 '최단 거리' 그 자체이므로, 여기서 반환할 거리는 `Mathf.Sqrt(minSqDist)`가 됩니다.
                    // 이 s, t 값은 더 이상 사용되지 않으므로, 이 함수 내에서만 유효합니다.
                    return Mathf.Sqrt(minSqDist);
                }
            }
        }

        // 최종적으로 계산된 s, t 값을 사용하여 최단 거리 벡터 r_c를 구하고 크기를 반환
        Vector3 closestPoint1 = p1 + s * d1;
        Vector3 closestPoint2 = p3 + t * d2;
        return Vector3.Distance(closestPoint1, closestPoint2);
    }










}












