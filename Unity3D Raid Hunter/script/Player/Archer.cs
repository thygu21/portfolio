using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;

public class Archer : PlayerCharacter
{
    // 플레이어 설정
    public float damage = 10f;
    public float skillDamage = 20f;
    public float attackRange = 20f;
    // 활 및 화살
    public GameObject arrow; // Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 Spine1/Bip01 Spine2/Bip01 R Clavicle/Bip01 R UpperArm/Bip01 R Forearm/Bip01 R Hand/arrow_00
    public GameObject shot_Arrow;
    public GameObject archer_Skill;
    public GameObject bowpoint;
    private int skillBehavior = 0;


    float fireDistance = 60f;


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
        skillBehavior = 1;
        if (Vector3.Distance(transform.position, GameManager.monster.transform.position) < attackRange)
        {
            transform.LookAt(GameManager.monster.transform);
            arrow.SetActive(true);
        }
    }
    // 스킬
    [PunRPC]
    override protected void StartSkill()
    {
        base.StartSkill();
        skillBehavior = 2;

        arrow.SetActive(true);
        StartCoroutine("SkillShot");    
    }
    protected IEnumerator SkillShot()
    { 
        if (Vector3.Distance(transform.position, GameManager.monster.transform.position) < attackRange) //enable only boss in range
        {
            transform.LookAt(GameManager.monster.transform);
            Vector3 bowPosition = bowpoint.transform.position;
            Quaternion newRotation = Quaternion.LookRotation(GameManager.monster.transform.position + new Vector3(0, 1.3f, 0) - bowPosition);

            yield return new WaitForSeconds(0.5f);
            GameObject effect = Instantiate(archer_Skill, bowPosition + new Vector3(0, 0.3f, 0), newRotation);
            yield return new WaitForSeconds(1.5f);
            Destroy(effect);
        }
        yield return new WaitForSeconds(3f);
        EndSkill();
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

    // 화살 발사
    [PunRPC]
    protected void ShotArrow()
    {
        arrow.SetActive(false); // 화살이 날라감을 표현
        RaycastHit hit;
        // 레이캐스트(시작지점, 방향, 충돌 정보 컨테이너, 사정거리)
        if (Physics.Raycast(arrow.transform.position, GameManager.monster.transform.position - gameObject.transform.position, out hit, fireDistance)
        && Vector3.Distance(transform.position, GameManager.monster.transform.position) < attackRange)
        { 
            if (photonView.IsMine)
            {
                PhotonView pv = GameManager.monster.GetPhotonView();
                if (skillBehavior == 1)
                    pv.RPC("OnDamage", RpcTarget.MasterClient, damage);
                else if (skillBehavior == 2)
                    pv.RPC("OnDamage", RpcTarget.MasterClient, skillDamage);
            }
            // 발사 이펙트 재생 시작
            ShotEffect(hit.point);
        }
    }
    private void ShotEffect(Vector3 hitPosition)
    { // 발사 이펙트와 소리를 재생하고 총알 궤적을 그린다
        if (animator.GetInteger("AttackBehavior") == 2)
            return;

        float trans = 30f;
        GameObject effect = Instantiate(shot_Arrow, transform.position + transform.up * 1.2f + transform.forward * 0.3f, Quaternion.Euler(new Vector3(0, transform.eulerAngles.y + 90, 0)));
        Rigidbody rb = effect.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * trans, ForceMode.Impulse);

        Destroy(effect, 3f);
    }
}
