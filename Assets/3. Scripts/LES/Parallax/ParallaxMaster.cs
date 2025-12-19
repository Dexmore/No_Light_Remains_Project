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
        [Tooltip("체크하면 Z축 깊이에 따라 속도가 자동 결정됩니다. (Custom 모드 권장)")]
        public bool useZCoordinate = false; // 새로 추가된 기능

        [Tooltip("Z값을 사용할 때의 기준 깊이 (이 값만큼 멀어지면 고정됨)")]
        public float maxDepth = 10f; 

        public LayerType layerType = LayerType.Custom;
        
        [Range(-1f, 1f)]
        public float parallaxFactor;
        
        public bool lockYAxis = true;
        public bool infiniteLoop = false;
        public float customLoopWidth = 0f;

        [Header("2. Visual Settings")]
        public Color layerColor = Color.white;
        public bool applyColorToChildren = true;

        private Transform cameraTransform;
        private Vector3 lastCameraPosition;
        private float textureUnitSizeX;

        private void OnValidate()
        {
            // Z좌표 모드가 꺼져있을 때만 프리셋 적용
            if (!useZCoordinate) ApplyPreset();
            ApplyColor();
        }

        void Start()
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            
            lastCameraPosition = cameraTransform.position;

            if (infiniteLoop)
            {
                SpriteRenderer sprite = GetComponent<SpriteRenderer>();
                if (sprite != null && customLoopWidth == 0)
                    textureUnitSizeX = sprite.bounds.size.x;
                else
                    textureUnitSizeX = customLoopWidth;
            }
            
            ApplyColor();
        }

        void LateUpdate()
        {
            if (cameraTransform == null) return;

            // 핵심: Z좌표 사용 모드일 때 Factor를 실시간 계산
            if (useZCoordinate)
            {
                // Z값이 클수록(멀수록) 1에 가까워짐(고정), 0이면 0(따라감)
                // 예: Z=10, MaxDepth=10 -> Factor 1.0 (하늘)
                // 예: Z=5,  MaxDepth=10 -> Factor 0.5 (중경)
                parallaxFactor = Mathf.Clamp01(Mathf.Abs(transform.position.z) / maxDepth);
                
                // 만약 전경(Z가 음수)이라면 반대로 처리
                if (transform.position.z < 0) parallaxFactor = -0.2f; // 전경은 약간 빠르게
            }

            Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
            
            float parallaxX = deltaMovement.x * parallaxFactor;
            float parallaxY = lockYAxis ? 0 : deltaMovement.y * parallaxFactor;

            // 좌표 이동 (Z값은 건드리지 않음 -> 그래야 정렬이 유지됨)
            transform.position += new Vector3(parallaxX, parallaxY, 0);

            if (infiniteLoop && textureUnitSizeX > 0)
            {
                float offsetPosX = cameraTransform.position.x - transform.position.x;
                if (Mathf.Abs(offsetPosX) >= textureUnitSizeX)
                {
                    float offsetMultiplier = offsetPosX > 0 ? 1 : -1;
                    transform.position += new Vector3(textureUnitSizeX * offsetMultiplier, 0, 0);
                }
            }

            lastCameraPosition = cameraTransform.position;
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