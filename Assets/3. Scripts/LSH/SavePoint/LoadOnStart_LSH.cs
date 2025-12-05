/*
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadOnStart_LSH : MonoBehaviour
{
    [Header("옵션")]
    public KeyCode debugKey = KeyCode.L; // 테스트용

    //Start에서 정보 불러오기
    void Start()
    {
        var data = SaveManager_LSH.Load();
        if (data == null)
        {
            Debug.Log("[Load] No save found");
        }
        else
        {
            Debug.Log($"[Load] Save exists (scene=\"{data.scene}\", x={data.x}, y={data.y})");
        }
    }

    // 테스트용
    void Update()
    {
        if (debugKey != KeyCode.None && Input.GetKeyDown(debugKey))
        {
            ApplySavedPosition();
        }
    }

    // 상호작용에서 호출로 사용가능
    public void ApplySavedPosition()
    {
        var data = SaveManager_LSH.Load();
        if (data == null)
        {
            Debug.Log("[Load] No save to apply");
            return;
        }

        if (data.scene != SceneManager.GetActiveScene().name)
        {
            Debug.Log($"[Load] Loading scene {data.scene}");
            SceneManager.LoadScene(data.scene);
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[Load] Player not found!");
            return;
        }

        player.transform.position = new Vector3(
            data.x,
            data.y,
            player.transform.position.z
        );

        if (Camera.main != null)
        {
            var cam = Camera.main;
            Vector3 camPos = cam.transform.position;
            camPos.x = data.x;
            camPos.y = data.y;
            cam.transform.position = camPos;
        }

        Debug.Log("[Load] Player moved to saved point");
    }
}
*/