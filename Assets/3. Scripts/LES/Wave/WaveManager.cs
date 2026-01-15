using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SpawnEntry
{
    [Tooltip("생성할 몬스터 프리팹")]
    public GameObject monsterPrefab;

    [Tooltip("이 종류의 몬스터를 몇 마리 생성할 것인가")]
    public int count = 1;

    [Tooltip("몬스터 간 생성 간격 (초)")]
    public float spawnInterval = 0.5f;

    [Header("Optional Settings")]
    [Tooltip("비워두면(None): 기존처럼 랜덤한 곳에서 나옵니다.\n지정하면(Transform): 해당 위치가 '화면 밖'일 때만 그곳에서 나옵니다.")]
    public Transform specificSpawnPoint;


}

[System.Serializable]
public class Wave
{
    public List<SpawnEntry> spawnEntries;
    public float delayBeforeWave = 2.0f;

    [Header("Clear Condition")]
    public bool waitForClear = true;
    public float waveDuration = 2.0f;
}

public class WaveManager : MonoBehaviour
{

    [Header("--- Settings ---")]
    public List<Transform> allSpawnPoints;

    [Tooltip("카메라 화면 밖으로 간주할 여유 범위 (0이면 화면 딱 끝, 0.1이면 화면보다 조금 더 밖)")]
    public float cameraBuffer = -8f;

    [Header("--- Waves Config ---")]
    public List<Wave> waves;

    [Tooltip("클리어하면 나타날 상자 프리팹")]
    public GameObject chestPrefab;

    private List<GameObject> currentActiveMonsters = new List<GameObject>();
    private Camera mainCam;
    private int currentWaveIndex = 0;
    private bool isBattleStarted = false;

    void Start()
    {
        mainCam = Camera.main;
    }
    Vector2 _startPosition;
    [SerializeField] DoorType1 doorType1;
    DoorType2 doorType2;
    public void StartBattle(Vector2 startPosition)
    {
        _startPosition = startPosition;
        if (isBattleStarted) return;
        isBattleStarted = true;
        if (doorType1.isComplete || doorType1.isPlayerRight) return;
        doorType1?.Close();
        doorType2 = doorType1.doorType2;
        StartCoroutine(ExecuteWaves());
    }
    IEnumerator ExecuteWaves()
    {
        foreach (var wave in waves)
        {
            currentWaveIndex++;
            Debug.Log($"=== Wave {currentWaveIndex} Start ===");

            yield return new WaitForSeconds(wave.delayBeforeWave);

            //float waitTimer = 0f;
            //float maxWaitTime = 10.0f;

            // 1. 몬스터 스폰 진행
            foreach (var entry in wave.spawnEntries)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    // [핵심 변경] 지정된 스폰 포인트가 있는 경우
                    if (entry.specificSpawnPoint != null)
                    {
                        // 조건: 지정된 위치가 카메라 '안'에 있다면, '밖'으로 나갈 때까지 무한 대기
                        while (IsVisibleOnScreen(entry.specificSpawnPoint.position))
                        {
                            // waitTimer += 0.5f;
                            // if (waitTimer > maxWaitTime)
                            // {
                            //     Debug.LogWarning("플레이어가 너무 오래 버텨서 강제 소환합니다!");
                            //     break; // 반복문 탈출 -> 소환
                            // }
                            // 개발자를 위한 로그 (너무 자주 뜨지 않게 하고 싶으면 주석 처리)
                            // Debug.Log($"몬스터가 {entry.specificSpawnPoint.name}에서 나오려 했으나, 화면 안이라 대기중...");

                            // 0.5초 뒤에 다시 검사 (매 프레임 검사는 성능 낭비)
                            yield return new WaitForSeconds(0.5f);
                        }

                        // 반복문을 탈출했다면 화면 밖이라는 뜻 -> 소환
                        SpawnMonsterAtPoint(entry.monsterPrefab, entry.specificSpawnPoint);
                    }
                    else
                    {
                        // 지정된 위치가 없으면 기존 로직 (알아서 화면 밖 찾아서 소환)
                        TrySpawnMonsterOffScreen(entry.monsterPrefab);
                    }

                    // 다음 몬스터 소환 전 딜레이
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            // 2. 클리어 조건 확인 (파괴/비활성화 모두 대응)
            if (wave.waitForClear)
            {
                if (currentActiveMonsters.Count > 0)
                {
                    while (true)
                    {
                        // 1. 이미 파괴(Destroy)되어 null이 된 참조들을 리스트에서 먼저 제거합니다.
                        currentActiveMonsters.RemoveAll(m => m == null);

                        // 2. 리스트에 남은 객체 중 하이라키에서 '활성화'된 몬스터가 있는지 확인합니다.
                        // 리스트가 비어있거나, 남은 몬스터가 모두 비활성화 상태라면 false가 됩니다.
                        bool isAnyMonsterActive = currentActiveMonsters.Any(m => m.activeInHierarchy);

                        if (!isAnyMonsterActive)
                        {
                            currentActiveMonsters.Clear(); // 다음 웨이브를 위해 리스트 청소
                            break;
                        }

                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }

            Debug.Log($"=== Wave {currentWaveIndex} Ended ===");
        }

        Debug.Log("🎉 STAGE CLEARED 🎉");
        doorType2?.Open();
        doorType1?.Open();
        doorType1.isComplete = true;

        GameObject chest = Instantiate(chestPrefab);
        chest.transform.position = 0.5f * (_startPosition + (Vector2)doorType2.transform.position) + 5f * Vector2.up;
        chest.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

    }

    // --- Helper Logic ---

    void SpawnMonsterAtPoint(GameObject prefab, Transform spawnPoint)
    {
        if (spawnPoint == null) return;
        GameObject mon = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        mon.transform.name = prefab.transform.name;
        MonsterControl monsterControl = mon.GetComponent<MonsterControl>();
        if (monsterControl)
        {
            if (monsterControl.homeValue >= 0.5f) monsterControl.homeValue = 0.5f * monsterControl.homeValue;
        }
        currentActiveMonsters.Add(mon);
    }

    /// <summary>
    /// 해당 월드 좌표가 현재 카메라 화면(Viewport) 안에 있는지 검사
    /// </summary>
    bool IsVisibleOnScreen(Vector3 targetPos)
    {
        // 1. 카메라의 절반 높이와 너비 계산
        float camHeight = mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;

        // 2. 카메라 중심과 타겟 사이의 거리 계산 (Z축 무시)
        Vector2 camPos = mainCam.transform.position;
        Vector2 targetPos2D = targetPos;
        Vector2 diff = targetPos2D - camPos;

        // 3. 버퍼(여유 공간)를 포함한 화면 영역 안에 있는지 체크
        // cameraBuffer가 1.0이면 화면 크기보다 1.0만큼 더 넓은 범위를 '화면 안'으로 칩니다.
        // 타겟이 이 범위 안에 있으면 "보인다(true)" -> 스폰 대기
        bool isInsideX = Mathf.Abs(diff.x) < (camWidth + cameraBuffer);
        bool isInsideY = Mathf.Abs(diff.y) < (camHeight + cameraBuffer);

        return isInsideX && isInsideY;
    }

    void TrySpawnMonsterOffScreen(GameObject prefab)
    {
        Transform bestSpot = GetOffScreenSpawnPoint();
        if (bestSpot != null)
        {
            GameObject mon = Instantiate(prefab, bestSpot.position, Quaternion.identity);
            currentActiveMonsters.Add(mon);
        }
        else
        {
            // 랜덤 스폰인데 쏠 곳이 없으면, 가장 먼 곳에 쏨 (이건 비상 대책이라 그냥 둠)
            Transform fallbackSpot = GetFurthestSpawnPoint();
            if (fallbackSpot != null)
            {
                GameObject mon = Instantiate(prefab, fallbackSpot.position, Quaternion.identity);
                currentActiveMonsters.Add(mon);
            }
        }
    }

    Transform GetOffScreenSpawnPoint()
    {
        // 기존 로직 유지 (랜덤 스폰용)
        Vector3 minScreen = mainCam.ViewportToWorldPoint(new Vector3(0, 0, mainCam.nearClipPlane));
        Vector3 maxScreen = mainCam.ViewportToWorldPoint(new Vector3(1, 1, mainCam.nearClipPlane));

        float minX = minScreen.x - cameraBuffer;
        float maxX = maxScreen.x + cameraBuffer;
        float minY = minScreen.y - cameraBuffer;
        float maxY = maxScreen.y + cameraBuffer;

        var validPoints = allSpawnPoints.Where(p =>
            p.position.x < minX || p.position.x > maxX ||
            p.position.y < minY || p.position.y > maxY
        ).ToList();

        if (validPoints.Count > 0) return validPoints[Random.Range(0, validPoints.Count)];
        return null;
    }

    Transform GetFurthestSpawnPoint()
    {
        if (allSpawnPoints == null || allSpawnPoints.Count == 0) return null;
        return allSpawnPoints.OrderByDescending(p => Vector3.Distance(p.position, mainCam.transform.position)).FirstOrDefault();
    }
}