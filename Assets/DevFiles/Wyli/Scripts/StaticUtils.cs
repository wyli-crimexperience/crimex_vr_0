using System;

using UnityEngine;



public static class StaticUtils {

    private static float dtAngle, hdtAngle, midAngle, offset;

    public static float ClampAngle(float current, float min, float max) {
        dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
        hdtAngle = dtAngle * 0.5f;
        midAngle = min + hdtAngle;

        offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - hdtAngle;
        if (offset > 0)
            current = Mathf.MoveTowardsAngle(current, midAngle, offset);
        return current;
    }

    public static DateTime ConvertToMorning(DateTime date) {
        if (date.Hour < 6 || date.Hour > 18) {
            date.AddHours(12);
        }
        return date;
    }
    public static DateTime ConvertToEvening(DateTime date) {
        if (date.Hour > 6 || date.Hour < 18) {
            date.AddHours(12);
        }
        return date;
    }

}