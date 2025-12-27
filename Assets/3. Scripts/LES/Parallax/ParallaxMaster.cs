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

                // [핵심 해결책] 켜지자마자 카메라 근처로 즉시 이동 (스냅)
                // 이것이 없으면 멀리서 돌아왔을 때 배경이 따라오는데 한참 걸림
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

            // 시작할 때도 스냅 한 번 실행
            if (infiniteLoop) SnapToCamera();
        }

        // [New] 카메라 위치로 배경을 즉시 소환하는 함수
        private void SnapToCamera()
        {
            if (singleImageWidth <= 0 || cloneCount <= 0) return;

            float totalLoopSize = singleImageWidth * cloneCount;
            float dist = cameraTransform.position.x - transform.position.x;

            // 카메라와의 거리가 너무 멀면, 루프 사이즈 단위로 계산해서 한 번에 이동
            if (Mathf.Abs(dist) >= totalLoopSize / 2f) // 대략 절반 이상 멀어지면
            {
                // 몇 바퀴(몇 번의 점프)를 돌아야 하는지 정수로 계산
                int numJumps = Mathf.RoundToInt(dist / totalLoopSize);
                
                // 해당 횟수만큼 좌표 이동
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

            // Move
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

            // Infinite Loop
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