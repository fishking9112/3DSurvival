using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFire : MonoBehaviour
{
    public int damage;
    public float damageRate;

    List<IDamagalbe> things = new List<IDamagalbe>();


    void Start()
    {
        InvokeRepeating("DealDamage" , 0 , damageRate);
    }

    void DealDamage()
    {
        for(int i = 0; i < things.Count; i++)
        {
            things[i].TakePhysicalDamage(damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out IDamagalbe damagalbe))
        {
            things.Add(damagalbe);

            //트리거에 닿으면 리스트에 저장해 두었다가,
            //DealDamage 함수에서 리스트 안에 있는 객체들에게 데미지를 주는 방식
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out IDamagalbe damagalbe))
        {
            things.Remove(damagalbe);
        }
    }
}
