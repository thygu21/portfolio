using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swordman : MonoBehaviour
{   
    public static Swordman swordman;
    public Rigidbody2D m_rigidbody;
    private CapsuleCollider2D m_CapsulleCollider;
    private Animator m_Anim;

    //viewing direction
    private bool NowViewingRight;

    //status
    public int health;
    private int damage;
    private float speed;

    private void Start()
    {
        swordman = this;

        m_CapsulleCollider  = transform.GetComponent<CapsuleCollider2D>();
        m_Anim = transform.Find("model").GetComponent<Animator>();
        m_rigidbody = transform.GetComponent<Rigidbody2D>();

        NowViewingRight = true;

        SetStatus(1, 1, 0f);
    }

    void SetStatus(int hp, int dmg, float speed)
    {
        health = hp;
        damage = dmg;
        this.speed = speed;
    }

    private void Update()
    {
        if (getPlayerState() == 4) return;

        if (Input.anyKeyDown)
        {
            if (Input.GetKey(KeyCode.Mouse0))
                return;
            SelectAnimation();
            SelectAttack();
        }
        else
            setPlayerState(0);
    }

    private void SelectAnimation()
    {
        if (Input.GetKey(KeyCode.A))
        {
            setPlayerState(1);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            setPlayerState(2);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            setPlayerState(3);
        }
    }

    //for moblie
    public void Attack()
    {
        if (getPlayerState() == 4) return;
        setPlayerState(1);
        SelectAttack();
    }

    public void Defense()
    {
        if (getPlayerState() == 4) return;
        setPlayerState(2);
        SelectAttack();
    }

    public void Pierce()
    {
        if (getPlayerState() == 4) return;
        setPlayerState(3);
        SelectAttack();
    }

    private void SelectAttack()
    {
        //적이 범위에 있을 경우
        if (Battle.IsEnemyInRange())
        {
            GameManager.isStart = true;
            //올바른 입력을 했을 경우
            if(Battle.getEnemyAction() == getPlayerState())
            {
                if(getPlayerState() == 1)
                {
                    ParticleManager.CreateHitParticle();
                }
                else if(getPlayerState() == 2)
                {
                    ParticleManager.CreateDefenseParticle();
                    SoundManager.PlayerSound("defense");
                }
                else if(getPlayerState() == 3)
                {
                    ParticleManager.CreateRedEnemyHitParticle();
                }
                Battle.EnemyDamaged(1);
                GameManager.TimeUp();
            }
            //잘못된 입력을 했을 경우
            else
            {
                GameManager.setTime(0);
                return;
            }
        }

        //공격 이펙트 및 사운드 출력
        switch (getPlayerState())
        {
            case 1:
                ParticleManager.CreateSlashParticle();
                SoundManager.PlayerSound("slash");
                break;
            case 3:
                ParticleManager.CreatePierceParticle();
                SoundManager.PlayerSound("pierce");
                break;
        }
    }

    public void onDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
            return;
    }

    public static void AnimationInit()
    {
        swordman.m_Anim.SetInteger("State", 0);
        //swordman.m_Anim.Play("Run");
    }

    private int getPlayerState()
    {
        return m_Anim.GetInteger("State");
    }

    public static void setPlayerState(int state)
    {
        swordman.m_Anim.SetInteger("State", state);
    }
}
