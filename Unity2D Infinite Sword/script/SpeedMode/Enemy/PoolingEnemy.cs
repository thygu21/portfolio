using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

public class PoolingEnemy : MonoBehaviour
{
    public static PoolingEnemy poolingEnemy;

    //object polling
    [SerializeField]
    private GreenGoblinSpear GreenGoblinSpear;
    private Queue<GreenGoblinSpear> GGSpearQueue = new Queue<GreenGoblinSpear>();

    [SerializeField]
    private RedGoblinFire RedGoblinFire;
    private Queue<RedGoblinFire> RGFireQueue = new Queue<RedGoblinFire>();

    public GameObject arrow;

    //enemy create
    void Start()
    {
        poolingEnemy = this;
        InitEnemy();
    }

    //큐에 몬스터 오브젝트를 각각 8개씩 넣음
    private void InitEnemy()
    {
        for(int i = 0; i < 8; i++)
        {
            GGSpearQueue.Enqueue(CreateGGSpear());
            RGFireQueue.Enqueue(CreateRGFire());
        }
    }

    private GreenGoblinSpear CreateGGSpear()
    {
        var enemy = Instantiate(GreenGoblinSpear, new Vector3(0, 0, 0), Quaternion.identity);
        enemy.HideEnemy();

        return enemy;
    }

    private RedGoblinFire CreateRGFire()
    {
        var enemy = Instantiate(RedGoblinFire, new Vector3(0, 0, 0), Quaternion.identity);
        enemy.HideEnemy();

        return enemy;
    }

    //enemy info send GameManager.cs
    //if queue is full, create more enemy
    public static Enemy GetEnemy()
    {
        Enemy enemy = poolingEnemy.RandomEnemy();
        poolingEnemy.DrawBestScoreArrow(enemy);
        poolingEnemy.SetEnemyStartPosition(enemy);

        return enemy;
    }
   
    void SetEnemyStartPosition(Enemy enemy)
    {
        enemy.transform.position = GameManager.createPos;
    }

    private Enemy RandomEnemy()
    {
        Enemy enemy;

        if (Random.Range(0f, 1.0f) > GameManager.RedGoblinRate)
        {
            //초록 고블린
            if (poolingEnemy.GGSpearQueue.Count > 0)
                enemy = poolingEnemy.GGSpearQueue.Dequeue();
            else
                enemy = poolingEnemy.CreateGGSpear();

            if (Random.Range(0f, 1.0f) > GameManager.AttackGoblinRate)
            {
                enemy.SetStatus(1, 1, 10f);
                enemy.SetState(1);
                GameManager.expectScore += 1;
            }
            else
            {
                enemy.SetStatus(2, 1, 10f);
                enemy.SetState(2);
                GameManager.expectScore += 2;
            }
        }
        else
        {
            //빨간 고블린
            if (poolingEnemy.RGFireQueue.Count > 0)
                enemy = poolingEnemy.RGFireQueue.Dequeue();
            else
                enemy = poolingEnemy.CreateRGFire();

            enemy.SetStatus(1, 1, 10f);
            enemy.SetState(3);
            GameManager.expectScore += 1;
        }

        return enemy;
    }
 
    void DrawBestScoreArrow(Enemy enemy)
    {
        if(!GameManager.isArrowDrawed && GameManager.expectScore > GameManager.bestScore)
        {
            Vector3 UIposition = enemy.gameObject.transform.position + new Vector3(0, 2, 0);
            var bestScoreUI = Instantiate(arrow, UIposition, Quaternion.Euler(0, 0, 180));
            bestScoreUI.transform.SetParent(enemy.gameObject.transform);

            GameManager.isArrowDrawed = true;
        }
    }

    //Enqueue enemy for reuse
    public static void ReturnObject(Enemy obj)
    {
        poolingEnemy.EraseBestScoreArrow(obj);
        obj.HideEnemy();

        if (obj is GreenGoblinSpear)
        {
            poolingEnemy.GGSpearQueue.Enqueue((GreenGoblinSpear)obj);
        }
        else if(obj is RedGoblinFire)
        {
            poolingEnemy.RGFireQueue.Enqueue((RedGoblinFire)obj);
        }
    }

    void EraseBestScoreArrow(Enemy obj)
    {
        if(obj.transform.Find("G_arrow(Clone)") != null)
        {
            Destroy(obj.transform.Find("G_arrow(Clone)").gameObject);
        }
    }
}
