using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour {
    NetworkManagerHUDChanged hud;
    PlayerData data;

    public GameObject multPanel;
    public GameObject mainPanel;
    // Use this for initialization
    void Start () {
        hud = GameObject.Find("Network").GetComponent<NetworkManagerHUDChanged>();
        data = GameObject.Find("Network").GetComponent<PlayerData>();
        Time.timeScale = 1;
        Cursor.visible = true;

        data.playerName = "";
        data.playerPassword = "";

        hud.enabled = false;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    public void setMultiplayer()
    {
        data.isSingle = false;

        hud.enabled = true;

        multPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void RunSingle()
    {
        data.playerName = "single";
        data.isSingle = true;

        NetworkManager manager =
        data.GetComponent<NetworkManager>();

        manager.StartHost();
    }

    public void Back()
    {
        hud.enabled = false;
    }

    public void SetNickname(string value)
    {
        data.setName(value);
    }

    public void SetPassword(string value)
    {
        data.setPassword(value);
    }

}
