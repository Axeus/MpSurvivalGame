using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DayNightCycle : NetworkBehaviour {

	public Gradient nightDayColor;

	public float maxIntensity = 3f;
	public float minIntensity = 0f;
	public float minPoint = -0.2f;

	public Gradient nightDayFogColor;

	public float dayAtmosphereThickness = 0.4f;
	public float nightAtmosphereThickness = 0.87f;

    public float updateFreq = 3;
    public float updateDuration = 1f;

    [SyncVar]
    public float skySpeed = 1; 

    [SyncVar]
    public float time = 0f;
    public AudioSource ambient;
   
    public float nightTime = 18f;
    public float dayTime = 6f;
    public float hoursInDay = 24f;

    float timeLoc;
    Light mainLight;
    Material skyMat;

    Transform thisTransform;

    ParticleSystemRenderer stars;
    Vector3 rotateDirection;

    float upd;
    float updDur;

    float alpha = 1;

    float dot;
    bool isNight;

    void Start () 
	{       
        rotateDirection = new Vector3(-1, 0, 0);

        thisTransform = transform;
        timeLoc = time;

        if (isServer)
            thisTransform.eulerAngles = (rotateDirection * (time/ hoursInDay)) * 360f + rotateDirection * 90f;

        stars = transform.FindChild("Stars").GetComponent<ParticleSystemRenderer>();

        if (updateFreq < 1f)
        {
            updateFreq = 0;
            updateDuration = 0;
        }
        upd = updateFreq;

        mainLight = GetComponent<Light>();
		skyMat = RenderSettings.skybox;

        if (time > nightTime || time < dayTime)
            isNight = true;

        if (!isNight)
            stars.material.SetColor("_TintColor", new Color(1f, 1f, 1f, 0f));
        else
            ambient.volume = 0f;
    }

    void Update()
    {
        if(isServer)
            time += (Time.deltaTime * skySpeed) / 30f;

        if (time > hoursInDay)
        {
            if (isServer)
                time -= hoursInDay;

            timeLoc -= hoursInDay;
        }

        if (time > nightTime)
            isNight = true;            
        else if (time > dayTime)
            isNight = false;
            
        if(isNight)
            ambient.volume -= Time.deltaTime / (50f / skySpeed);
        else
            ambient.volume += Time.deltaTime / (50f / skySpeed);

        if (upd >= updateFreq)
        {
            updDur += Time.deltaTime;

            if (updDur >= updateDuration)
            {
                upd = 0;
                updDur = 0;
                timeLoc = time;
            }

            float tRange = 1 - minPoint;
            dot = Mathf.Clamp01((Vector3.Dot(thisTransform.forward, Vector3.down) - minPoint) / tRange);
            float i = ((maxIntensity - minIntensity) * dot) + minIntensity;

            mainLight.intensity = i;
            mainLight.color = nightDayColor.Evaluate(dot);

            RenderSettings.fogColor = nightDayFogColor.Evaluate(dot);

            i = ((dayAtmosphereThickness - nightAtmosphereThickness) * dot) + nightAtmosphereThickness;
            skyMat.SetFloat("_AtmosphereThickness", i);

            float freq = updateFreq;
            if (freq == 0)
                freq = 1f;

            float timeLoc2 = timeLoc;

            if (timeLoc != time)
                timeLoc2 = Mathf.Lerp(timeLoc, time, updDur / updateDuration);               

            thisTransform.localRotation = Quaternion.Euler((rotateDirection * (timeLoc2/ hoursInDay)) * 360f + rotateDirection * 90f);
        }
        else
            upd += Time.deltaTime;

        if (isServer)
        {
            if (Input.GetKeyUp(KeyCode.KeypadMinus)) skySpeed *= 0.5f;
            if (Input.GetKeyUp(KeyCode.KeypadPlus)) skySpeed *= 5f;
        }       

        if (!isNight)
        {
            if (alpha > 0)
            {
                alpha -= Time.deltaTime;

                stars.material.SetColor("_TintColor", new Color(1f, 1f, 1f, alpha));
            }
        }
        else
        {
            if (alpha < 1)
            {
                alpha += Time.deltaTime;

                stars.material.SetColor("_TintColor", new Color(1f, 1f, 1f, alpha));
            }
        }
    }

}
