using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Wizard : PlayerCharacter
{
    // 플레이어 설정
    public float damage = 10f;
    public float skillDamage = 20f;
    private float spellRange = 10f;
    public GameObject HealPrefeb;
    public GameObject TargetHealPrefeb;
    public GameObject staffpoint;


    // 공격
    [PunRPC]
    override protected void StartAttack()
    {
        base.StartAttack();
        StartCoroutine("CreateAttackParticle");
    }
    private IEnumerator CreateAttackParticle()
    {
        // 공격 범위 안에 레이드 몬스터가 있다면 공격
        if (Vector3.Distance(transform.position, GameManager.monster.transform.position) < spellRange)
        {
            transform.LookAt(GameManager.monster.transform);
            yield return new WaitForSeconds(0.5f);

            Vector3 staffPosition = staffpoint.transform.position - new Vector3(0, 1, 0);
            Quaternion newRotation = Quaternion.LookRotation(GameManager.monster.transform.position - staffPosition);

            var effect = Instantiate(AttackPrefab, staffPosition, newRotation);
            Destroy(effect, 1.2f);
            if (photonView.IsMine)
            {
                PhotonView pv = GameManager.monster.GetPhotonView();
                pv.RPC("OnDamage", RpcTarget.MasterClient, damage);
            }
        }
    }
    // 스킬
    [PunRPC]
    override protected void StartSkill()
    {
        base.StartSkill();
        StartCoroutine("CreateSkillParticle");
    }
    private IEnumerator CreateSkillParticle()
    {
        // 공격 범위 안에 레이드 몬스터가 있다면 공격
        if (Vector3.Distance(transform.position, GameManager.monster.transform.position) < spellRange)
        {
            Vector3 pos = GameManager.monster.transform.position;
            yield return new WaitForSeconds(0.5f);

            if (photonView.IsMine)
            {
                PhotonView pv = GameManager.monster.GetComponentInParent<PhotonView>();
                pv.RPC("OnDamage", RpcTarget.MasterClient, skillDamage);
            }

            var effect = Instantiate(SkillPrefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(5f);
            Destroy(effect, 5f);
        }

        yield return new WaitForSeconds(4.5f); //10초
        EndSkill();
    }
    // 힐
    [PunRPC]
    override protected void StartHeal()
    {
        base.StartHeal();
        
        GameObject target = GameManager.players[0];
        float temp = 200;
        foreach (GameObject player in GameManager.players)
        {
            float player_health = player.GetComponent<PlayerCharacter>().health;
            if(player_health < temp)
            {
                temp = player_health;
                target = player;
            }
        }
        Heal(target, 50f);
        StartCoroutine(CreateHealParticle(target));
    }
    public void Heal(GameObject player, float value)
    {
        if (photonView.IsMine)
        {
            PhotonView pv = PhotonView.Get(player);
            pv.RPC("RestoreHealth", RpcTarget.All, value);
        }
    }
    protected IEnumerator CreateHealParticle(GameObject target)
    {
        var targetEffect = Instantiate(TargetHealPrefeb, target.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
        var effect = Instantiate(HealPrefeb, transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)));
        yield return new WaitForSeconds(2f);

        Destroy(targetEffect);
        Destroy(effect);

        yield return new WaitForSeconds(8f); //10초
        EndHeal();
    }

    // 회피
    [PunRPC]
    protected override void StartAvoid()
    {
        base.StartAvoid();
        StartCoroutine("CreateAvoidParticle");
    }
    private IEnumerator CreateAvoidParticle()
    {
        rigidbody.MovePosition(transform.position + transform.forward * 5);
        yield return new WaitForSeconds(0.05f);

        var effect = Instantiate(AvoidPrefab, transform.position + transform.up * 0.5f, Quaternion.identity);
        yield return new WaitForSeconds(0.7f);
        Destroy(effect);
        yield return new WaitForSeconds(2.25f); // 3초
        EndAvoid();
    }
    public void AvoidAnimationEnd()
    {
        animator.SetBool("isAvoid", false);
    }

}
