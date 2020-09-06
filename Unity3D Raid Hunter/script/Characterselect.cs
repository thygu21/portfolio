using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Characterselect : MonoBehaviour
{
    // Camera
    public Camera cam;

    // Selected Option
    public GameObject targetCharacter = null;

    // Character Select
    public GameObject[] Player;
    public Material[] playerMat; // 0: kngiht, 1: wizard, 2: warrior, 3: archer
    public Material outline;
    public Material outlineWarrior;

    private Material[] outlined;
    private Material none;

    private Button button_Character;
    private GameObject setNickname;

    // Loading
    private GameObject loading;

    private void Start()
    {
        button_Character = GameObject.Find("UI/캐릭터선택 버튼").GetComponent<Button>();
        setNickname = GameObject.Find("UI/NickName 설정");
        button_Character.onClick.AddListener(SelectDone);

        loading = GameObject.Find("Loading");
        loading.SetActive(false);

        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // 캐릭터 선택 유무에 따른 활성화
        if (targetCharacter == null)
            button_Character.interactable = false;
        else
            button_Character.interactable = true;

        if (Input.GetMouseButton(0))  // 마우스가 클릭 되면
        {
            Vector3 mos = Input.mousePosition;
            mos.z = cam.farClipPlane; // 카메라가 보는 방향과, 시야를 가져온다.

            Vector3 dir = cam.ScreenToWorldPoint(mos);
            // 월드의 좌표를 클릭했을 때 화면에 자신이 보고있는 화면에 맞춰 좌표를 바꿔준다.

            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, mos.z))
            {
                GameObject hitTarget = hit.collider.gameObject;
                if(hitTarget.tag == "Player")
                {
                    DrawOutline(hitTarget);
                }
            }
        }

    }

    private void DrawOutline(GameObject hitTarget)
    {
        if(targetCharacter != null) // if selected character before
        {
            switch (targetCharacter.name)
            {
                case "knight":
                    outlined = new Material[] { playerMat[0], none };
                    break;
                case "wizard":
                    outlined = new Material[] { playerMat[1], none };
                    break;
                case "Warrior":
                    outlined = new Material[] { playerMat[2], none };
                    break;
                case "archer":
                    outlined = new Material[] { playerMat[3], none };
                    break;
            }
            targetCharacter.GetComponentInChildren<SkinnedMeshRenderer>().materials = outlined;
        }

        switch (hitTarget.name)
        {
            case "knight":
                outlined = new Material[] { playerMat[0], outline };
                break;
            case "wizard":
                outlined = new Material[] { playerMat[1], outline };
                break;
            case "Warrior":
                outlined = new Material[] { playerMat[2], outlineWarrior };
                break;
            case "archer":
                outlined = new Material[] { playerMat[3], outline };
                break;
        }
        hitTarget.GetComponentInChildren<SkinnedMeshRenderer>().materials = outlined;
        targetCharacter = hitTarget; // save selected character
    }

    private void SelectDone()
    {
        // 캐릭터 선택
        if (targetCharacter != null)
        {
            PlayerPref.selected = targetCharacter.name;
            PlayerPref.nickname = setNickname.GetComponentInChildren<Text>().text;
            StartCoroutine(LoadScene());
        }
    }

    IEnumerator LoadScene()
    {
        loading.SetActive(true);
        yield return null;
        AsyncOperation asyncOper = SceneManager.LoadSceneAsync("Map");
        //asyncOper.allowSceneActivation = false;

        while (!asyncOper.isDone)
        {
            yield return null;
        }
        //asyncOper.allowSceneActivation = true;
    }


}
