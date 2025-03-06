using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollToBottom : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    void Start()
    {
        // ��������� ����� ���� �� �����
        ScrollToBottomInstant();
    }

    private void ScrollToBottomInstant()
    {
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases(); // ��������� UI ����� ���������       
    }
}
