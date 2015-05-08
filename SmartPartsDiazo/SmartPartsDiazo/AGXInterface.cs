﻿/*
 * Author: dtobi, Firov
 * This work is shared under Creative Commons CC BY-NC-SA 3.0 license.
 *
 */

/* Action Groups Extended Interface
 * Author: Diazo
 * Action Groups Extended Mod info: http://forum.kerbalspaceprogram.com/threads/74195
 * More info on interface: http://forum.kerbalspaceprogram.com/threads/74199
 * 
 * This version of the interface is released as part of the Smart Parts mod
 * and is licensed the same as per the paragraph above.
 * 
 * Please visit the More info link above for a Public Domain version you can use as you see fit.
 * */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSPAPIExtensions;
using System.Reflection;

namespace Lib
{
    public class AGXInterface
    {
        public static bool AGExtInstalled() //is AGX installed on this KSP game?
        {
            try //try-catch is required as the below code returns a NullRef if AGX is not present.
            {
                Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
                return (bool)calledType.InvokeMember("AGXInstalled", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, null);
            }
            catch
            {
                return false;
            }
        }

        public static bool AGX2VslToggleGroup(uint FlightID, int group) //toggle action group on specific ship. FlightID is FlightID of rootPart of ship, not of part with action to enable
        {
            Type calledType = Type.GetType("ActionGroupsExtended.AGExtExternal, AGExt");
            bool GroupAct = (bool)calledType.InvokeMember("AGX2VslToggleGroup", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static, null, null, new System.Object[] { FlightID, group });
            return GroupAct;
        }
    }
}
