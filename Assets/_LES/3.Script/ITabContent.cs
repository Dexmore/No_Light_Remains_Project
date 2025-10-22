// 이 스크립트는 MonoBehaviour를 상속하지 않는 순수 C# 인터페이스입니다.
public interface ITabContent
{
    /// <summary>
    /// 이 탭이 활성화될 때 TabGroup에 의해 호출됩니다.
    /// UI 갱신, 데이터 로딩 등을 여기에 구현합니다.
    /// </summary>
    void OnShow();

    /// <summary>
    /// 이 탭이 비활성화될 때 TabGroup에 의해 호출됩니다.
    /// 애니메이션 정지, 상태 저장 등을 여기에 구현합니다.
    /// </summary>
    void OnHide();
}