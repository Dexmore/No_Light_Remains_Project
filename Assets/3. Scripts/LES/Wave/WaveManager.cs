using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 리스트 필터링을 위해 사용

[System.Serializable]
public class SpawnEntry
{
    [Tooltip("생성할 몬스터 프리팹")]
    public GameObject monsterPrefab;

    [Tooltip("이 종류의 몬스터를 몇 마리 생성할 것인가")]
    public int count = 1;

    [Tooltip("몬스터 간 생성 간격 (초)")]
    public float spawnInterval = 0.5f;
}

[System.Serializable]
public class Wave
{
    [Tooltip("이 웨이브에 등장할 몬스터 구성")]
    public List<SpawnEntry> spawnEntries;

    [Tooltip("웨이브 시작 전 대기 시간")]
    public float delayBeforeWave = 2.0f;

    [Header("Clear Condition")]
    [Tooltip("체크(True): 모든 몬스터를 다 죽여야 다음 웨이브 진행 (섬멸전)\n해제(False): 몬스터가 살아있어도 아래 시간(Duration)이 지나면 다음 웨이브 진행 (난전)")]
    public bool waitForClear = true;

    [Tooltip("waitForClear가 꺼져있을 때 적용됨. 스폰 완료 후 다음 웨이브로 넘어가기까지 버티는 시간")]
    public float waveDuration = 2.0f;
}

public class WaveManager : MonoBehaviour
{
    [Header("--- Settings ---")]
    [Tooltip("맵 곳곳에 배치한 스폰 위치들 (빈 GameObject)")]
    public List<Transform> allSpawnPoints;

    [Tooltip("카메라 밖으로 판단할 여유 공간 (클수록 카메라에서 더 멀리 떨어진 곳 찾음)")]
    public float cameraBuffer = 1.0f;

    [Header("--- Waves Config (기획자 설정) ---")]
    public List<Wave> waves;

    // 내부 상태 변수
    private List<GameObject> currentActiveMonsters = new List<GameObject>();
    private Camera mainCam;
    private int currentWaveIndex = 0;

    private bool isBattleStarted = false;

    void Start()
    {
        mainCam = Camera.main;
    }

    // 외부(Trigger)에서 호출할 공개 함수
    public void StartBattle()
    {
        if (isBattleStarted) return; // 이미 시작됐다면 무시

        isBattleStarted = true;
        StartCoroutine(ExecuteWaves());
    }

    IEnumerator ExecuteWaves()
    {
        foreach (var wave in waves)
        {
            currentWaveIndex++;
            Debug.Log($"=== Wave {currentWaveIndex} Start (Type: {(wave.waitForClear ? "Elimination" : "Survival")}) ===");

            // 웨이브 시작 전 딜레이
            yield return new WaitForSeconds(wave.delayBeforeWave);

            // 1. 몬스터 스폰 진행
            foreach (var entry in wave.spawnEntries)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    TrySpawnMonsterOffScreen(entry.monsterPrefab);
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            // 2. 클리어 조건 확인 (여기가 핵심 변경 사항)
            if (wave.waitForClear)
            {
                if (currentActiveMonsters.Count > 0)
                {
                    Debug.Log("Final Wave Logic Finished. Eliminating remaining enemies...");

                    // [A] 섬멸 모드: 맵 상의 모든 몬스터가 0마리가 될 때까지 무한 대기
                    while (true)
                    {
                        currentActiveMonsters.RemoveAll(m => m == null); // 죽은 놈 정리
                        if (currentActiveMonsters.Count == 0) break; // 다 죽었으면 탈출
                        yield return new WaitForSeconds(0.5f);
                    }
                }

                Debug.Log($"=== Wave {currentWaveIndex} Cleared (All Killed) ===");
            }
            else
            {
                // [B] 서바이벌 모드: 몬스터 생존 여부 상관없이 지정된 시간만큼 버티면 통과
                // 플레이어는 남은 몬스터 + 다음 웨이브 몬스터를 동시에 상대해야 함 (난이도 상승 요소)
                Debug.Log($"=== Wave {currentWaveIndex} Spawn Finished. Surviving for {wave.waveDuration}s... ===");
                yield return new WaitForSeconds(wave.waveDuration);
                Debug.Log($"=== Wave {currentWaveIndex} Passed (Time Over) ===");
            }
        }

        Debug.Log("🎉 STAGE CLEARED 🎉");
        // TODO: Clear UI Logic
    }

    /// <summary>
    /// 카메라 밖의 유효한 스폰 포인트를 찾아 몬스터를 생성합니다.
    /// </summary>
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
            Debug.LogWarning("카메라 밖 스폰 포인트를 찾지 못했습니다. 플레이어에게서 가장 먼 곳에 강제 스폰합니다.");
            // 비상 대책: 카메라 안이라도 가장 먼 곳에 생성 (게임 멈춤 방지)
            Transform fallbackSpot = GetFurthestSpawnPoint();
            GameObject mon = Instantiate(prefab, fallbackSpot.position, Quaternion.identity);
            currentActiveMonsters.Add(mon);
        }
    }

    /// <summary>
    /// 현재 카메라 뷰포트(화면) 밖에 있는 스폰 포인트 중 하나를 랜덤 반환
    /// </summary>
    Transform GetOffScreenSpawnPoint()
    {
        // 카메라가 비추는 월드 좌표 영역 계산
        // 뷰포트 (0,0) -> 좌하단, (1,1) -> 우상단
        Vector3 minScreen = mainCam.ViewportToWorldPoint(new Vector3(0, 0, mainCam.nearClipPlane));
        Vector3 maxScreen = mainCam.ViewportToWorldPoint(new Vector3(1, 1, mainCam.nearClipPlane));

        // Z축 고려가 필요 없다면 2D 게임 기준 로직 적용
        float minX = minScreen.x - cameraBuffer;
        float maxX = maxScreen.x + cameraBuffer;
        float minY = minScreen.y - cameraBuffer;
        float maxY = maxScreen.y + cameraBuffer;

        // 조건에 맞는(화면 밖) 포인트들만 추출
        var validPoints = allSpawnPoints.Where(p =>
            p.position.x < minX || p.position.x > maxX || // 좌우 밖
            p.position.y < minY || p.position.y > maxY    // 상하 밖
        ).ToList();

        if (validPoints.Count > 0)
        {
            // 그 중 랜덤 하나 선택
            return validPoints[Random.Range(0, validPoints.Count)];
        }

        return null; // 모든 포인트가 화면 안에 있음
    }

    Transform GetFurthestSpawnPoint()
    {
        Vector3 cameraPos = mainCam.transform.position;
        return allSpawnPoints.OrderByDescending(p => Vector3.Distance(p.position, cameraPos)).FirstOrDefault();
    }

    // 에디터에서 스폰 포인트 위치를 쉽게 보기 위한 기즈모
    private void OnDrawGizmos()
    {
        if (allSpawnPoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (var p in allSpawnPoints)
        {
            if (p != null) Gizmos.DrawWireSphere(p.position, 0.5f);
        }
    }
}