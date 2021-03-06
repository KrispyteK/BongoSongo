﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractionTest : MonoBehaviour {
    public Interactable interactable;
    public RawImage rawImage;

    private RectTransform rectTransform;

    void Start() {
        rectTransform = GetComponent<RectTransform>();
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        if (interactable == null) {
            Destroy(gameObject);
            return;
        }

        rectTransform.position = interactable.ScreenPosition;

        if (interactable.isInteractedWith) {
            rawImage.color = Color.green;
        } else {
            rawImage.color = Color.red;
        }

        rectTransform.sizeDelta = new Vector2(interactable.size * Camera.main.pixelWidth, interactable.size * Camera.main.pixelWidth) * 1;
    }
}
