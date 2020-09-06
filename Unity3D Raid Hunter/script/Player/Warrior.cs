using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Warrior : PlayerCharacter
{
    // 플레이어 설정
    public float damage = 10f;
    public float skillDamage = 20f;
    private bool IsParry = false;
    private bool IsDefend = false;



    // 움직임 ( 워리어의 애니메이션의 이상으로 Rotation을 수정 ) 
    override protected void Turn()
    {
        Quaternion newRotation = Quaternion.LookRotation(movement);
        rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, newRotation * Quaternion.AngleAxis(17, Vector3.up), rotateSpeed * Time.deltaTime * 5);
    }

    // 공격
    [PunRPC]
    override protected void StartAttack()
    {
        base.StartAttack();
        StartCoroutine("CreateAttackParticle");
    }
    private IEnumerator CreateAttackParticle()
    {
        yield return new WaitForSeconds(0.1f);
        GameObject effect = Instantiate(AttackPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(-90, transform.eulerAngles.y - 90, 0)));
        Destroy(effect, 0.5f);
        yield return new WaitForSeconds(0.5f);
        //공격후 다시 정면 보기
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), rotateSpeed * Time.deltaTime * 5);
    }
    // 스킬
    [PunRPC]
    override protected void StartSkill()
    {
        base.StartSkill();
        StartCoroutine("CreateSkillParticle");
        Invoke("EndSkill", 1.25f);
    }
    private IEnumerator CreateSkillParticle()
    {
        yield return new WaitForSeconds(0.05f);
        IsParry = true;

        yield return new WaitForSeconds(0.2f);
        IsParry = false;
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
        IsDefend = true;
        GameObject effect = Instantiate(AvoidPrefab, transform.position + transform.up * 0.5f, Quaternion.identity);
        yield return new WaitForSeconds(0.5f);
        animator.speed = 0.0f;

        while (IsDefend) //button 땔 때 까지 반복
            yield return null;
        animator.speed = 1f;
        Destroy(effect);
        animator.SetBool("isAvoid", false);
        EndAvoid();
    }
    [PunRPC]
    public void PointUP()
    {
        if(photonView.IsMine)
            pv.RPC("PointUP", RpcTarget.Others);
        // 회피 버튼에서 땔 때
        IsDefend = false;
    }
    // 공격 이벤트
    public void AttackTrigger(Collider other)
    {
        GameObject temp = other.GetComponentInParent<BossAI>().gameObject;
        if (temp == GameManager.monster && attackCoolTime && photonView.IsMine)
        {
            PhotonView pv = GameManager.monster.GetPhotonView();
            pv.RPC("OnDamage", RpcTarget.MasterClient, damage);
            attackCoolTime = false;
        }
    }

    // 공격 적용 ( 패링 관련 추가 )
    [PunRPC]
    public override void OnDamage(float damage)
    {
        // 평소 상태
        if (!IsParry)
        {
            if (IsDefend)
                base.OnDamage(damage * 0.3f);
            else
                base.OnDamage(damage);
        } 
        else
        {
            // 레이드 몬스터를 행동 불능 상태로 만듬
            PhotonView monsterPv = GameManager.monster.GetPhotonView();
            monsterPv.RPC("OnStiffness", RpcTarget.All, 2);
            // 패링 성공 이펙트
            pv.RPC("WarriorParry", RpcTarget.All);
            IsParry = false;
        }
    }
    [PunRPC]
    private void WarriorParry()
    {
        GameObject effect = Instantiate(SkillPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, 0)));
        Destroy(effect, 1f);
    }
}
