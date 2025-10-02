using UnityEngine;

public interface IParry_LSH
{
    // 공격이 들어온 쪽(플레이어)에서 패링 성공 여부를 판정하여
    // true를 반환하면 해당 공격은 무효화됩니다.
    bool TryParry(object attackSource, Vector3 hitPoint);
}