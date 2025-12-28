// 상호작용 완료된 오브젝트가 씬이 넘어가도 (또는 저장후 게임을 다시켜도) 완료된 상태로 유지하기 위한 인터페이스
public interface ISavable
{
    // 이 인터페이스를 상속받는 모노비헤이비어 트랜스폼을 연결할것
    public UnityEngine.Transform transform {get;}

    // 상속받는 각각의 오브젝트 스크립트에서 완료시 꼭 아래 값을 true로 할것
    public bool IsComplete { get; protected set; }
    // 바로 완료 되어있는 상태로 만드는 메소드를 만들것 (ex. 바로 아무것도 나오지않고 텅 열려있는 형태의 상자, 바로 열려져 있는 문 등등)
    public void SetCompletedImmediately();

    // 계정의 한 캐릭터당 한번 완료하면 '영원히 완료상태' 인 오브젝트의 경우 아래 것들은 필요 없음
    // 영원히 완료되는 1회한정 오브젝트들은 아래 값들을 CanReplay = false , ReplayWaitTimeSecond = 0 으로 하고.

    // 만약 일일퀘스트 오브젝트라던지 '일정시간이 지나면 다시 씬에 돌아왔을때 작동할수있는 오브젝트'인 경우
    // CanReplay = true , ReplayWaitTimeSecond = (해당 오브젝트에 알맞게 시간 셋팅) 으로 해야함

    public bool CanReplay { get; }
    public int ReplayWaitTimeSecond { get; }
    

}
