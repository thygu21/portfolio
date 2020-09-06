using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{
    private GameObject infoborder;
    public AudioMixer mainmix;
    Image soundImg;
    bool soundOn;

    private void Awake()
    {
#if UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif

#if UNITY_EDITOR
        Application.targetFrameRate = -1;
#endif
    }

    void Start()
    {
        infoborder = GameObject.Find("InfoBorder");
        infoborder.SetActive(false);
        
        soundImg = GameObject.Find("SoundButton").GetComponent<Image>();
        soundOn = true;
    }

    void Update()
    {
        GameQuit();
    }

    void GameQuit()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if(Input.GetKey(KeyCode.Escape))
                Application.Quit();
        }
    }

    public void TouchDownSpeedModeButton()
    {
        SceneManager.LoadScene("Speed");
    }

    public void TouchDownBattleModeButton()
    {
        //SceneManager.LoadScene("Battle_1");
    }

    public void TouchDownInfoButton()
    {
        if(infoborder.activeSelf == false)
            infoborder.SetActive(true);
        else
            infoborder.SetActive(false);
    }

    public void TouchDownSoundButton()
    {
        if(soundOn)
        {
            soundImg.sprite = Resources.Load<Sprite>("SoundOFF") as Sprite;
            soundOn = false;
            mainmix.SetFloat("Volume", -80);
        }
        else
        {
            soundImg.sprite = Resources.Load<Sprite>("SoundON") as Sprite;
            soundOn = true;
            mainmix.SetFloat("Volume", 0);
        }
    }
}
