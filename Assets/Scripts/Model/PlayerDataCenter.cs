using UnityEngine;
using System.Collections;

public static class PlayerDataCenter {

    public static RoleInfo CurrentRoleInfo;

    public static void InitPlayerStatus() {
        CurrentRoleInfo = new RoleInfo();
        CurrentRoleInfo.Fuel = CurrentRoleInfo.MaxFuel = 240;
        CurrentRoleInfo.Oxygen = CurrentRoleInfo.MaxOxygen = 120;
        CurrentRoleInfo.ElectricityAmount = 10;
        CurrentRoleInfo.TechAmount = 10;
        CurrentRoleInfo.OxygenSpeed = 2;
        CurrentRoleInfo.LowFuelSpeed = 1;
        CurrentRoleInfo.MiddleFuelSpeed = 4;
        CurrentRoleInfo.HighFuelSpeed = 6;
    }
}
