using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f; // 검사할 주기
    private float lastCheckTime;    // 마지막 검사한 시간
    public float maxCheckDistance;  // 검사 길이

    public LayerMask layerMask;     // 어떤 레이어에서 검사할지

    public GameObject curInteractGameObject;    // 현재 인터렉트 된 게임오브젝트
    private IInteractable curInteractable;      // 의 인터페이스 

    public TextMeshProUGUI promptText;  // 프롬포트에 띄워질 텍스트 , 나중에 분리 가능 ??
    private Camera camera;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        //일정 시간마다 Ray 쏘기
        if(Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            //카메라 중점에서 레이 발사 해서 충돌 했으면 ?
            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                if (hit.collider.gameObject != curInteractGameObject)
                {
                    // 정보 가져오기
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();

                    //출력
                    SetPromptText();
                }
            }
            //아니면
            else
            {
                // 데이터 초기화
                curInteractGameObject = null;
                curInteractable = null;
                // 프롬포트 끄기
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

            // 데이터 초기화
            curInteractGameObject = null;
            curInteractable = null;
            // 프롬포트 끄기
            promptText.gameObject.SetActive(false);
        }
    }
}
