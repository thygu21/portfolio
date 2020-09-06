using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ShieldEffect : MonoBehaviour
{
    private bool touched = true;
    private void OnTriggerEnter(Collider other)
    {
        if (touched && other.GetComponentInParent<BossAI>().gameObject == GameManager.monster)
        {
            touched = false;
            PhotonView pv = GameManager.monster.GetPhotonView();
            pv.RPC("OnStiffness", RpcTarget.All, 0.5);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        touched = true;
    }
}
