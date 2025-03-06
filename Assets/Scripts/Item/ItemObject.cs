using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Item을 상호작용 하기 위한 스크립트
// 아이템 종류에 맞는 클래스에 대응할 수 있게 만들자.

public interface IInteractable
{
    public string GetInteractPrompt();
    public void OnInterract();
}

public class ItemObject : MonoBehaviour , IInteractable
{
    public ItemData data;

    public string GetInteractPrompt()
    {
        string str = $"{data.displayName}\n{data.description}";

        return str;
    }

    public void OnInterract()
    {
        CharacterManager.Instance.Player.itemData = data;   // 데이터 넘겨주고
        CharacterManager.Instance.Player.addItem?.Invoke(); // 델리게이트를 통해 구독한 함수(addItem) 실행
        Destroy(gameObject);
    }
}
