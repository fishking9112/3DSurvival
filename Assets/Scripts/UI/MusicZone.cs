using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZone : MonoBehaviour
{

    public AudioSource audioSource;
    public float fadeTime;
    public float maxVolume;
    private float targetVolume;
    // Start is called before the first frame update
    void Start()
    {
        targetVolume = 0;
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = targetVolume;
        audioSource.Play();

    }

    // Update is called once per frame
    void Update()
    {
        //Approximately 소숫점 연산을 할 때 , 근사값 단위의 연산을 한다
        // 현재 코드에서는 근사값이 아닐때 임
        if ( !Mathf.Approximately(audioSource.volume , targetVolume))
        {
            //MoveTowards 점진적으로 커진다
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, (maxVolume / fadeTime) * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            targetVolume = maxVolume;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetVolume = 0;
        }
    }
}
