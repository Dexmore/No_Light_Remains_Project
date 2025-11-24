using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadOnStart_LSH : MonoBehaviour
{
    void Start()
    {
        SavePos data = SaveManager_LSH.Load();
        if (data == null)
        {
            Debug.Log("[Load] No save found");
            return;
        }

        if (data.scene != SceneManager.GetActiveScene().name)
        {
            SceneManager.LoadScene(data.scene);
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[Load] Player not found! Tag가 Player인지 확인하세요.");
            return;
        }

        player.transform.position = new Vector3(
            data.x,
            data.y,
            player.transform.position.z
        );

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 camPos = cam.transform.position;
            camPos.x = data.x;
            camPos.y = data.y;
            cam.transform.position = camPos;
        }

        Debug.Log("[Load] Player & Camera moved to saved point");
    }
}
