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
            }
        }

        private void OnValidate()
        {
            if (!useZCoordinate) ApplyPreset();
            ApplyColor();
            // 에디터에서 값 수정 시 최소한의 안전장치
            // (여기서는 강제하지 않고 사용자의 입력을 존중하되, 0일 때만 자동 적용)
            if (infiniteLoop && loopThreshold == 0 && singleImageWidth > 0)
            {
                 // 안전하게 1.5배로 자동 설정
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

            // [핵심 수정] 점프 타이밍을 이미지 너비보다 50% 더 넓게 잡아서
            // 양옆에 붙어있는 친구들이 시작하자마자 점프하는 참사를 방지함
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
                
                // loopThreshold(노란선)를 넘어가면 점프
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
                // 임계값이 0이면 시각적으로 1.5배로 보여줌 (안전을 위해)
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