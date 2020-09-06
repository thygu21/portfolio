using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class warriorSword : MonoBehaviour
{
    GameObject Warrior;

    void Awake()
    {
        Warrior = GetComponentInParent<Warrior>().gameObject;
    }
    //trigger
    private void OnTriggerEnter(Collider other)
    {
        Warrior.GetComponent<Warrior>().AttackTrigger(other);
    }
}
