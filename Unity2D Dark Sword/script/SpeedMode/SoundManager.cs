using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager sm;

    //BGM source
    public AudioSource BGM;

    //Player sound
    public AudioSource playerMusicPlayer;
    public AudioClip slashSound;
    public AudioClip defenseSound;
    public AudioClip pierceSound;

    //Enemy sound
    public AudioSource enemyMusicPlayer;
    public AudioClip EnemyHittedSound;

    //effect sound
    public AudioSource effectSoundPlayer;
    public AudioClip gameOverSound;
    public AudioClip BestScoreUpdateSound;

    void Start()
    {
        sm = this;
    }

    //Player sound
    public static void PlayerSound(string name)
    {
        sm.playerMusicPlayer.Stop();
        sm.playerMusicPlayer.clip = sm.selectSound(name);
        sm.playerMusicPlayer.loop = false;
        sm.playerMusicPlayer.time = 0;
        sm.playerMusicPlayer.Play();
    }

    private AudioClip selectSound(string name)
    {
        if(name == "slash") 
            return slashSound;
        else if(name == "defense")
            return defenseSound;
        else if(name == "pierce")
            return pierceSound;
        else
            return null;
    }

    //Enemy Sound
    public static void EnemySound()
    {
        sm.enemyMusicPlayer.Stop();
        sm.enemyMusicPlayer.clip = sm.EnemyHittedSound;
        sm.enemyMusicPlayer.loop = false;
        sm.enemyMusicPlayer.time = 0;
        sm.enemyMusicPlayer.Play();
    }
    
    //Dead sound
    public static void PlayGameOverSound()
    {
        BGMStop();
        sm.effectSoundPlayer.Stop();
        sm.effectSoundPlayer.clip = sm.gameOverSound;
        sm.effectSoundPlayer.loop = false;
        sm.effectSoundPlayer.time = 0;
        sm.effectSoundPlayer.Play();
    }

    //best Score sound
    public static void PlayBestScoreUpdate()
    {
        sm.effectSoundPlayer.Stop();
        sm.effectSoundPlayer.clip = sm.BestScoreUpdateSound;
        sm.effectSoundPlayer.loop = false;
        sm.effectSoundPlayer.time = 0;
        sm.effectSoundPlayer.Play();
    }

    //BGM sound
    public static void BGMStop()
    {
        sm.BGM.Stop();
    }

    public static void BGMStart()
    {
        sm.BGM.Play();
    }
}
