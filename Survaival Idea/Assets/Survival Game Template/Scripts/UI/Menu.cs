using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Menu : MonoBehaviour {

    Canvas canvas;
    NetworkManagerHUDChanged networkHUD;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        networkHUD = GameObject.Find("Network").GetComponent<NetworkManagerHUDChanged>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Press();
        }
    }

    public void Press()
    {
        canvas.enabled = !canvas.enabled;
        if(!networkHUD.GetComponent<PlayerData>().isSingle)
            networkHUD.enabled = !networkHUD.enabled;

        if (canvas.enabled)        
            Cursor.visible = true;        
        else if (!Global.inventoryControl.IsInventoryOrContainersActive())
            Cursor.visible = false;

        Pause();

        Global.inventoryControl.SetUIEnabled(!canvas.enabled);
    }

    public void Pause()
    {
        if (networkHUD.GetComponent<PlayerData>().isSingle)
            Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }

    public void MainMenu()
    {
        networkHUD.GetComponent<NetworkManager>().StopHost();
    }    

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
}
