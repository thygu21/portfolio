using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class outsideTrigger : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;
        other.GetComponent<Rigidbody>().MovePosition(new Vector3(-31, 16, -1));
    }
}
