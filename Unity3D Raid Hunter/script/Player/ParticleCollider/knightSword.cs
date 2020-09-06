using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class knightSword : MonoBehaviour
{
    GameObject knight;

    void Awake()
    {
        knight = GetComponentInParent<Knight>().gameObject;
    }
    //trigger
    private void OnTriggerEnter(Collider other)
    {
        knight.GetComponent<Knight>().AttackTrigger(other);
    }
}
