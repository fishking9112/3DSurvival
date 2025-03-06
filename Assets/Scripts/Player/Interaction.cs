using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f; // �˻��� �ֱ�
    private float lastCheckTime;    // ������ �˻��� �ð�
    public float maxCheckDistance;  // �˻� ����

    public LayerMask layerMask;     // � ���̾�� �˻�����

    public GameObject curInteractGameObject;    // ���� ���ͷ�Ʈ �� ���ӿ�����Ʈ
    private IInteractable curInteractable;      // �� �������̽� 

    public TextMeshProUGUI promptText;  // ������Ʈ�� ����� �ؽ�Ʈ , ���߿� �и� ���� ??
    private Camera camera;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        //���� �ð����� Ray ���
        if(Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            //ī�޶� �������� ���� �߻� �ؼ� �浹 ������ ?
            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                if (hit.collider.gameObject != curInteractGameObject)
                {
                    // ���� ��������
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();

                    //���
                    SetPromptText();
                }
            }
            //�ƴϸ�
            else
            {
                // ������ �ʱ�ȭ
                curInteractGameObject = null;
                curInteractable = null;
                // ������Ʈ ����
                promptText.gameObject.SetActive(false);
            }
        }


    }

    private void SetPromptText()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = curInteractable.GetInteractPrompt();
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started && curInteractable != null)
        {
            curInteractable.OnInterract();

            // ������ �ʱ�ȭ
            curInteractGameObject = null;
            curInteractable = null;
            // ������Ʈ ����
            promptText.gameObject.SetActive(false);
        }
    }
}
