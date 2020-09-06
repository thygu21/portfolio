using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRock : MonoBehaviour
{
    private GameObject cyc;

    public GameObject RockCollision;

    void Awake()
    {
        cyc = GameObject.Find("Cyc");
    }
    //trigger
    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(RockCollisionf(other));
    }

    IEnumerator RockCollisionf(Collider other)
    {
        cyc.GetComponent<BossAI>().AttackTrigger(other);
        GameObject effect = Instantiate(RockCollision, transform.position + new Vector3(0, 2, 0), Quaternion.identity);
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshCollider>().enabled = false;
        cyc.GetComponent<BossAI>().rangeTime = 0; // 장판 끄기
        yield return new WaitForSeconds(0.5f);

        Destroy(effect);
        yield break;
    }
}
