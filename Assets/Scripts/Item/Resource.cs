using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//필드에 있는 자원 sc
public class Resource : MonoBehaviour
{
    public ItemData itemToGive;     // 어떤 아이템을 줄것인지
    public int quantityPerHit = 1;  // 한번 공격에 아이템 몇개를 줄것인지 정하는 변수
    public int capacy;              // 몇개까지 아이템을 줄것인지

    public void Gather(Vector3 hitPoint , Vector3 hitNormal)
    {
        for(int i = 0; i < quantityPerHit; i++)
        {
            if (capacy <= 0) break;

            capacy -= 1;

            Instantiate(itemToGive.dropPrefab, hitPoint + Vector3.up, Quaternion.LookRotation(hitNormal, Vector3.up));
        }
    }
    
}
