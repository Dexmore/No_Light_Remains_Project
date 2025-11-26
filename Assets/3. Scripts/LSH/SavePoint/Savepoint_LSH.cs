using UnityEngine;
using UnityEngine.SceneManagement;
public class Savepoint_LSH : MonoBehaviour
{
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        Vector2 pos = transform.position;
        string scene = SceneManager.GetActiveScene().name;

        SaveManager_LSH.Save(scene, pos);

        Debug.Log("[Checkpoint] Saved checkpoint at " + pos);
    }
}
