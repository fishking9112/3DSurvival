using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Item�� ��ȣ�ۿ� �ϱ� ���� ��ũ��Ʈ
// ������ ������ �´� Ŭ������ ������ �� �ְ� ������.

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
        CharacterManager.Instance.Player.itemData = data;   // ������ �Ѱ��ְ�
        CharacterManager.Instance.Player.addItem?.Invoke(); // ��������Ʈ�� ���� ������ �Լ�(addItem) ����
        Destroy(gameObject);
    }
}
