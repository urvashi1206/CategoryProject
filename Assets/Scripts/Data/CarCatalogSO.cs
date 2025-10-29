using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrashCourse/Cars/Car Catalog")]
public class CarCatalogSO : ScriptableObject
{
    public List<CarSkinSO> cars = new List<CarSkinSO>();

    public CarSkinSO GetById(int id)
    {
        for (int i = 0; i < cars.Count; i++)
            if (cars[i] && cars[i].id == id) return cars[i];
        return null;
    }

    public CarSkinSO GetFirstUnlockedDefault()
    {
        foreach (var c in cars) if (c && c.defaultUnlocked) return c;
        return cars.Count > 0 ? cars[0] : null;
    }
}
