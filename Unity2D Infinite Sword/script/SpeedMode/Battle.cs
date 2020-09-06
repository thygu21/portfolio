using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour
{
    public static Battle battle;
    public static List<Enemy> EnemyList = new List<Enemy>();

    public static float maxGap = 1f;

    void Awake()
    {
        battle = this;
    }

    public static void CreateEnemy()
    {
        Enemy enemy = PoolingEnemy.GetEnemy();
        EnemyList.Add(enemy);
        enemy.setOrder(EnemyList.Count - 1);
        enemy.EmergeEnemy();
    }

    //모든 몬스터의 순서를 재설정함
    void setEnemyOrder()
    {
        for (int i = 0; i < EnemyList.Count; i++)
            EnemyList[i].setOrder(i);
    }

    public static void EnemyDamaged(int dmg)
    {
        EnemyList[0].onDamage(dmg);

        if(EnemyList[0].health <= 0)
            battle.KillEnemy();
        else
            DisarmEnemy();
    }

    void KillEnemy()
    {
        PoolingEnemy.ReturnObject(EnemyList[0]);
        battle.RemoveEnemyList();
        CreateEnemy();
    }

    public static void DisarmEnemy()
    {
        EnemyList[0].animator.SetInteger("Action", 1);
    }

    void RemoveEnemyList()
    {
        EnemyList.RemoveAt(0);
        battle.setEnemyOrder();
    }

    public static bool IsEnemyInRange()
    {
        if (EnemyList.Count > 0)
            if (EnemyList[0].gameObject.transform.position.x < GameManager.battlePos.x + maxGap)
                return true;

        return false;
    }

    public static int getEnemyAction()
    {
        return EnemyList[0].animator.GetInteger("Action");
    }

    public static void ClearEnemy()
    {
        for (int i = 0; i < EnemyList.Count; i++)
            PoolingEnemy.ReturnObject(EnemyList[i]);
        EnemyList.Clear();
    }
}
