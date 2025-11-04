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
     public float jumpForce;
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
          try
          {
               cts?.Cancel();
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
     RaycastHit2D[] rayHitsBuffer = new RaycastHit2D[11]; // 넉넉하게 11개까지 히트 감지 가능
     List<RaycastHit2D> rayHitsBuffer2 = new List<RaycastHit2D>();
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
     public async UniTask<Vector2[]> Find(Vector2 targetWorldPos)
     {
          Dispose();
          cts?.Cancel();
          cts = new CancellationTokenSource();
          offeset = 0.1f * height + 0.2f * unit;
          Vector2 nearGround = NearGround(targetWorldPos);
          return await Find(nearGround, cts.Token);
     }
     JobHandle jobHandle1;
     void Dispose()
     {
          jobHandle1.Complete();
          if (allNodes.IsCreated) allNodes.Dispose();
          if (openMinHeap.IsCreated) openMinHeap.Dispose();
          if (closedSet.IsCreated) closedSet.Dispose();
          if (resultNA.IsCreated) resultNA.Dispose();
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
     async UniTask DebugPath(Vector2[] worldPos)
     {
          await UniTask.Yield(cancellationToken: cts.Token);
          if (worldPos.Length > 0)
               for (int i = 0; i < worldPos.Length; i++)
               {
                    if (i == 0) { }
                    else if (i == 1)
                         Debug.DrawLine(worldPos[1], worldPos[0], Color.blue, 2f, true);
                    else
                         Debug.DrawLine(worldPos[i], worldPos[i - 1], Color.blue, 2f, true);

                    await UniTask.Delay(30, cancellationToken: cts.Token);
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
          DebugPath(result).Forget();
#endif
          return result;
     }
     #endregion
     public async UniTask<Vector2[]> Find(Vector2 targetWorldPos, CancellationToken token)
     {
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
                    // 레이 오리진의 높이좌표는 대충 캐릭터의 position.y + unit * astarArraySize.x * 0.5 로 했음.
                    // 이렇게하면 검사영역의 크기가 캐릭터를 중심으로 하는 거대한 정사각형 형태가 된다
                    Vector2 rayOrigin = new Vector2
                    (
                         characterPosition.x + (x - centerXIndex) * unit,
                         characterPosition.y + unit * astarArraySize.x * 0.5f
                    );
                    // Ray를 수직아래로 쏴서 하늘과 가장 가까운 순서대로 담자
                    int countR = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.down, rayHitsBuffer, unit * astarArraySize.x, groundLayer | obstacleLayer);
                    rayHitsBuffer2.Clear();
                    for (int k = 0; k < countR; k++) rayHitsBuffer2.Add(rayHitsBuffer[k]);
                    rayHitsBuffer2.Sort((a, b) => a.distance.CompareTo(b.distance));
                    for (int k = 0; k < astarArraySize.y; k++)
                    {
                         AstarNode node = new AstarNode();
                         node.isValid = true;
                         node.xIndex = x;
                         node.yIndex = k;
                         int index = x * (astarArraySize.y) + k;
                         node.index = index;
                         node.parentIndex = -1; //처음에는 부모 없음
                         bool canMove;
                         float yPos;
                         //Debug.Log($"{k} / {countR} / {astarArraySize.y}");
                         if (k >= countR)
                         {
                              node.isValid = false;
                              canMove = false;
                              yPos = 9999f;
                         }
                         else
                         {
                              canMove = true;
                              yPos = rayHitsBuffer2[k].point.y;
#if UNITY_EDITOR
                              //DebugExtension.DebugCircle(rayHitsBuffer2[k].point, Vector3.forward, Color.yellow, 0.3f * unit, 5f, true);
#endif
                         }
                         node.yWorldPos = yPos;
                         Vector2 nodeWorldPos = NodeToWorld(node);
                         // 이 다음으로 각 그리드 포인트에 장애물이 있는지 충돌 검사도 해버리자.
                         if (canMove)
                         {
                              int countB = Physics2D.OverlapBoxNonAlloc(
                                  nodeWorldPos + (0.25f + 0.5f * (height - 0.25f)) * Vector2.up,
                                  halfBox,
                                  0f, // 2D 오버랩 박스는 회전 각도가 Z축 회전이라서 0으로 고정
                                  overlapCollidersBuffer,
                                  groundLayer | obstacleLayer
                              );
                              if (countB > 0)
                              {
                                   canMove = false;
                              }
                         }
                         // 모든 그리드 다중 for문으로 전체 검사하는김에.
                         // 겸사겸사 startNode 와 endNode도 여기서 찾아버리자.
                         float dist = (targetWorldPos - nodeWorldPos).sqrMagnitude;
                         if (dist < minDistToEnd)
                         {
                              minDistToEnd = dist;
                              endNode = node;
                         }
                         if (x == centerXIndex)
                         {
                              float dist2 = (characterPosition - nodeWorldPos).sqrMagnitude;
                              if (dist2 < minDistToStart)
                              {
                                   minDistToStart = dist2;
                                   startNode = node;
                              }
                         }
                         // canMove 값은 위 과정을 모두 거치고 최종적으로 대입하자. 
                         // 레이검사든 -> 오버랩충돌검사든 어느 과정상에서라도 갈수없는 판정이 하나라도 나오면 갈수없는 곳이다.
                         node.canMove = canMove;
                         //Debug.Log(index);
                         // 최종적으로 node 를 allNodes[x * k * z]에 넣자
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
               characterJumpMaxHeight = math.max(0.0175f * (jumpForce)*(jumpForce) + 0.0075f * (jumpForce) - 0.025f, 0.5f * height),
               characterDownMax = 8f,
               characterPosition = characterPosition,
               modelForward = new float2(model.forward.x, model.forward.z),
               centerXIndex = centerXIndex,
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
               //현재노드의 모든 이웃노드 탐색해서 openSet에 넣기 (26방향: X, Y, Z축으로 -1, 0, 1 범위)
               for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                         // 자기 자신 노드는 건너뛰기
                         if (x == 0 && y == 0) continue;
                         // '이 이웃 노드'의 좌표는
                         int neighborX = currNode.xIndex + x;
                         int neighborY = currNode.yIndex + y;


                         // '이 이웃 노드'가 그리드 범위를 벗어나면 건너뛰기
                         if (neighborX < 0 || neighborX >= astarArraySize.x
                             || neighborY < 0 || neighborY >= astarArraySize.y)
                         {
                              continue;
                         }

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
                         // GetCost는 아래 헬퍼 메서드에 정의
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
                    }


          }
          // while 루프가 openSet.Length > 0 조건을 만족하지 못해 종료된 경우 (경로를 찾지 못함)
          //Debug.Log("경로를 찾지 못했습니다.");
          result[0] = new AstarNode { isValid = false, parentIndex = -1 };
     }
}