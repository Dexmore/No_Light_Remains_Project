using UnityEngine;
public class FallZone : MonoBehaviour
{
    void Start()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        monsterLayer = LayerMask.NameToLayer("Monster");
    }

    int playerLayer;
    int monsterLayer;

    void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rigidbody = collision.GetComponentInChildren<Rigidbody2D>();
        if (rigidbody == null)
            rigidbody = collision.GetComponentInParent<Rigidbody2D>();

        if (rigidbody == null) return;

        if (collision.gameObject.layer == playerLayer)
        {
            PlayerControl playerControl = collision.GetComponentInChildren<PlayerControl>();
            if (playerControl == null)
                playerControl = collision.GetComponentInParent<PlayerControl>();

            if (playerControl == null) return;

            rigidbody.simulated = false;
            playerControl.currHealth = 0f;
            playerControl.fsm.ChangeState(playerControl.die);
            HitData hitData = new HitData();
            hitData.attackName = "Fall";
            hitData.attacker = transform;
            hitData.target = playerControl.transform;
            GameManager.I.onDie.Invoke(hitData);

        }
        else if (collision.gameObject.layer == monsterLayer)
        {
            MonsterControl monsterControl = collision.GetComponentInChildren<MonsterControl>();
            if (monsterControl == null)
                monsterControl = collision.GetComponentInParent<MonsterControl>();

            if (monsterControl == null) return;

            rigidbody.simulated = false;
            monsterControl.currHealth = 0f;

        }


        //DBManager.I.AddGear("CounterGeaer");
    }





}
