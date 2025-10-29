using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLoader : MonoBehaviour
{
    public CarCatalogSO catalog;
    public Transform spawnPoint; // empty at the start line

    void Awake()
    {
        CarUnlocks.InitDefaults(catalog);

        int id = CarUnlocks.GetSelectedCarId();
        var skin = catalog.GetById(id);
        if (!skin || !skin.prefab)
        {
            // fallback
            id = CarUnlocks.GetFirstUnlockedId(catalog);
            skin = catalog.GetById(id);
        }

        if (skin && skin.prefab && spawnPoint)
        {
            Instantiate(skin.prefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning("CarLoader: Missing skin, prefab, or spawn point.");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
