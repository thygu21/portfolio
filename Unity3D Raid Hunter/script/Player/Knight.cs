using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Knight : PlayerCharacter
{
    // 플레이어 설정
    public float damage = 8f;
    public float skillDamage = 30f;


    override protected void FixedUpdate()
    {
        base.FixedUpdate();
        // 회피 시 일시적으로 올라간 속도로 움직임
        if (animator.GetBool("isAvoid"))
        {
            movement = transform.forward * speed * Time.deltaTime;
            rigidbody.MovePosition(transform.position + movement);
        }
    }
    // 공격
    [PunRPC]
    override protected void StartAttack()
    {
        base.StartAttack();
        CreateAttackParticle();
    }
    private void CreateAttackParticle()
    {
        GameObject effect = Instantiate(AttackPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(130, transform.eulerAngles.y - 90, 0)));
        Destroy(effect, 1f);
    }
    // 스킬
    [PunRPC]
    override protected void StartSkill()
    {
        base.StartSkill();
        CreateSkillParticle();
        Invoke("EndSkill", 10f);
    }
    private void CreateSkillParticle()
    {
        var effect = Instantiate(SkillPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0)));
        effect.transform.parent = gameObject.transform;
        Destroy(effect, 3f);
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
        speed = 40f;
        yield return new WaitForSeconds(0.1f);

        speed = 0f;
        var effect = Instantiate(AvoidPrefab, transform.position - movement * 6, Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0)));
        Destroy(effect, 1.5f);
        yield return new WaitForSeconds(0.2f);

        speed = 5f;
        animator.SetBool("isAvoid", false);
        yield return new WaitForSeconds(2.7f);
        EndAvoid();
    }

    // 공격 이벤트
    public void AttackTrigger(Collider other)
    {
        GameObject temp = other.GetComponentInParent<BossAI>().gameObject;
        if(temp == GameManager.monster && attackCoolTime && photonView.IsMine)
        {
            PhotonView pv = GameManager.monster.GetPhotonView();
            pv.RPC("OnDamage", RpcTarget.MasterClient, damage);
            attackCoolTime = false;
        }
    }
}
