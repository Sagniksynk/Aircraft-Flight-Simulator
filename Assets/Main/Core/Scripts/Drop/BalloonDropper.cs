using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BalloonDropper : MonoBehaviour
{
    [Header("Balloon")]
    [SerializeField]
    GameObject balloonPrefab = null;
    [SerializeField]
    Transform dropPoint = null;
    [SerializeField]
    int maxBalloons = 10;
    [SerializeField]
    float dropCooldown = 0.5f;
    [SerializeField]
    float balloonLifetime = 30f;

    [Header("Detection")]
    [SerializeField]
    float zoneDetectionRange = 50f;

    [Header("UI")]
    [SerializeField]
    Text balloonText = null;

    int balloonsRemaining;
    float lastDropTime;
    List<DropZone> allZones = new List<DropZone>();
    Rigidbody rb;

    private void Start()
    {
        balloonsRemaining = maxBalloons;
        lastDropTime = -dropCooldown;
        rb = GetComponent<Rigidbody>();

        // Collect all drop zones in the scene
        allZones.AddRange(FindObjectsOfType<DropZone>());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && CanDrop())
        {
            Drop();
        }

        UpdateUI();
    }

    bool CanDrop()
    {
        return balloonsRemaining > 0
            && balloonPrefab != null
            && Time.time - lastDropTime >= dropCooldown;
    }

    void Drop()
    {
        Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position - transform.up * 2f;
        GameObject balloon = Instantiate(balloonPrefab, spawnPos, Quaternion.identity);

        // Inherit aircraft velocity so balloon follows realistic trajectory
        Rigidbody balloonRb = balloon.GetComponent<Rigidbody>();
        if (balloonRb != null && rb != null)
        {
            balloonRb.linearVelocity = rb.linearVelocity;
        }

        Destroy(balloon, balloonLifetime);

        balloonsRemaining--;
        lastDropTime = Time.time;

        // Score: check if aircraft is over any incomplete drop zone
        foreach (var zone in allZones)
        {
            if (zone != null && !zone.IsCompleted && zone.IsInsideZone(spawnPos))
            {
                zone.MarkCompleted();
                Debug.Log("Drop zone completed: " + zone.ZoneName);
                break;
            }
        }
    }

    void UpdateUI()
    {
        if (balloonText == null) return;

        // Count completed zones
        int completed = 0;
        int total = 0;
        DropZone nearest = null;
        float nearestDist = zoneDetectionRange;

        foreach (var zone in allZones)
        {
            if (zone == null) continue;
            total++;
            if (zone.IsCompleted) { completed++; continue; }

            float dist = Vector3.Distance(transform.position, zone.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = zone;
            }
        }

        balloonText.text = "BALLOONS: " + balloonsRemaining + "/" + maxBalloons;
        balloonText.text += "\nZONES: " + completed + "/" + total;

        if (nearest != null)
        {
            balloonText.text += "\n-> " + nearest.ZoneName + " (" + (int)nearestDist + "m)";
        }

        if (completed >= total && total > 0)
        {
            balloonText.text += "\n<color=green>ALL ZONES COMPLETE!</color>";
        }
    }
}