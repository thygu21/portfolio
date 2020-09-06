using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;

    protected int state;
    protected int order;

    //status
    public int health;
    protected int damage;
    protected float speed;

    void OnEnable()
    {
        animator.SetInteger("Action", state);
    }

    void Update()
    {
        //자기 자신이 가장 앞에 있는 enemy일 경우 battlePos로 이동
        if (order == 0)
            transform.position = Vector3.MoveTowards(transform.position, GameManager.battlePos, speed * Time.deltaTime);
        //그렇지 않을 경우 자기 앞에 있는 enemy의 위치에 x좌표 +2한 위치로 이동
        else
            transform.position = Vector3.MoveTowards(transform.position, Battle.EnemyList[order - 1].gameObject.transform.position + new Vector3(2f, 0, 0), speed * Time.deltaTime);
    }

    public void HideEnemy()
    {
        transform.SetParent(PoolingEnemy.poolingEnemy.transform);
        gameObject.SetActive(false);
    }

    public void EmergeEnemy()
    {
        transform.SetParent(null);
        gameObject.SetActive(true);
    }

    public void SetState(int state)
    {
        this.state = state;
    }

    public void setOrder(int order)
    {
        this.order = order;
    }

    public void SetStatus(int hp, int dmg, float speed)
    {
        health = hp;
        damage = dmg;
        this.speed = speed;
    }

    public void onDamage(int dmg)
    {
        health -= dmg;
        if(health <= 0)
            Die();
    }

    void Die()
    {
        SoundManager.EnemySound();
        SetState(0);
    }
}
