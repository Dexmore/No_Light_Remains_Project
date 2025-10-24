public interface ITabContent
{
    //이 탭이 활성화될 때 TabGroup에 의해 호출됩니다.
    //UI 갱신, 데이터 로딩 등을 여기에 구현합니다.
    void OnShow();

    //이 탭이 비활성화될 때 TabGroup에 의해 호출됩니다.
    //애니메이션 정지, 상태 저장 등을 여기에 구현합니다.
    void OnHide();
}