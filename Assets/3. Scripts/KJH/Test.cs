using UnityEngine;
using UnityEngine.InputSystem; // New Input System 네임스페이스 추가
using System.Collections.Generic;

public class ColliderListerNewInput : MonoBehaviour
{
    // === public 설정 변수 ===
    [Header("Detection Settings")]
    [Tooltip("체크하면 OverlapPoint 방식으로 감지합니다. 해제하면 RaycastAll 방식으로 감지합니다.")]
    public bool useOverlapPoint = true; // Overlap 방식 사용 여부 (토글)
    
    // RaycastAll 방식에서 사용할 최대 거리 (OverlapPoint는 이 값을 사용하지 않음)
    public float raycastDistance = 100f; 
    public float overlapRadius = 0.1f;

    // === private 내부 변수 ===
    private List<Collider2D> hitColliders = new List<Collider2D>();

    [SerializeField]
    private List<string> colliderNames = new List<string>();

    void Update()
    {
        if (Mouse.current == null) return;
        
        // 1. 마우스 위치를 월드 좌표로 변환
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        hitColliders.Clear();
        colliderNames.Clear();
        
        // 2. 선택된 방식에 따라 콜라이더 감지
        if (useOverlapPoint)
        {
            DetectCollidersOverlap(mouseWorldPos);
        }
        else
        {
            DetectCollidersRaycast(mouseWorldPos);
        }
    }
    
    // --- OverlapPoint 방식 감지 메서드 ---
    private void DetectCollidersOverlap(Vector2 position)
    {
        // Physics2D.OverlapPointAll을 사용하여 특정 지점의 모든 콜라이더를 감지
        Collider2D[] overlapHits = new Collider2D[10];
        int count = Physics2D.OverlapCircleNonAlloc(position, overlapRadius, overlapHits);

        for (int i = 0; i < count; i++)
        {
            AddCollider(overlapHits[i]);
        }
    }
    
    // --- RaycastAll 방식 감지 메서드 ---
    private void DetectCollidersRaycast(Vector2 position)
    {
        // RaycastAll은 방향 벡터가 필요하며, PointCast와 유사하게 Vector2.zero를 사용해도 작동함
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero, raycastDistance);
        
        foreach (RaycastHit2D hit in hits)
        {
            AddCollider(hit.collider);
        }
    }

    // --- 콜라이더 리스트에 추가하는 헬퍼 메서드 ---
    private void AddCollider(Collider2D col)
    {
        if (col != null && !hitColliders.Contains(col))
        {
            hitColliders.Add(col);
            colliderNames.Add(col.gameObject.name);
        }
    }
    

}