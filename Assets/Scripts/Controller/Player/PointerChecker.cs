using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class PointerChecker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public Action OnPointerDownCallback = delegate { };
    public Action OnPointerUpCallback = delegate { };

    private Button Btn_;
    private Button Btn {
        get {
            if(Btn_ == null ) {
                Btn_ = GetComponent<Button>();
            }
            return Btn_;
        }
    }

    public void OnPointerDown( PointerEventData eventData ) {
        OnPointerDownCallback();
    }

    public void OnPointerUp( PointerEventData eventData ) {
        OnPointerUpCallback();
    }

    public void SetActive(bool active ) {
        Btn.interactable = active;
    }
}
