using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager pm;
    
    //Player particle
    public GameObject attackParticle;
    public GameObject pierceParticle;
    public ParticleSystem defenseParticle;
    public ParticleSystem redEnemyHit;
    public ParticleSystem enemyHit;
    public ParticleSystem brokenHeart;

    void Start()
    {
        pm = this;
        pm.playerParticleHide();
    }

    void playerParticleHide()
    {
        defenseParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        redEnemyHit.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        enemyHit.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        brokenHeart.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    //Player particle
    public static void CreateSlashParticle()
    {
        pm.attackParticle.SetActive(true);
    }

    public static void CreatePierceParticle()
    {
        pm.pierceParticle.SetActive(true);
    }

    public static void CreateDefenseParticle()
    {
        pm.defenseParticle.Play();
    }

    //Enemy Particle
    public static void CreateHitParticle()
    {
        pm.enemyHit.Play();
    }

    public static void CreateRedEnemyHitParticle()
    {
        pm.redEnemyHit.Play();
    }

    //effect Particle
    public static void CreateBrokenHeartParticle()
    {
        pm.brokenHeart.Play();
    }
}
