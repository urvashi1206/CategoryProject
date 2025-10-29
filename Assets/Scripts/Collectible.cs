using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public ItemSO itemData;

    [Header("Spin around WORLD axes")]
    public Vector3 worldDegreesPerSecond = new Vector3(0f, 50f, 0f); // X,Y,Z speeds

    // we only store your original rotation; we never modify it
    private Quaternion baseRotation;
    private Vector3 angles; // accumulated world-angles

    // Start is called before the first frame update
    void Start()
    {
        baseRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        // accumulate angles and keep them in [0,360) so they don't grow forever
        angles += worldDegreesPerSecond * Time.deltaTime;
        angles.x = Mathf.Repeat(angles.x, 360f);
        angles.y = Mathf.Repeat(angles.y, 360f);
        angles.z = Mathf.Repeat(angles.z, 360f);

        // IMPORTANT: left-multiply (world rotation) * baseRotation.
        // This applies a rotation in WORLD space without changing your original orientation.
        Quaternion worldSpin = Quaternion.Euler(angles);
        transform.rotation = worldSpin * baseRotation;
    }

    void OnTriggerEnter(Collider other)
    {
        // Get the root that owns this collider (if it has a Rigidbody)
        GameObject hitRoot = other.attachedRigidbody ? other.attachedRigidbody.gameObject
                                                     : other.gameObject;

        if (!hitRoot.CompareTag("Player")) return;

        //GameEvents.CollectItem(itemData);
        GameEvents.CollectItemAt(itemData, transform.position);
        Destroy(gameObject);
    }
}
