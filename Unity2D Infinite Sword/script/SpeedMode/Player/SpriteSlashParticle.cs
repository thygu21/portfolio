using UnityEngine;
using System.Collections;

public class SpriteSlashParticle : MonoBehaviour
{
    public float fps = 30.0f;
    public Texture[] frames;

    private MeshRenderer rendererMy;

    void Start()
    {
        rendererMy = GetComponent<MeshRenderer>();
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        StartCoroutine("NextFrame");
    }

    IEnumerator NextFrame()
    {
        for(int i = 0; i < frames.Length; i++)
        {
            //여기 왜 오류나는지 모르겠음 ㅠ
            rendererMy.sharedMaterial.SetTexture("_MainTex", frames[i]);
            yield return new WaitForSeconds(1 / fps);
        }
        //여운?주기
        yield return new WaitForSeconds(3 / fps);
        gameObject.SetActive(false);
    }
}