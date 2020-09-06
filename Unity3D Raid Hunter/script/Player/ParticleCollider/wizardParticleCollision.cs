using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class wizardParticleCollision : MonoBehaviour
{
    private Wizard wizard;

    void OnParticleCollision(GameObject other)
    {
        wizard = GetComponentInParent<Wizard>();
        PhotonView pv = other.GetPhotonView();
        pv.RPC("OnDamage", RpcTarget.MasterClient, wizard.skillDamage);
    }
}
