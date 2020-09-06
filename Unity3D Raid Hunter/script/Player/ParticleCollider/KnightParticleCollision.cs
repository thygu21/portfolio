using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class KnightParticleCollision : MonoBehaviour
{
    private Knight knight;
    
    void OnParticleCollision(GameObject other)
    {
        knight = GetComponentInParent<Knight>();
        if (other.GetComponentInParent<BossAI>().gameObject == GameManager.monster)
        {
            PhotonView pv = GameManager.monster.GetPhotonView();
            pv.RPC("OnDamage", RpcTarget.MasterClient, knight.skillDamage);
        }
    }
}
