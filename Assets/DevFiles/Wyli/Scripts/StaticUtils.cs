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

    public static string ConvertToLetter(int index) {
        switch (index) {
            case 0: { return "A"; }
            case 1: { return "B"; }
            case 2: { return "C"; }
            case 3: { return "D"; }
            case 4: { return "E"; }
            case 5: { return "F"; }
            case 6: { return "G"; }
            case 7: { return "H"; }
            case 8: { return "I"; }
            case 9: { return "J"; }
            case 10: { return "K"; }
            case 11: { return "L"; }
            case 12: { return "M"; }
            case 13: { return "N"; }
            case 14: { return "O"; }
            case 15: { return "P"; }
            case 16: { return "Q"; }
            case 17: { return "R"; }
            case 18: { return "S"; }
            case 19: { return "T"; }
            case 20: { return "U"; }
            case 21: { return "V"; }
            case 22: { return "W"; }
            case 23: { return "X"; }
            case 24: { return "Y"; }
            case 25: { return "Z"; }
            default: { return ""; }
        }
    }

}