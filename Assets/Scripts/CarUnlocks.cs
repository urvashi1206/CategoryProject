using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarUnlocks
{
    const string UnlockMaskKey = "CarUnlockMask";   // bit i = unlocked for car id i
    const string SelectedKey = "CurrentCarId";

    public static void InitDefaults(CarCatalogSO catalog)
    {
        if (catalog == null || catalog.cars.Count == 0) return;

        // Initialize unlock mask only once
        if (!PlayerPrefs.HasKey(UnlockMaskKey))
        {
            int mask = 0;
            foreach (var car in catalog.cars)
            {
                if (!car) continue;
                if (car.defaultUnlocked) mask |= (1 << car.id);
            }
            PlayerPrefs.SetInt(UnlockMaskKey, mask);
        }

        // Ensure there is a valid selected car
        if (!PlayerPrefs.HasKey(SelectedKey) || !IsUnlocked(GetSelectedCarId()))
        {
            int first = GetFirstUnlockedId(catalog);
            SetSelectedCarId(first);
        }
    }

    public static int GetUnlockMask() => PlayerPrefs.GetInt(UnlockMaskKey, 0);

    public static bool IsUnlocked(int id)
    {
        int mask = GetUnlockMask();
        return (mask & (1 << id)) != 0;
    }

    public static void Unlock(int id)
    {
        int mask = GetUnlockMask();
        mask |= (1 << id);
        PlayerPrefs.SetInt(UnlockMaskKey, mask);
    }

    public static int GetSelectedCarId() => PlayerPrefs.GetInt(SelectedKey, 0);

    public static void SetSelectedCarId(int id)
    {
        PlayerPrefs.SetInt(SelectedKey, id);
        PlayerPrefs.Save();
    }

    public static int GetFirstUnlockedId(CarCatalogSO catalog)
    {
        foreach (var car in catalog.cars)
            if (car && IsUnlocked(car.id)) return car.id;
        // fallback to first entry
        return (catalog.cars.Count > 0 && catalog.cars[0]) ? catalog.cars[0].id : 0;
    }

    /// <summary>
    /// Unlock all cars whose unlockScore <= score. Returns display names of newly unlocked cars.
    /// </summary>
    public static List<string> UnlockEligible(CarCatalogSO catalog, int score)
    {
        var newly = new List<string>();
        if (!catalog) return newly;

        foreach (var car in catalog.cars)
        {
            if (!car || car.defaultUnlocked) continue;
            if (IsUnlocked(car.id)) continue;
            if (score >= car.unlockScore)
            {
                Unlock(car.id);
                newly.Add(car.displayName);
            }
        }

        if (newly.Count > 0) PlayerPrefs.Save();
        return newly;
    }
}
