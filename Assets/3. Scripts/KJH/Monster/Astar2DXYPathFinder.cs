using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using AstarNode = Astar2DXYPathFinder.AstarNode;
public class Astar2DXYPathFinder : MonoBehaviour
{
     public float height;
     public float width;
     public float unit;
     public float offeset;
     public bool canJump;
     public float jumpLength;
     #region UniTask Setting
     public CancellationTokenSource cts;
     void OnEnable()
     {
          cts = new CancellationTokenSource();
          Application.quitting += UniTaskCancel;
     }
     void OnDisable() { UniTaskCancel(); }
     void OnDestroy() { UniTaskCancel(); Dispose(); }
     void UniTaskCancel()
     {
          cts?.Cancel();
          try
          {
               cts?.Dispose();
          }
          catch (System.Exception e)
          {
               Debug.Log(e.Message);
          }
          cts = null;
     }
     #endregion
     //Astar에 필요한 커스텀 노드 struct
     public struct AstarNode : IHeapItem<AstarNode>
     {
          public bool isValid;
          public bool canMove;
          public int xIndex, yIndex;
          public int G, H;
          public int F => G + H;
          //레이캐스트로 알아낸 raycastHit.point.y 자체를 노드 정보에 담아버리자!
          public float yWorldPos;
          // 대부분의 예제에서 AstarNode는 class 재귀 구조로 되어있는데. struct화 하기위해서 아래를 추가하였음.
          public int index;
          public int parentIndex;
          //아래는 IEquatable<AstarNode> 인터페이스 구현이고. Job에서 NativeContainer를 다룰때 아래를 이용하면 편하므로 미리 구현해둔다.
          public bool Equals(AstarNode other) => index == other.index;
          // 아래는 IHeapItem<AstarNode> 인터페이스 구현으로. 
          // 특히 GetHeapValue가 중요한데. 속성들중에 최소 또는 최대를 찾고싶은 그 속성을 지정한다.
          public float GetHeapValue() => F;
          public int HeapIndex { get => heapIndex; set => heapIndex = value; }
          public int heapIndex;
     }
     public LayerMask groundLayer;
     public LayerMask obstacleLayer;
     // 캐릭터를 중심으로하는 다차원 배열크기 (가급적이면 그리드 상하좌우 대칭을 위해 홀수 사이즈로 해주세요)
     public Vector2Int astarArraySize = new Vector2Int(73, 9);

     // RaycastNonAlloc 및 OverlapBoxNonAlloc을 위한 버퍼
     // Astar2DXYPathFinder 클래스 멤버 변수 영역 (overlapCollidersBuffer 주변)에 추가해주세요.
     RaycastHit2D[] rayHitsBuffer = new RaycastHit2D[100]; // astarArraySize.y * 2 보다 큰 넉넉한 크기
     List<float> distinctYPoints = new List<float>(32);
     Collider2D[] overlapCollidersBuffer = new Collider2D[11]; // 넉넉하게 11개까지 콜라이더 감지 가능

     // 캐릭터를 주변 공간정보를 아래 node 다차원배열--> 잡일시 1D로 평탄화된 네이티브 배열에 담는다
     NativeArray<AstarNode> allNodes;
     // A* 알고리즘에 필요한 OpenSet과 ClosedSet
     NativeMinHeap<AstarNode> openMinHeap;
     NativeList<AstarNode> closedSet;
     NativeArray<AstarNode> resultNA;
     AstarNode startNode;
     AstarNode endNode;
     AstarNode currNode;
     private int centerXIndex;
     Vector2 characterPosition;
     Transform model;
     [HideInInspector] public Transform target;
     void Awake()
     {
          centerXIndex = Mathf.FloorToInt(astarArraySize.x * 0.5f);

     }
     void Start()
     {
          if (transform.childCount > 0)
               model = transform.GetChild(0);
          else
               model = transform;
     }
     // public async UniTask<Vector2[]> Find(Vector2 targetWorldPos, CancellationToken token)
     // {
     //      cts?.Cancel();
     //      Dispose();
     //      cts = new CancellationTokenSource();
     //      
     // }
     JobHandle jobHandle1;
     void Dispose()
     {
          // 1. 작업이 완료될 때까지 기다립니다. (Job 완료 보장)
          jobHandle1.Complete();
          // 2. 모든 Native Container에 대해 이중 해제 방지 로직을 적용합니다.
          if (allNodes.IsCreated)
          {
               try
               {
                    allNodes.Dispose();

               }
               catch
               {

               }
               allNodes = default; // 해제 후 명시적으로 default 값 할당 (안전성 강화)
          }
          // openMinHeap
          // NativeMinHeap은 내부적으로 Dispose에 IsCreated 체크가 있지만, 외부에서도 체크합니다.
          if (openMinHeap.IsCreated)
          {
               try
               {
                    openMinHeap.Dispose();

               }
               catch
               {

               }
               openMinHeap = default; // 해제 후 명시적으로 default 값 할당 (안전성 강화)
          }
          // closedSet
          if (closedSet.IsCreated)
          {
               try
               {
                    closedSet.Dispose();

               }
               catch
               {

               }
               closedSet = default; // 해제 후 명시적으로 default 값 할당 (안전성 강화)
          }
          // resultNA
          if (resultNA.IsCreated)
          {
               try
               {
                    resultNA.Dispose();

               }
               catch
               {

               }
               resultNA = default; // 해제 후 명시적으로 default 값 할당 (안전성 강화)
          }
     }
     #region 헬퍼 메소드
     // -----------------------------------------------------
     /// 두 노드 간의 이동 비용을 계산합니다 (G값 및 H값에 사용).
     int GetCost(AstarNode nodeA, AstarNode nodeB)
     {
          float tempX = ((nodeA.xIndex - nodeB.xIndex) * unit);
          float tempY = nodeA.yWorldPos - nodeB.yWorldPos;
          // 기본 거리
          int distCost = (int)(math.sqrt(tempX * tempX + tempY * tempY) * 100f);
          // 층 이동은 횡이동보다 더 많은 추가 비용을 들게 함 (예시 값)
          int addtionalCost = (int)(Mathf.Abs(tempY) * 500f);
          return distCost + addtionalCost;
     }
     Vector2 NodeToWorld(AstarNode node)
     {
          //if (node == null) return Vector2.zero; // 안전 장치
          // 그리드 원점 (transform.position을 기준으로)
          return new Vector2(
              characterPosition.x + (node.xIndex - centerXIndex) * unit,
              node.yWorldPos // AstarNode에 저장된 실제 월드 Y 좌표 사용
          );
          /* ------------------------- 중요 ------------------------------
          // 예를들어 xIndex=27, yIndex=0(시작 위치가 층이 1개라면), zIndex=27 인 node의 월드 위치는
          // characterPosition이 된다.
          */
     }
     Vector2 NearGround(Vector2 pos)
     {
          float minDist = float.MaxValue;
          Vector2 findPoint = Vector2.zero;
          RaycastHit2D hit;
          for (int i = 0; i < 50; i++)
          {
               Vector3 dir3D = Quaternion.Euler(0f, 0f, (150f * (((float)i) / 50f)) - 75f) * Vector3.down;
               Vector2 dir = (Vector2)dir3D;
               if (hit = Physics2D.Raycast(pos, dir, 10f, groundLayer))
               {
                    if (minDist > hit.distance)
                    {
                         minDist = hit.distance;
                         findPoint = hit.point;
                    }
                    if (hit.distance < 0.1f)
                    {
                         findPoint = hit.point;
                         break;
                    }
               }
          }
          if (findPoint != Vector2.zero)
          {
               return findPoint;
          }
          return pos;
     }
#if UNITY_EDITOR
     async UniTask DebugPath(Vector2[] worldPos, CancellationToken token)
     {
          await UniTask.Yield(cancellationToken: token);
          if (worldPos.Length > 0)
               for (int i = 0; i < worldPos.Length; i++)
               {
                    if (i == 0) { }
                    else if (i == 1)
                         Debug.DrawLine(worldPos[1], worldPos[0], Color.blue, 2f, true);
                    else
                         Debug.DrawLine(worldPos[i], worldPos[i - 1], Color.blue, 2f, true);

                    await UniTask.Delay(30, cancellationToken: token);
               }
     }
#endif
     Vector2[] ReconstructPath(AstarNode endNodeResult) // endNodeResult는 resultNA[0]에서 온 최종 노드
     {
          List<Vector2> path = new List<Vector2>();
          AstarNode currentNode = endNodeResult; // Job에서 찾은 최종 노드부터 시작

          // parentIndex가 -1이 될 때까지 (시작 노드에 도달할 때까지) 반복
          // allNodes NativeArray는 이미 Job이 완료된 후에도 사용 가능해야 합니다.
          while (currentNode.parentIndex != -1)
          {
               path.Add(NodeToWorld(currentNode));
               // allNodes에서 부모 노드를 찾아서 다음 currentNode로 설정
               currentNode = allNodes[currentNode.parentIndex];
          }
          path.Add(NodeToWorld(currentNode)); // 마지막으로 시작 노드 추가

          path.Reverse(); // 시작 노드부터 끝 노드까지 순서대로 정렬
          Vector2[] result = path.ToArray();
#if UNITY_EDITOR
          DebugPath(result, cts.Token).Forget();
#endif
          return result;
     }
     private void ProcessAndAssignNode(int x, int y, float yPos, Vector2 targetWorldPos, ref float minDistToEnd, ref AstarNode endNode, ref float minDistToStart, ref AstarNode startNode, Vector2 halfBox, Collider2D[] overlapCollidersBuffer, Vector2Int astarArraySize, NativeArray<AstarNode> allNodes, float unit, int centerXIndex, Vector2 characterPosition)
     {
          AstarNode node = new AstarNode();
          node.isValid = true;
          node.xIndex = x;
          node.yIndex = y;
          int index = x * (astarArraySize.y) + y;
          node.index = index;
          node.parentIndex = -1;
          node.yWorldPos = yPos;
          bool canMove = true;

          Vector2 nodeWorldPos = NodeToWorld(node);

          // 1. OverlapBox 검사 (장애물/벽 검사)
          int countB = Physics2D.OverlapBoxNonAlloc(
              nodeWorldPos + (0.25f + 0.5f * (height - 0.25f)) * Vector2.up,
              halfBox,
              0f,
              overlapCollidersBuffer,
              groundLayer | obstacleLayer
          );
          if (countB > 0)
          {
               canMove = false;
          }

          // 2. Start/End Node 찾기
          // End Node (Target) 찾기
          float dist = (targetWorldPos - nodeWorldPos).sqrMagnitude;
          if (dist < minDistToEnd)
          {
               minDistToEnd = dist;
               endNode = node;
          }
          // Start Node (Character Center) 찾기
          if (x == centerXIndex)
          {
               float dist2 = (characterPosition - nodeWorldPos).sqrMagnitude;
               if (dist2 < minDistToStart)
               {
                    minDistToStart = dist2;
                    startNode = node;
               }
          }

          // 3. 최종 할당
          node.canMove = canMove;
          allNodes[index] = node;
     }
     #endregion
     public async UniTask<Vector2[]> Find(Vector2 targetWorldPos, CancellationToken token)
     {
          offeset = 0.1f * height + 0.2f * unit;
          Vector2 nearGround = NearGround(targetWorldPos);
          targetWorldPos = nearGround;
          Vector2[] result = new Vector2[0];
          // A* 탐색을 위한 NativeArray 초기화 // 인덱스 2D -> 1D 화
          resultNA = new NativeArray<AstarNode>(1, Allocator.TempJob);
          allNodes = new NativeArray<AstarNode>(astarArraySize.x * astarArraySize.y, Allocator.TempJob);
          openMinHeap = new NativeMinHeap<AstarNode>(0, Allocator.TempJob);
          closedSet = new NativeList<AstarNode>(Allocator.TempJob);
          #region 그리드 초기화
          float minDistToStart = float.MaxValue;
          float minDistToEnd = float.MaxValue;
          characterPosition = (Vector2)transform.position;
          Vector2 halfBox = new Vector2(0.5f * width * 0.8f, 0.5f * (height - 0.25f));
          for (int i = 0; i < astarArraySize.x * astarArraySize.y; i++)
          {
               // 1D 인덱스 ---> 2D 인덱스화
               int x = i / (astarArraySize.y);
               int y = i % (astarArraySize.y);
               // allNodes 에서 y=0인 경우에만 레이캐스트를해서 astarArraySize.y를 전부 담을 계획이다.
               // y=1 ~ y=astarArraySize.y-1 의 allNodes[i] 값은 y=0 에서 하는 레이캐스트 과정에서 다 담긴다.
               if (y == 0)
               {
                    Vector2 rayOrigin = new Vector2
                    (
                         characterPosition.x + (x - centerXIndex) * unit,
                         characterPosition.y + unit * astarArraySize.x * 0.5f
                    );
                    Vector2 currentRayStartPos = rayOrigin; // 반복적으로 갱신될 레이 시작 위치

                    ////////////////////////
                    /// 현재 X-열에서 하늘 위에서 아래로 레이를 쏘아서 부딪히는 모든 층에 대한 정보를
                    /// 노드화 시켜야 하는데 문제가..
                    ///  
                    /// 방법1) Physics2D.RaycastNonAlloc 을 써서 충돌하는 astarArraySize.y개 이하의 점들의 정보를 담는 방법을 쓸경우.
                    /// 박스콜라이더 서클, 캡슐 2D들로만 이루어진 씬에서는 잘 작동하나... 
                    /// 타일맵 콜라이더 같은게 있을경우 타일상으로 여러 복잡한 층이 나눠져있는 지형이면
                    /// RaycastNonAlloc을 썼음에도 불구하고 타일의 천장(제일 윗부분에서) 한번만 충돌하고 끝남
                    /// 
                    /// 방법2) 방법1의 단점을 보완하려고 
                    /// 타일맵 콜라이더 같은거에서도 출동 층 정보를 잘 담기위해 Physics2D.Raycast 를 쏘고 닿는지점이 있다면 약간아래에 
                    /// 즉.. 땅(벽)속에서 다시 아래로 쏘아 새로운 땅(층)을 반복적으로 담는 방법을 사용한다면
                    /// 타일맵 콜라이더로만 이루어진 씬에서도 층 정보를 잘 담아냄. 근데 이러면 문제가 기존의 박스콜라이더들은
                    /// 약간아래의 콜라이더속에서 아래로 레이를 쏘면 타일맵에서때와 달리 콜라이더 속 레이 원점에서 바로 충돌 완료했다고 인식해서
                    /// 첫 충돌에서 9개가 다 차버림..
                    /// 
                    /// 방법3) 그래서 위 두가지 방법을 조합하여
                    /// 레이캐스트올(혹은 NonAlloc)으로 여러개의 개별로된 게임오브젝트 콜라이더들을 전부 감지하고 (방법1)
                    /// 이중에 타일맵 콜라이더가 있는경우. 타일맵콜라이더의 첫 충돌포인트에서 한개만 감지하는 Physics2D.Raycast를 약간아래 내부에서 파고들어서 다시 반복하는 로직 (방법2)
                    /// 를 같이 사용하고 싶음
                    /// 단 이때 하늘하고 가까운 위치대로 잘 담겨야함.
                    /// 만약 씬에 복합적으로 여러 장애물 플랫폼이 있다고 했을때
                    /// 하늘 레이 시작위치에서 시작해서 가까운 순서대로
                    /// 박스콜1 --> 타일맵(같은 타일맵 게임오브젝트 1지점) --> 서클콜라이더 --> 타일맵(같은 타일맵 게임오브젝트 2지점) --> 타일맵(같은 타일맵 게임오브젝트 3지점) --> 박스콜2
                    /// 이런순서로 잘 담기게 해야함
                    ///////////////////////

                    float maxDist = unit * astarArraySize.x * 2.0f; // 충분히 큰 최대 거리
                    // 1. 첫 번째 레이를 쏘아 충돌체의 종류를 확인
                    RaycastHit2D firstHit = Physics2D.Raycast(rayOrigin, Vector2.down, maxDist, groundLayer | obstacleLayer);

                    int layerCount = 0; // 이 X-열에서 실제로 감지된 층의 개수

                    // 2. 충돌이 발생한 경우 (콜라이더 종류에 따라 분기)
                    if (firstHit.collider)
                    {
                         // TilemapCollider2D, CompositeCollider2D 인지 확인
                         // CompositeCollider2D는 TilemapCollider2D와 함께 복합 지형을 만들 때 사용됩니다.
                         bool isTilemapCollider = firstHit.collider is UnityEngine.Tilemaps.TilemapCollider2D || firstHit.collider is UnityEngine.CompositeCollider2D;
                         // --- Case 1: TilemapCollider2D 또는 CompositeCollider2D (복합 콜라이더) - 반복 Raycast 사용 ---
                         if (isTilemapCollider)
                         {
                              Vector2 currentRayStartPos_Loop = rayOrigin; // 반복 루프 내에서 사용할 시작점
                              while (layerCount < astarArraySize.y)
                              {
                                   // currentRayStartPos에서 rayOrigin까지의 거리를 계산하여 maxDist를 재설정 (안전성 확보)
                                   float currentMaxDist = maxDist + (rayOrigin.y - currentRayStartPos_Loop.y);
                                   RaycastHit2D hit = Physics2D.Raycast(currentRayStartPos_Loop, Vector2.down, currentMaxDist, groundLayer | obstacleLayer);
                                   if (!hit.collider)
                                   {
                                        // 더 이상 층이 없으므로 루프 종료
                                        break;
                                   }
                                   // 노드 처리 로직 실행 (ProcessAndAssignNode 헬퍼 메서드 사용)
                                   ProcessAndAssignNode(x, layerCount, hit.point.y, targetWorldPos, ref minDistToEnd, ref endNode, ref minDistToStart, ref startNode, halfBox, overlapCollidersBuffer, astarArraySize, allNodes, unit, centerXIndex, characterPosition);

                                   // 다음 레이 발사 위치 갱신 (콜라이더 내부에서 재발사)
                                   currentRayStartPos_Loop = hit.point - Vector2.up * 0.2f;
                                   layerCount++;
                              }
                         }
                         // --- Case 2: Box, Circle, Polygon, Edge 등 (단일 콜라이더) - RaycastNonAlloc + 필터링 사용 ---
                         else
                         {
                              distinctYPoints.Clear(); // Y 좌표 리스트 초기화

                              // RaycastNonAlloc을 사용하여 모든 충돌 지점을 한 번에 가져옴
                              int countR = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.down, rayHitsBuffer, maxDist, groundLayer | obstacleLayer);

                              // 3-1. 고유한 Y 좌표(층 높이)만 추출
                              for (int j = 0; j < countR; j++)
                              {
                                   // 충돌 지점의 y좌표
                                   float yPos = rayHitsBuffer[j].point.y;
                                   bool isNew = true;

                                   // 작은 오차 범위(0.01f) 내에서 중복 제거 (float 비교 안전성 확보)
                                   foreach (float existingY in distinctYPoints)
                                   {
                                        if (Mathf.Abs(existingY - yPos) < 0.01f)
                                        {
                                             isNew = false;
                                             break;
                                        }
                                   }
                                   if (isNew)
                                   {
                                        distinctYPoints.Add(yPos);
                                   }
                              }

                              // 3-2. Y 좌표를 높은 순서(천장 -> 바닥)로 정렬
                              distinctYPoints.Sort((a, b) => b.CompareTo(a));

                              // 3-3. 정렬된 고유 Y 좌표로 노드 할당
                              for (int k = 0; k < distinctYPoints.Count && k < astarArraySize.y; k++)
                              {
                                   float yPos = distinctYPoints[k];
                                   // 노드 처리 로직 실행 (ProcessAndAssignNode 헬퍼 메서드 사용)
                                   ProcessAndAssignNode(x, k, yPos, targetWorldPos, ref minDistToEnd, ref endNode, ref minDistToStart, ref startNode, halfBox, overlapCollidersBuffer, astarArraySize, allNodes, unit, centerXIndex, characterPosition);
                                   layerCount++;
                              }
                         } // end else (Simple Collider)
                    } // end if (firstHit.collider)

                    // 4. Case 1 또는 Case 2에서 할당 후 남은 노드를 Invalid로 채우는 마무리 루프
                    for (int k = layerCount; k < astarArraySize.y; k++)
                    {
                         int index = x * (astarArraySize.y) + k;
                         AstarNode node = new AstarNode
                         {
                              isValid = false,
                              canMove = false,
                              xIndex = x,
                              yIndex = k,
                              index = index,
                              yWorldPos = 9999f,
                              parentIndex = -1
                         };
                         allNodes[index] = node;
                    }
               }
          }
          #endregion
          //Debug.Log($"{endNode.xIndex} , {endNode.yIndex} , {endNode.index} , {endNode.parentIndex}");
          //Debug.Log($"{startNode.xIndex} , {startNode.yIndex} , {startNode.index} , {startNode.parentIndex}");
          // 그리드 검사 끝나면.. startNode 또는 endNode를 못찾은 경우 길찾기 취소
          if (startNode.isValid == false || endNode.isValid == false)
          {
               //Debug.Log("startNode, endNode 못찾음 에러");
               Dispose();
               return result;
          }
          // startNode 와 endNode가 같은 경우도 길찾기 취소 (결과 배열은 원소 한개인 배열로 리턴)
          if (startNode.xIndex == endNode.xIndex && startNode.yIndex == endNode.yIndex)
          {
               //Debug.Log("startNode = endNode 에러");
               Dispose();
               return new Vector2[] { NodeToWorld(startNode) + offeset * Vector2.up };
          }
          // 만약에 Astar 타겟 목표지점에 충돌체 몬스터가 서있을경우. 
          // 위에 전체 그리드검사 오버랩 충돌검사에서 분명 canMove가 false가 되었을것이다.
          // "캐릭터 자신의 시작위치"와. "타겟 오브젝트의 도착위치"의 콜라이더를 "갈수 없는 곳"으로 판정해서는 안된다.
          AstarNode temp;
          temp = allNodes[startNode.index];
          temp.canMove = true;
          allNodes[startNode.index] = temp;
          temp = allNodes[endNode.index];
          temp.canMove = true;
          allNodes[endNode.index] = temp;
          startNode.canMove = true;
          endNode.canMove = true;
          // 스타트 노드 초기 비용 설정
          startNode.G = 0;
          // 스타트 노드 휴리스틱 설정
          startNode.H = GetCost(startNode, endNode);
          // 스타트 노드 인덱스 설정
          startNode.index = startNode.xIndex * (astarArraySize.y) + startNode.yIndex;
          // startNode 바꾼값을 allNodes에 저장 (struct는 값타입이므로 이렇게 해야 변경이 적용됨)
          allNodes[startNode.index] = startNode;

          // --------------- A start 알고리즘의 대략적인 흐름 설명 -------------------
          // 그리드 초기화가 바로 위에서 끝났으므로, 이제 startNode와 endNode를 사용하여
          // A* 알고리즘의 핵심 탐색 로직을 구현합니다.
          // openMinHeap startNode를 추가하고 반복 시작:
          // - openMinHeap F값이 가장 낮은 노드를 currNode로 선택.
          // - currNode가 endNode라면 경로 재구성 및 반환.
          // - currNode를 closedSet으로 이동.
          // - currNode의 모든 이웃 노드(8방향 + 위/아래 층)에 대해:
          //   - canMove == false 이거나 closedSet에 있다면 건너뛰기.
          //   - 새로운 G 값 계산 (이동 비용 + 높이 변화 비용).
          //   - openMinHeap 없거나 새로운 G 값이 더 낮으면:
          //     - G, H, F 값 업데이트 및 parent 설정.
          //     - openMinHeap 추가 (이미 있다면 업데이트).
          // -----------------------------------------------------
          openMinHeap.Add(startNode);
          AstarJob job = new AstarJob
          {
               result = resultNA,
               allNodes = allNodes,
               openMinHeap = openMinHeap,
               closedSet = closedSet,
               startNode = startNode,
               endNode = endNode,
               astarArraySize = new int2(astarArraySize.x, astarArraySize.y),
               unit = unit,
               characterHeight = height,
               characterDiameter = width,
               characterJumpMaxHeight = math.max(jumpLength, 0.8f * height),
               characterDownMax = 1.5f * height + unit + 0.5f,
               characterPosition = characterPosition,
               modelForward = new float2(model.forward.x, model.forward.z),
               centerXIndex = centerXIndex,
               canJump = canJump,
               jumpLength = jumpLength,
               randomSeed = (uint)Time.frameCount,
          };
          jobHandle1 = job.Schedule();
          await UniTask.WaitUntil(() => jobHandle1.IsCompleted);
          jobHandle1.Complete();

          //Astar Job 끝. 최종경로
          if (resultNA[0].parentIndex == -1)
          {
               //Debug.Log("경로를 찾지 못했습니다.");
               Dispose();
               return result;
          }
          result = ReconstructPath(resultNA[0]);
          Dispose();
          for (int i = 0; i < result.Length; i++)
          {
               result[i] += offeset * Vector2.up;
          }
          return result;
     }
}
[BurstCompile]
public struct AstarJob : IJob
{
     [WriteOnly] public NativeArray<AstarNode> result;
     public NativeArray<AstarNode> allNodes;
     public NativeMinHeap<AstarNode> openMinHeap;
     public NativeList<AstarNode> closedSet;
     [ReadOnly] public AstarNode startNode;
     [ReadOnly] public AstarNode endNode;
     [ReadOnly] public int2 astarArraySize;
     [ReadOnly] public float unit;
     [ReadOnly] public float characterHeight;
     [ReadOnly] public float characterDiameter;
     [ReadOnly] public float characterJumpMaxHeight;
     [ReadOnly] public float characterDownMax;
     [ReadOnly] public float2 characterPosition;
     [ReadOnly] public float2 modelForward;
     [ReadOnly] public int centerXIndex;
     [ReadOnly] public bool canJump;
     [ReadOnly] public float jumpLength;
     [ReadOnly] public uint randomSeed;
     private Unity.Mathematics.Random random;
     #region 헬퍼 메소드
     int GetCost(AstarNode nodeA, AstarNode nodeB)
     {
          float tempX = ((nodeA.xIndex - nodeB.xIndex) * unit);
          float tempY = nodeA.yWorldPos - nodeB.yWorldPos;
          // 기본 거리
          int distCost = (int)(math.sqrt(tempX * tempX + tempY * tempY) * 100f);
          // 층 이동은 횡이동보다 더 많은 추가 비용을 들게 함 (예시 값)
          int addtionalCost = (int)(Mathf.Abs(tempY) * 500f);
          return distCost + addtionalCost;
     }
     float2 NodeToWorld(AstarNode node)
     {
          // 예를들어 xIndex=27, yIndex=0(시작 위치가 층이 1개라면), zIndex=27 인 node의 월드 위치는
          // characterPosition이 된다.
          return new float2(
              characterPosition.x + (node.xIndex - centerXIndex) * unit,
              node.yWorldPos // AstarNode에 저장된 실제 월드 Y 좌표 사용
          );
     }
     #endregion
     public void Execute()
     {
          random = new Unity.Mathematics.Random(randomSeed);
          AstarNode currNode;
          const int MAX_STEP = 500000;
          int step = 0;
          if (openMinHeap.Count == 0)
          {
               //Debug.Log("openMinHeap가 비었습니다.");
               result[0] = new AstarNode { isValid = false };
               return;
          }
          while (openMinHeap.Count > 0 && step < MAX_STEP)
          {
               step++;
               currNode = openMinHeap.RemoveMin();
               closedSet.Add(currNode);
               //현재 노드가 목표 노드라면 반환
               if (currNode.Equals(endNode))
               {
                    //Debug.Log("Path를 찾았습니다");
                    result[0] = currNode;
                    return;
               }
               // 현재노드의 모든 이웃노드 탐색해서 openSet에 넣기




               //      // 예를들어 jumpLength 가 3이고 // unit 이 0.6 이라면. 
               //      // 기본적인 -1,1 인접칸을 오픈셋에 넣는 기존 Astar에서 더 추가적으로
               //      // 좌우로 -5~5 유닛(int)(jumpLength / unit)까지도 점프뛰어야 하는 상황이 있을수있다.
               //      int sideJumpMaxIndex = (int)(jumpLength / unit);
               //      // 하지만 모든 사이트 포인트를 다 경로후보인 openSet에 넣기에는 알고리즘이 실행 횟수가 너무 많아진다.
               //      // 따라서 어느정도의 취사선택 로직을 만들어야 하는데.
               //      // 가령 (int)(jumpLength / unit)가 5인 경우. x=-1,0,1 에서 추가적으로 2개까지만 더 넣을것인데.
               //      // 원래대로라면 x=-5,-4,-3,-2,2,3,4,5 8개가 오픈셋에 들어가야하나. 이중에서 좌우 하나씩 랜덤으로
               //      // x= -4, 2 이렇게만 아래 for문에서 더 추가되게 할것이다.
               //      // for (int x = -4,-1,0,1,2;) 이런식으로
               NativeList<int> xDeltas = new NativeList<int>(10, Allocator.Temp);
               // 1. 기본 인접 노드 (x = -1, 0, 1)
               xDeltas.Add(-1);
               xDeltas.Add(0);
               xDeltas.Add(1);
               if (canJump)
               {
                    int sideJumpMaxIndex = (int)((jumpLength + 0.8f * unit + 0.1f) / unit);
                    if (sideJumpMaxIndex >= 2)
                    {
                         xDeltas.Add(sideJumpMaxIndex);
                         xDeltas.Add(-sideJumpMaxIndex);
                         // 만약 sideJumpMaxIndex가 4 이상일 때, 중간 지점 2개 (예: -3, 3)를 추가하는 로직이 필요하다면:
                         if (sideJumpMaxIndex >= 4)
                         {
                              int middleJumpIndexR = random.NextInt(3, sideJumpMaxIndex - 1);
                              int middleJumpIndexL = -random.NextInt(3, sideJumpMaxIndex - 1);
                              xDeltas.Add(middleJumpIndexR);
                              xDeltas.Add(middleJumpIndexL);
                         }
                    }
               }

               for (int n = 0; n < xDeltas.Length; n++)
               {
                    int x = xDeltas[n];
                    int jumpCost = 0;
                    if (n >= 3)
                    {
                         jumpCost = math.abs(x) * 200;
                    }

                    // '이 이웃 노드'의 X 좌표
                    int neighborX = currNode.xIndex + x;

                    // '이 이웃 노드'가 그리드 X 범위를 벗어나면 건너뛰기
                    if (neighborX < 0 || neighborX >= astarArraySize.x)
                    {
                         continue;
                    }

                    // X축 변화가 없을 때: 제자리에서 위층(+1)과 아래층(-1)만 확인
                    if (x == 0)
                    {
                         for (int yDelta = -1; yDelta <= 1; yDelta++)
                         {
                              // 자기 자신 노드 (x=0, yDelta=0)는 건너뛰기
                              if (yDelta == 0) continue;

                              int neighborY = currNode.yIndex + yDelta;

                              // '이 이웃 노드'가 그리드 Y 범위를 벗어나면 건너뛰기
                              if (neighborY < 0 || neighborY >= astarArraySize.y)
                              {
                                   continue;
                              }

                              // ------------ 이웃 노드 처리 시작 ------------
                              AstarNode neighbor = allNodes[neighborX * (astarArraySize.y) + neighborY];

                              neighbor.xIndex = neighborX;
                              neighbor.yIndex = neighborY;
                              int index = neighborX * (astarArraySize.y) + neighborY;
                              neighbor.index = index;


                              // 이동 불가능한 노드이거나 이미 ClosedSet에 있다면 건너뛰기
                              if (!neighbor.isValid || !neighbor.canMove)
                              {
                                   continue;
                              }
                              bool temp = false;
                              for (int i = 0; i < closedSet.Length; i++)
                              {
                                   if (closedSet[i].Equals(neighbor))
                                   {
                                        temp = true;
                                        break;
                                   }
                              }
                              if (temp) continue;
                              // 높이 차이 검사
                              float heightDifference = neighbor.yWorldPos - currNode.yWorldPos;
                              if (heightDifference > characterJumpMaxHeight   // 너무 높이 올라갈 수 없음
                              || heightDifference < -characterDownMax)       // 너무 깊이 내려갈 수 없음
                              {
                                   continue;
                              }
                              // '현재 노드'에서 '이 이웃 노드'로 가는 새로운 G 값 계산
                              int cost = currNode.G + GetCost(currNode, neighbor) + jumpCost;
                              // 플레이어 근처이면서 & 모델의 방향과 많이 꺽인 노드라면 방향전환 비용 증가.
                              // 캐릭터 근처 노드인지 판단하는 정도
                              float2 world = NodeToWorld(currNode);
                              // 대충 캐릭터와 제일 가까운 노드는 64정도 값이 되고. 가장 먼 노드는 10 정도의 값이 됨. 이를 바탕으로
                              float near = astarArraySize.x + 10 - (math.abs(currNode.xIndex - centerXIndex));
                              near = near / (astarArraySize.x + 10); // 정규화
                                                                     // 가까운 노드로 한정하여.
                              if (near > 0.3f)
                              {
                                   // Vector2.Angle 대신 math.acos(math.dot) 또는 float2.Angle 사용
                                   // Burst에 친화적인 math 라이브러리 사용을 권장합니다.
                                   float angleRad = math.acos(math.dot(math.normalize(world - characterPosition), math.normalize(modelForward)));
                                   float angleDeg = math.degrees(angleRad); // 라디안을 다시 각도로 변환
                                   float angle = angleDeg / 180f; //정규화
                                   int addCost = (int)(20f * angle * near);
                                   cost += addCost;
                              }
                              // 만약 이웃 노드가 OpenSet에 없거나, 새로운 G 값이 기존 G 값보다 낮다면
                              bool isNeighborInOpenSet = false;
                              for (int i = 0; i < openMinHeap.Count; i++) // <- .Length 사용
                              {
                                   if (openMinHeap.items[i].Equals(neighbor))
                                   {
                                        isNeighborInOpenSet = true;
                                        break;
                                   }
                              }
                              if (!isNeighborInOpenSet || cost < neighbor.G)
                              {
                                   neighbor.G = cost; // G 값 업데이트
                                   neighbor.H = GetCost(neighbor, endNode); // H 값 업데이트 (휴리스틱)
                                   neighbor.parentIndex = currNode.index; // 부모 노드 설정
                                   allNodes[index] = neighbor; // struct 갱신
                                                               // 이웃 노드가 OpenSet에 없다면 추가
                                   if (!isNeighborInOpenSet)
                                   {
                                        openMinHeap.Add(neighbor);
                                   }
                                   // 이웃 노드가 OpenSet에 이미있다면 G값 갱신
                                   else
                                   {
                                        openMinHeap.UpdateItem(neighbor);
                                   }

                              }
                              // ------------ 이웃 노드 처리 끝 ------------
                         }
                    }

                    // X축 변화가 있을 때: 좌우 이동 시, 이웃 X열의 모든 Y층을 확인
                    else // x == -1 or x == 1
                    {
                         for (int neighborY = 0; neighborY < astarArraySize.y; neighborY++)
                         {
                              // 모든 Y층을 검사하며, 높이 차이 검사를 통해 유효한 층만 걸러짐

                              // ------------ 이웃 노드 처리 시작 ------------
                              AstarNode neighbor = allNodes[neighborX * (astarArraySize.y) + neighborY];

                              neighbor.xIndex = neighborX;
                              neighbor.yIndex = neighborY;
                              int index = neighborX * (astarArraySize.y) + neighborY;
                              neighbor.index = index;


                              // 이동 불가능한 노드이거나 이미 ClosedSet에 있다면 건너뛰기
                              if (!neighbor.isValid || !neighbor.canMove)
                              {
                                   continue;
                              }
                              bool temp = false;
                              for (int i = 0; i < closedSet.Length; i++)
                              {
                                   if (closedSet[i].Equals(neighbor))
                                   {
                                        temp = true;
                                        break;
                                   }
                              }
                              if (temp) continue;
                              // 높이 차이 검사
                              float heightDifference = neighbor.yWorldPos - currNode.yWorldPos;
                              if (heightDifference > characterJumpMaxHeight   // 너무 높이 올라갈 수 없음
                              || heightDifference < -characterDownMax)       // 너무 깊이 내려갈 수 없음
                              {
                                   continue;
                              }
                              // '현재 노드'에서 '이 이웃 노드'로 가는 새로운 G 값 계산
                              int cost = currNode.G + GetCost(currNode, neighbor);
                              // 플레이어 근처이면서 & 모델의 방향과 많이 꺽인 노드라면 방향전환 비용 증가.
                              // 캐릭터 근처 노드인지 판단하는 정도
                              float2 world = NodeToWorld(currNode);
                              // 대충 캐릭터와 제일 가까운 노드는 64정도 값이 되고. 가장 먼 노드는 10 정도의 값이 됨. 이를 바탕으로
                              float near = astarArraySize.x + 10 - (math.abs(currNode.xIndex - centerXIndex));
                              near = near / (astarArraySize.x + 10); // 정규화
                                                                     // 가까운 노드로 한정하여.
                              if (near > 0.3f)
                              {
                                   // Vector2.Angle 대신 math.acos(math.dot) 또는 float2.Angle 사용
                                   // Burst에 친화적인 math 라이브러리 사용을 권장합니다.
                                   float angleRad = math.acos(math.dot(math.normalize(world - characterPosition), math.normalize(modelForward)));
                                   float angleDeg = math.degrees(angleRad); // 라디안을 다시 각도로 변환
                                   float angle = angleDeg / 180f; //정규화
                                   int addCost = (int)(20f * angle * near);
                                   cost += addCost;
                              }
                              // 만약 이웃 노드가 OpenSet에 없거나, 새로운 G 값이 기존 G 값보다 낮다면
                              bool isNeighborInOpenSet = false;
                              for (int i = 0; i < openMinHeap.Count; i++) // <- .Length 사용
                              {
                                   if (openMinHeap.items[i].Equals(neighbor))
                                   {
                                        isNeighborInOpenSet = true;
                                        break;
                                   }
                              }
                              if (!isNeighborInOpenSet || cost < neighbor.G)
                              {
                                   neighbor.G = cost; // G 값 업데이트
                                   neighbor.H = GetCost(neighbor, endNode); // H 값 업데이트 (휴리스틱)
                                   neighbor.parentIndex = currNode.index; // 부모 노드 설정
                                   allNodes[index] = neighbor; // struct 갱신
                                                               // 이웃 노드가 OpenSet에 없다면 추가
                                   if (!isNeighborInOpenSet)
                                   {
                                        openMinHeap.Add(neighbor);
                                   }
                                   // 이웃 노드가 OpenSet에 이미있다면 G값 갱신
                                   else
                                   {
                                        openMinHeap.UpdateItem(neighbor);
                                   }

                              }
                              // ------------ 이웃 노드 처리 끝 ------------
                         }
                    }
               } // end for x
          } // end while

          // while 루프가 openSet.Length > 0 조건을 만족하지 못해 종료된 경우 (경로를 찾지 못함)
          //Debug.Log("경로를 찾지 못했습니다.");
          result[0] = new AstarNode { isValid = false, parentIndex = -1 };
     }
}