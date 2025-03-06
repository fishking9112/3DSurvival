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
        //Approximately �Ҽ��� ������ �� �� , �ٻ簪 ������ ������ �Ѵ�
        // ���� �ڵ忡���� �ٻ簪�� �ƴҶ� ��
        if ( !Mathf.Approximately(audioSource.volume , targetVolume))
        {
            //MoveTowards ���������� Ŀ����
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
