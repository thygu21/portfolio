using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    public string gameVersion = "2.0";
    public string nickName = "KwangWoon";      // 닉네임 설정

    private bool joined = false;

    public Material[] playerMat; // 0: kngiht, 1: wizard, 2: warrior, 3: archer
    public Material outline;
    public Material outlineWarrior;
    private Material[] outlined;

    public Material jangpan;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        nickName = PlayerPref.nickname;
        OnLogin();
    }

    void OnLogin()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = nickName;
        PhotonNetwork.ConnectUsingSettings();   // 위의 설정 이후 OnConnectedToMaster 실행
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
        //CreateRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }
    public override void OnJoinedRoom()
    {
        StartCoroutine(CreatePlayer());
        joined = true;
    }
    private void CreateRoom()
    {
        // CreateRoom(방이름, 방옵션)
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5 });
    }


    private void CreateMonster()
    {
        GameObject monster = PhotonNetwork.Instantiate("Cyc", new Vector3(31, 12.5f, 0), Quaternion.identity, 0);
        monster.GetComponent<BossAI>().slider = gameObject.GetComponent<Canvas>().GetComponentInChildren<Slider>();
    }
    public IEnumerator CreatePlayer()
    {
        GameObject player = PhotonNetwork.Instantiate("03.prefabs/" + PlayerPref.selected, new Vector3(-31, 16, -1f + 1.5f * (PhotonNetwork.CountOfPlayers)), Quaternion.Euler(new Vector3(0, 90, 0)), 0); // Instantiate(프리팹, 위치, 회전, 0)
        GameManager.mainCamera.GetComponent<MainCameraAction>().player = player;
        player.GetComponent<PlayerCharacter>().slider = GameObject.Find("HpBar").GetComponent<Slider>();
        player.GetComponent<PlayerCharacter>().slider.maxValue = player.GetComponent<PlayerCharacter>().InitHealth;       //슬라이더를 직업마다 최대값 조정
        player.GetComponent<PlayerCharacter>().slider.value = player.GetComponent<PlayerCharacter>().health;
        player.GetComponent<PlayerCharacter>().nickName = PlayerPref.nickname;

        GameObject.Find("PlayerParty").GetComponent<MeshRenderer>().material = jangpan;

        yield return null;

        GameObject loading = GameObject.Find("Loading");
        if(loading != null)
            Destroy(loading);
    }
    public void OnClickContinew()
    {
        StartCoroutine(CreatePlayer());
        GameManager.gameManager.continewMessage.SetActive(false);
        GameManager.gameManager.continewButton.SetActive(false);
    }
}

