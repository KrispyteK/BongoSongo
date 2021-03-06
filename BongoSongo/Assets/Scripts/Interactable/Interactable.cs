﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {

    [Range(0.01f, 1)] public float size = 0.25f;
    public float threshold = 0.8f;
    public bool isActive = true;
    public bool isInteractedWith = false;
    public bool isCircular;
    public bool measureAverage = false;
    [Range(1, 25)] public int density = 10;
    public bool randomize = false;
    public int randomizePoints = 50;
    public float randomizeCooldown = 40f;

    public float interactionAmount;

    public Vector3 ScreenPosition => Camera.main.WorldToScreenPoint(transform.position);

    public event EventHandler OnInteract;

    private float circularThreshold;

    void OnValidate() {
        circularThreshold = threshold / Mathf.PI;
    }

    void Update() {
        if (!WebcamMotionCapture.instance.hasWebcam || !isActive) return;

        isInteractedWith = CheckInteraction() || IsMouseInteracted();

        if (isInteractedWith && OnInteract != null) OnInteract(this, EventArgs.Empty);
    }

    private bool IsMouseInteracted() {
        var pos = Input.mousePosition;
        var screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        var pixelSize = Camera.main.pixelWidth * size;

        var minX = screenPoint.x - pixelSize / 2;
        var minY = screenPoint.y - pixelSize / 2;
        var maxX = screenPoint.x + pixelSize / 2;
        var maxY = screenPoint.y + pixelSize / 2;

        var isInButton = (pos.x > minX && pos.x < maxX) && (pos.y > minY && pos.y < maxY);

        if (Input.GetButton("Fire1") && isInButton) {
            return true;
        }

        return false;
    }

    private float GetCircularOffset(float height, float y) {
        if (!isCircular) return 0;

        return (1 - Mathf.Pow(Mathf.Sin((y - height) / size * Mathf.PI + Mathf.PI / 2), 0.5f)) * size / 2;
    }

    private bool CheckInteraction() {
        if (!randomize) interactionAmount = 0f;
        var stepSize = 1f / density * size;
        var points = Mathf.Pow((size / stepSize), 2f);

        // Get normalized X and Y coordinates of object on the screen where the X axis goes between 0 and 1 and the Y axis between 0 and the screen's aspect ratio.
        var screenPoint = Camera.main.WorldToScreenPoint(transform.position);

        var normalizedX = screenPoint.x / Camera.main.pixelWidth;
        var normalizedY = screenPoint.y / Camera.main.pixelHeight;
        var aspect = Camera.main.aspect;

        normalizedY /= aspect;

        if (!randomize) {
            for (float y = normalizedY - size / 2 + stepSize / 2; y <= normalizedY + size / 2; y += stepSize) {
                for (float x = normalizedX - size / 2 + stepSize / 2 + GetCircularOffset(normalizedY, y); x <= normalizedX + size / 2 - GetCircularOffset(normalizedY, y); x += stepSize) {
                    // Add gray scale value at the point on the motion texture thats behind the interactable.  
                    var color = WebcamMotionCapture.instance.texture.GetPixelBilinear(1 - x, y * aspect);
                    var gray = (color.r + color.g + color.b) / 3;
                    interactionAmount += gray;

                    if (measureAverage) interactionAmount /= points;

                    if (interactionAmount > (isCircular ? circularThreshold : threshold)) return true;
                }
            }
        }
        else {
            interactionAmount = Mathf.Max(0, interactionAmount - randomizeCooldown * Time.deltaTime);

            if (!isCircular) {
                for (var i = 0; i < randomizePoints; i++) {
                    var y = UnityEngine.Random.Range(normalizedY - size / 2 + stepSize / 2, normalizedY + size / 2);
                    var x = UnityEngine.Random.Range(normalizedX - size / 2 + stepSize / 2 + GetCircularOffset(normalizedY, y), normalizedX + size / 2 - GetCircularOffset(normalizedY, y));

                    // Add gray scale value at the point on the motion texture thats behind the interactable.   
                    var color = WebcamMotionCapture.instance.texture.GetPixelBilinear(1 - x, y * aspect);
                    var gray = (color.r + color.g + color.b) / 3;
                    interactionAmount += gray;

                    if (interactionAmount > threshold) return true;
                }
            }
            else {
                for (var i = 0; i < randomizePoints; i++) {
                    var a = UnityEngine.Random.Range(0, Mathf.PI * 2);
                    var y = normalizedY + (Mathf.Sin(a) * size / 2 * Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 0.5f));
                    var x = normalizedX + (Mathf.Cos(a) * size / 2 * Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 0.5f));

                    // Add gray scale value at the point on the motion texture thats behind the interactable.   
                    var color = WebcamMotionCapture.instance.texture.GetPixelBilinear(1 - x, y * aspect);
                    var gray = (color.r + color.g + color.b) / 3;
                    interactionAmount += gray;

                    if (interactionAmount > threshold) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void OnDrawGizmos() {
        var stepSize = 1f / density * size;
        var screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        var normalizedX = screenPoint.x / Camera.main.pixelWidth;
        var normalizedY = screenPoint.y / Camera.main.pixelHeight;
        var aspect = Camera.main.aspect;
        var deltaZ = transform.position.z - Camera.main.transform.position.z;

        normalizedY /= aspect;

        if (!(randomize && isCircular)) {
            for (float y = normalizedY - size / 2; y <= normalizedY + size / 2; y += stepSize) {
                for (float x = normalizedX - size / 2 + GetCircularOffset(normalizedY, y); x <= normalizedX + size / 2 - GetCircularOffset(normalizedY, y); x += stepSize) {
                    if (isCircular) x = Mathf.Round(x / stepSize) * stepSize;

                    Gizmos.DrawSphere(
                        CameraTransform.ScreenPointToWorld(new Vector2(
                            x,
                            y
                            )), 0.05f);
                }
            }
        }
        else {
            for (var i = 0; i < randomizePoints; i++) {
                var a = UnityEngine.Random.Range(0, Mathf.PI * 2);
                var y = normalizedY + (Mathf.Sin(a) * size / 2 * Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 0.5f));
                var x = normalizedX + (Mathf.Cos(a) * size / 2 * Mathf.Pow(UnityEngine.Random.Range(0f, 1f), 0.5f));

                Gizmos.DrawSphere(
                    CameraTransform.ScreenPointToWorld(new Vector2(
                        x,
                        y
                        )), 0.05f);
            }
        }

        // Top line
        Gizmos.DrawLine(CameraTransform.ScreenPointToWorld(new Vector2(
                normalizedX - size / 2,
                normalizedY + size / 2
            )), CameraTransform.ScreenPointToWorld(new Vector2(
                normalizedX + size / 2,
                normalizedY + size / 2
            )));

        // Right line
        Gizmos.DrawLine(CameraTransform.ScreenPointToWorld(new Vector2(
                normalizedX + size / 2,
                normalizedY + size / 2
            )), CameraTransform.ScreenPointToWorld(new Vector2(
                normalizedX + size / 2,
                normalizedY - size / 2
            )));

        // Bottom line
        Gizmos.DrawLine(CameraTransform.ScreenPointToWorld(new Vector2(
                normalizedX + size / 2,
                normalizedY - size / 2
            )), CameraTransform.ScreenPointToWorld(new Vector2(
                normalizedX - size / 2,
                normalizedY - size / 2
            )));

        // Right line
        Gizmos.DrawLine(CameraTransform.ScreenPointToWorld(new Vector2(
                normalizedX - size / 2,
                normalizedY - size / 2
            )), CameraTransform.ScreenPointToWorld(new Vector2(
                normalizedX - size / 2,
                normalizedY + size / 2
            )));
    }
}
