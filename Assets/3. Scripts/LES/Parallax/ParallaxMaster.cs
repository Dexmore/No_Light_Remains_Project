using UnityEngine;

namespace Game.Visuals
{
    public class ParallaxMaster : MonoBehaviour
    {
        public enum LayerType { Custom = 0, SkyFixed, BackgroundFar, BackgroundMid, Foreground }

        [Header("1. Movement Settings")]
        public bool useZCoordinate = false;
        public float maxDepth = 10f; 
        public LayerType layerType = LayerType.Custom;
        [Range(-1f, 1f)] public float parallaxFactor;
        public float autoMoveSpeedX = 0f; 
        public bool lockYAxis = true;

        [Header("2. Activation Settings")]
        public bool useActivationRange = false;
        public float activationRange = 20f;

        [Header("3. Limit Settings")]
        public bool limitMovement = false;
        public Vector2 maxMoveRange = new Vector2(5f, 2f);

        [Header("4. Infinite Loop Settings (Visualized)")]
        public bool infiniteLoop = false;
        public float singleImageWidth = 0f; 
        public int cloneCount = 3; 
        public float loopThreshold = 0f; 

        [Header("5. Visual Settings")]
        public Color layerColor = Color.white;
        public bool applyColorToChildren = true;

        private Transform cameraTransform;
        private Vector3 lastCameraPosition;
        private Vector3 initialPosition;

        private void OnEnable()
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                lastCameraPosition = cameraTransform.position;

                // [핵심 기능] 켜질 때 카메라 위치로 즉시 스냅 (씬 이동 후 복귀 시 튀는 문제 해결)
                if (infiniteLoop)
                {
                    SnapToCamera();
                }
            }
        }

        private void OnValidate()
        {
            if (!useZCoordinate) ApplyPreset();
            ApplyColor();
            if (infiniteLoop && loopThreshold == 0 && singleImageWidth > 0)
            {
                 loopThreshold = singleImageWidth * 1.5f; 
            }
        }

        void Start()
        {
            if (Camera.main != null) cameraTransform = Camera.main.transform;
            lastCameraPosition = cameraTransform.position;
            initialPosition = transform.position;
            
            CalculateSize();
            ApplyColor();

            // 시작 시에도 스냅 실행
            if (infiniteLoop) SnapToCamera();
        }

        // [New] 배경을 카메라 근처로 강제 이동시키는 함수
        private void SnapToCamera()
        {
            if (singleImageWidth <= 0 || cloneCount <= 0 || cameraTransform == null) return;

            float totalLoopSize = singleImageWidth * cloneCount;
            float dist = cameraTransform.position.x - transform.position.x;

            // 카메라와 배경 사이의 거리가 루프 절반 이상 벌어지면
            if (Mathf.Abs(dist) >= totalLoopSize / 2f)
            {
                // 필요한 점프 횟수 계산
                int numJumps = Mathf.RoundToInt(dist / totalLoopSize);
                // 즉시 이동
                transform.position += new Vector3(numJumps * totalLoopSize, 0, 0);
            }
        }

        [ContextMenu("Auto Calculate Size")]
        public void CalculateSize()
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite == null)
            {
                SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>();
                float maxWidth = 0f;
                foreach(var child in children)
                {
                    if(child.bounds.size.x > maxWidth) maxWidth = child.bounds.size.x;
                }
                singleImageWidth = maxWidth;
            }
            else
            {
                singleImageWidth = sprite.bounds.size.x;
            }

            // 점프 타이밍을 넉넉하게 설정
            loopThreshold = singleImageWidth * 1.5f; 
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        void LateUpdate()
        {
            if (cameraTransform == null) return;

            if (useActivationRange) {
                if (Mathf.Abs(cameraTransform.position.x - transform.position.x) > activationRange) {
                    lastCameraPosition = cameraTransform.position; return; 
                }
            }

            // Move Logic
            if (useZCoordinate) {
                parallaxFactor = Mathf.Clamp01(Mathf.Abs(transform.position.z) / maxDepth);
                if (transform.position.z < 0) parallaxFactor = -0.2f; 
            }
            Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
            float parallaxX = deltaMovement.x * parallaxFactor;
            float parallaxY = lockYAxis ? 0 : deltaMovement.y * parallaxFactor;
            transform.position += new Vector3(parallaxX + (autoMoveSpeedX * Time.deltaTime), parallaxY, 0);
            
            if (limitMovement && !infiniteLoop) {
                 float distFromOriginX = transform.position.x - initialPosition.x;
                float distFromOriginY = transform.position.y - initialPosition.y;
                transform.position = new Vector3(initialPosition.x + Mathf.Clamp(distFromOriginX, -maxMoveRange.x, maxMoveRange.x), initialPosition.y + Mathf.Clamp(distFromOriginY, -maxMoveRange.y, maxMoveRange.y), transform.position.z);
            }

            // Infinite Loop Logic
            if (infiniteLoop && singleImageWidth > 0 && !limitMovement)
            {
                float offsetPosX = cameraTransform.position.x - transform.position.x;
                
                if (Mathf.Abs(offsetPosX) >= loopThreshold)
                {
                    float totalLoopSize = singleImageWidth * cloneCount;
                    float direction = offsetPosX > 0 ? 1 : -1;
                    transform.position += new Vector3(totalLoopSize * direction, 0, 0);
                }
            }
            lastCameraPosition = cameraTransform.position;
        }

        private void OnDrawGizmosSelected()
        {
            if (singleImageWidth > 0) { 
                Gizmos.color = Color.cyan; 
                Gizmos.DrawWireCube(transform.position, new Vector3(singleImageWidth, 10f, 0)); 
            }
            if (infiniteLoop) { 
                Gizmos.color = Color.yellow; 
                float th = loopThreshold > 0 ? loopThreshold : singleImageWidth * 1.5f; 
                Gizmos.DrawLine(new Vector3(transform.position.x - th, transform.position.y - 10, 0), new Vector3(transform.position.x - th, transform.position.y + 10, 0)); 
                Gizmos.DrawLine(new Vector3(transform.position.x + th, transform.position.y - 10, 0), new Vector3(transform.position.x + th, transform.position.y + 10, 0)); 
            }
            if (useActivationRange) { 
                Gizmos.color = Color.green; 
                Gizmos.DrawWireCube(transform.position, new Vector3(activationRange * 2, 20f, 0)); 
            }
        }
        private void ApplyPreset()
        {
            switch (layerType)
            {
                case LayerType.SkyFixed: parallaxFactor = 1.0f; break;
                case LayerType.BackgroundFar: parallaxFactor = 0.9f; break;
                case LayerType.BackgroundMid: parallaxFactor = 0.5f; break;
                case LayerType.Foreground: parallaxFactor = -0.5f; break;
            }
        }

        private void ApplyColor()
        {
            if (applyColorToChildren)
            {
                SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
                foreach (var sprite in renderers) sprite.color = layerColor;
            }
            else
            {
                SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
                if (mySprite != null) mySprite.color = layerColor;
            }
        }
    }
}