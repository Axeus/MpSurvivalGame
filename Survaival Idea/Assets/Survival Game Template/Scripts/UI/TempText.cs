using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TempText : MonoBehaviour {
   
    public float time = 2f;

    private Text tempTextObj;
    private float timer;
    bool fade;

	void Start () {
        tempTextObj = GetComponent<Text>();
        tempTextObj.enabled = false;
    }
	
	void Update () {
        if(timer > 0)
        {
            timer -= Time.deltaTime;

            if (!fade && timer < time / 2f)
            {
                tempTextObj.CrossFadeAlpha(0.01f, time / 2f, true);
                fade = true;
            }

            if (timer <= 0)
            {
                tempTextObj.enabled = false;
                tempTextObj.text = "";
            }
        }
	}

    public void setText(string txt)
    {
        if (!fade)
        {         
            tempTextObj.text = tempTextObj.text + "\n" + txt;
        }
        else
        {
            tempTextObj.text = txt;
            tempTextObj.enabled = true;
        }

        fade = false;
        tempTextObj.canvasRenderer.SetAlpha(1f);
        tempTextObj.StopAllCoroutines();

        timer = time;
    }
}
