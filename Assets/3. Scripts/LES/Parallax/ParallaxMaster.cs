using UnityEngine;

namespace Game.Visuals
{
    public class ParallaxMaster : MonoBehaviour
    {
        public enum LayerType
        {
            Custom = 0, SkyFixed, BackgroundFar, BackgroundMid, Foreground
        }

        [Header("1. Movement Settings")]
        [Tooltip("체크하면 Z축 깊이에 따라 Parallax Factor가 자동 결정됩니다.")]
        public bool useZCoordinate = false;
        public float maxDepth = 10f; 

        public LayerType layerType = LayerType.Custom;
        
        [Range(-1f, 1f)]
        public float parallaxFactor;
        
        [Tooltip("구름/달 처럼 카메라와 상관없이 스스로 움직이는 속도")]
        public float autoMoveSpeedX = 0f; // [New] 자동 이동 기능 추가

        public bool lockYAxis = true;

        [Header("2. Infinite Loop Settings")]
        public bool infiniteLoop = false;
        
        [Tooltip("이미지 1개의 가로 길이 (필수 입력)")]
        public float singleImageWidth = 0f; 

        [Tooltip("배경 3개를 쓸 경우: 3을 입력 (점프 거리 계산용)")]
        public int cloneCount = 3; 

        [Tooltip("카메라 중심에서 이 거리만큼 멀어지면 점프합니다. (이미지 너비보다 약간 크게 설정 권장)")]
        public float loopThreshold = 16f;

        [Header("3. Visual Settings")]
        public Color layerColor = Color.white;
        public bool applyColorToChildren = true;

        private Transform cameraTransform;
        private Vector3 lastCameraPosition;

        private void OnValidate()
        {
            if (!useZCoordinate) ApplyPreset();
            ApplyColor();
        }

        void Start()
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            
            lastCameraPosition = cameraTransform.position;

            // 자동 계산: 0이면 스프라이트 렌더러에서 가져옴
            if (infiniteLoop && singleImageWidth == 0)
            {
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                if (sprite != null)
                    singleImageWidth = sprite.bounds.size.x;
            }
            
            ApplyColor();
        }

        void LateUpdate()
        {
            if (cameraTransform == null) return;

            // 1. Z축 기반 Parallax Factor 계산
            if (useZCoordinate)
            {
                parallaxFactor = Mathf.Clamp01(Mathf.Abs(transform.position.z) / maxDepth);
                if (transform.position.z < 0) parallaxFactor = -0.2f; 
            }

            // 2. 카메라 이동에 따른 Parallax 이동
            Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
            float parallaxX = deltaMovement.x * parallaxFactor;
            float parallaxY = lockYAxis ? 0 : deltaMovement.y * parallaxFactor;

            // 3. 자동 이동 (구름, 달) 추가 [New]
            float autoMoveX = autoMoveSpeedX * Time.deltaTime;

            // 최종 이동 적용
            transform.position += new Vector3(parallaxX + autoMoveX, parallaxY, 0);

            // 4. 무한 루프 로직 (개선됨)
            // 핵심: "카메라와의 거리"가 "Threshold"를 넘으면 "전체 길이(Width * Count)"만큼 점프
            if (infiniteLoop && singleImageWidth > 0)
            {
                float offsetPosX = cameraTransform.position.x - transform.position.x;

                // 왼쪽이나 오른쪽으로 너무 멀어지면
                if (Mathf.Abs(offsetPosX) >= loopThreshold)
                {
                    // 전체 루프 길이 (이미지 1개 너비 * 개수)
                    float totalLoopSize = singleImageWidth * cloneCount;
                    
                    // 방향 결정 (카메라가 오른쪽으로 갔으면 배경은 오른쪽 끝으로 보내야 함)
                    float direction = offsetPosX > 0 ? 1 : -1;
                    
                    // 점프!
                    transform.position += new Vector3(totalLoopSize * direction, 0, 0);
                }
            }

            lastCameraPosition = cameraTransform.position;
        }

        private void ApplyPreset() { /* 기존과 동일 */ }
        private void ApplyColor() { /* 기존과 동일 */ }
    }
}