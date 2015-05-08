﻿/*
 * Author: dtobi, Firov
 * This work is shared under Creative Commons CC BY-NC-SA 3.0 license.
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Lib
{
    public static class Helper
    {
        #region Dictionary

        public static Dictionary<int, KSPActionGroup> KM_dictAG = new Dictionary<int, KSPActionGroup> {
            { 0,  KSPActionGroup.None },
            { 1,  KSPActionGroup.Custom01 },
            { 2,  KSPActionGroup.Custom02 },
            { 3,  KSPActionGroup.Custom03 },
            { 4,  KSPActionGroup.Custom04 },
            { 5,  KSPActionGroup.Custom05 },
            { 6,  KSPActionGroup.Custom06 },
            { 7,  KSPActionGroup.Custom07 },
            { 8,  KSPActionGroup.Custom08 },
            { 9,  KSPActionGroup.Custom09 },
            { 10, KSPActionGroup.Custom10 },
            { 11, KSPActionGroup.Light },
            { 12, KSPActionGroup.RCS },
            { 13, KSPActionGroup.SAS },
            { 14, KSPActionGroup.Brakes },
            { 15, KSPActionGroup.Abort }
        };

        public static Dictionary<int, String> KM_dictAGNames = new Dictionary<int, String> {
            { 0,  "Stage" },
            { 1,  "Custom01" },
            { 2,  "Custom02" },
            { 3,  "Custom03" },
            { 4,  "Custom04" },
            { 5,  "Custom05" },
            { 6,  "Custom06" },
            { 7,  "Custom07" },
            { 8,  "Custom08" },
            { 9,  "Custom09" },
            { 10, "Custom10" },
            { 11, "Light" },
            { 12, "RCS" },
            { 13, "SAS" },
            { 14, "Brakes" },
            { 15, "Abort" },
            { 16, "Beep" },
        };

        public static int maxEvent = 17;

        #endregion


        #region Staging/AG Methods

        public static void fireEvent(Part p, int eventID, int AGXgroup) { //AGXgroup is only used if Action Groups Extended is installed, ignore it otherwiseDebug.Log("fire " + p.name + eventID + "||" + AGXgroup);
            if (p == null)
                return;
            if (eventID == 0) {
                MonoBehaviour.print("Fire Stage from part " + p);
                fireNextNonEmptyStage(p.vessel);
                return;
            }
            else if(AGXInterface.AGExtInstalled() && eventID == 1) 
            {
                AGXInterface.AGX2VslToggleGroup(p.vessel.rootPart.flightID, AGXgroup); //call to agx to activate group
            }
            else if (eventID > 0 && eventID <= maxEvent) {
                MonoBehaviour.print("Fire Event " + KM_dictAGNames[eventID] + " from part " + p);
                p.vessel.ActionGroups.ToggleGroup(KM_dictAG[eventID]);
            }
        }

        public static void fireNextNonEmptyStage(Vessel v) {
            // the parts to be fired
            List<Part> resultList = new List<Part>();

            int highestNextStage = getHighestNextStage(v.rootPart, v.currentStage);
            traverseChildren(v.rootPart, highestNextStage, ref resultList);

            foreach (Part stageItem in resultList) {
                MonoBehaviour.print("Activate:" + stageItem);
                stageItem.activate(highestNextStage, stageItem.vessel);
                stageItem.inverseStage = v.currentStage;
            }
            v.currentStage = highestNextStage;
            //If this is the currently active vessel, activate the next, now empty, stage. This is an ugly, ugly hack but it's the only way to clear out the empty stage.
            //Switching to a vessel that has been staged this way already clears out the empty stage, so this isn't required for those.
            if (v.isActiveVessel) {
                Staging.ActivateNextStage();
            }
        }

        private static int getHighestNextStage(Part p, int currentStage) {

            int highestChildStage = 0;

            // if this is the root part and its a decoupler: ignore it. It was probably fired before.
            // This is dirty guesswork but everything else seems not to work. KSP staging is too messy.
            if (p.vessel.rootPart == p &&
                (p.name.IndexOf("ecoupl") != -1 || p.name.IndexOf("eparat") != -1)) {
            }
            else if (p.inverseStage < currentStage) {
                highestChildStage = p.inverseStage;
            }


            // Check all children. If this part has no children, inversestage or current Stage will be returned
            int childStage = 0;
            foreach (Part child in p.children) {
                childStage = getHighestNextStage(child, currentStage);
                if (childStage > highestChildStage && childStage < currentStage) {
                    highestChildStage = childStage;
                }
            }
            return highestChildStage;
        }

        private static void traverseChildren(Part p, int nextStage, ref List<Part> resultList) {
            if (p.inverseStage >= nextStage) {
                resultList.Add(p);
            }
            foreach (Part child in p.children) {
                traverseChildren(child, nextStage, ref resultList);
            }
        }

        #endregion
    }
}
