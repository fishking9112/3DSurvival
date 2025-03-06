using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController controller;
    public PlayerCondition condition;

    public ItemData itemData;   // 현재 보이는 아이템 데이터
    public Action addItem;      // itemData를 

    public Transform dropPosition;

    private void Awake()
    {
        CharacterManager.Instance.Player = this;

        controller = GetComponent<PlayerController>(); ;

        condition = GetComponent<PlayerCondition>();
    }
}
