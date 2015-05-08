/*
 * Author: dtobi, Firov
 * This work is shared under Creative Commons CC BY-NC-SA 3.0 license.
 *
 */

using KSP.IO;
using KSPAPIExtensions;
using KSPAPIExtensions.PartMessage;
using KSPAPIExtensions.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Lib
{
    public class Stager : PartModule
    {

        #region Fields

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Group"),
            UI_ChooseOption(
                options = new String[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15" },
                display = new String[] { "Stage", "AG1", "AG2", "AG3", "AG4", "AG5", "AG6", "AG7", "AG8", "AG9", "AG10", "Lights", "RCS", "SAS", "Brakes", "Abort" }
            )]
        public string group = "0";

        //AGXGroup shows if AGX installed and hides Group above
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Group"),
            UI_ChooseOption(
            options = new String[] {
                "0",
                "1",
                "11",
                "12",
                "13",
                "14",
                "15"
            },
            display = new String[] {
                "Stage",
                "Action Group:",
                "Lights",
                "RCS",
                "SAS",
                "Brakes",
                "Abort"
            }
        )]
        public string agxGroupType = "0";

        // AGX Action groups, use own slider if selected, only show this field if AGXGroup above is 1
        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Group:", guiFormat = "N0"),
            UI_FloatEdit(scene = UI_Scene.All, minValue = 1f, maxValue = 250f, incrementLarge = 75f, incrementSmall = 25f, incrementSlide = 1f)]
        public float agxGroupNum = 1;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Resource"),
            UI_ChooseOption(
                options = new string[] { "Empty" }
            )]
        public string monitoredResource = "Empty";

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Resource")]
        public String resourceFlightDisplay = "Empty";

        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = true, guiName = "Percentage", guiFormat = "F0", guiUnits = "%"),
            UI_FloatEdit(scene = UI_Scene.All, minValue = 0f, maxValue = 100f, incrementSlide = 1f)]
        public float activationPercentage = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Detection"),
            UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        public bool isActive = true;

        [KSPAction("Activate Detection")]
        public void doActivateAG(KSPActionParam param) {
            isActive = true;
        }

        [KSPAction("Deactivate Detection")]
        public void doDeActivateAG(KSPActionParam param) {
            isActive = false;
        }

        #endregion

        #region Variables

        private Part observedPart = null;
        private string groupLastUpdate = "0"; //AGX: What was our selected group last update frame? Top slider.

        #endregion

        #region Overrides

        public override void OnStart(StartState state) {
            if (state == StartState.Editor) {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
            }
            print("KM Stager Started");
            //Find which part we should be monitoring in flight
            if (HighLogic.LoadedSceneIsFlight) {
                findObservedPart();
                //Update static resource flight display with correct resource name
                resourceFlightDisplay = monitoredResource;
            }
            //Find which part we should be monitoring, and update the fuel list in the editor
            if (HighLogic.LoadedSceneIsEditor && this.part.parent != null) {
                findObservedPart();
                updateList();
            }
            updateButtons();
        }

        public override void OnAwake() {
            PartMessageService.Register(this);
        }

        public override void OnUpdate() {
            if (isActive && observedPart != null && monitoredResource != "Empty") {
                //Check fuel percantage and compare it to target percentage, can not be true zero due to float errors
                float activationPercentageNonZero;
                if(activationPercentage < 0.5) //can not use == 0 due to float errors
                {
                    activationPercentageNonZero = 0.1f; //activate at zero percent set, give us a buffer
                }
                else
                {
                    activationPercentageNonZero = activationPercentage; //non-zero activation set, do not tweak it
                }
                if (((observedPart.Resources[monitoredResource].amount / observedPart.Resources[monitoredResource].maxAmount) * 100) <= activationPercentageNonZero) {
                    int groupToFire = 0; //AGX: need to send correct group
                    if (AGXInterface.AGExtInstalled())
                    {
                        groupToFire = int.Parse(agxGroupType);
                    }
                    else
                    {
                        groupToFire = int.Parse(group);
                    }
                    Helper.fireEvent(this.part, groupToFire,(int)agxGroupNum);
                    print("KM Stager: Target percentage hit");
                    isActive = false;
                }
            }

        }

        public void Update() //AGX: The OnUpdate above only seems to run in flight mode, Update() here runs in all scenes
        {
            if (agxGroupType == "1" & groupLastUpdate != "1" || agxGroupType != "1" & groupLastUpdate == "1") //AGX: Monitor group to see if we need to refresh window
            {
                updateButtons();
                refreshPartWindow();
                if (agxGroupType == "1")
                {
                    groupLastUpdate = "1";
                }
                else
                {
                    groupLastUpdate = "0";
                }
            }
        }
        public override string GetInfo() {
            return "Built-in auto staging smart part";
        }

        private void updateButtons()
        {
            
            //Change to AGX buttons if AGX installed
            if (AGXInterface.AGExtInstalled())
            {
                Fields["group"].guiActiveEditor = false;
                Fields["group"].guiActive = false;
                Fields["agxGroupType"].guiActiveEditor = true;
                Fields["agxGroupType"].guiActive = true;
                //Fields["agxGroupNum"].guiActiveEditor = true;
                //Fields["agxGroupNum"].guiActive = true;
                if (agxGroupType == "1") //only show groups select slider when selecting action group
                {
                    Fields["agxGroupNum"].guiActiveEditor = true;
                    Fields["agxGroupNum"].guiActive = true;
                    //Fields["agxGroupNum"].guiName = "Group:";
                }
                else
                {
                    Fields["agxGroupNum"].guiActiveEditor = false;
                    Fields["agxGroupNum"].guiActive = false;
                    //Fields["agxGroupNum"].guiName = "N/A";
                    //agxGroupNum = 1;
                }
            }
            else //AGX not installed, leave at default
            {
                Fields["group"].guiActiveEditor = true;
                Fields["group"].guiActive = true;
                Fields["agxGroupType"].guiActiveEditor = false;
                Fields["agxGroupType"].guiActive = false;
                Fields["agxGroupNum"].guiActiveEditor = false;
                Fields["agxGroupNum"].guiActive = false;
            }
        }

        private void refreshPartWindow() //AGX: Refresh right-click part window to show/hide Groups slider
        {
            UIPartActionWindow[] partWins = FindObjectsOfType<UIPartActionWindow>();
            //print("Wind count " + partWins.Count());
            foreach (UIPartActionWindow partWin in partWins)
            {
                partWin.displayDirty = true;
            }
        }

        #endregion

        #region Methods

        private void findObservedPart() {
            //If this is a smart fuel tank, monitor self
            if (this.part.Resources.Count > 0) {
                print("KM Stager: Monitoring this part");
                observedPart = this.part;
            }
            //Otherwise monitor the parent part
            else {
                print("KM Stager: Monitoring parent part");
                observedPart = this.part.parent;
            }
            print("KM Stager: Set observed part to " + observedPart.partName + "Active is: " + isActive);
        }

        private void updateList() {
            //Create temporary string list to collect resources
            List<string> resourceList = new List<string>();
            //Instantiate monitoredResource options so we can access its option array
            UI_ChooseOption resourceOptions = (UI_ChooseOption)Fields["monitoredResource"].uiControlEditor;
            if (observedPart != null) {
                if (observedPart.Resources.Count > 0) {
                    //Iterate through resources and add them to the resourceList
                    foreach (PartResource resource in observedPart.Resources) {
                        resourceList.Add(resource.resourceName);
                    }
                    //Convert resource list to array and assign it to the monitoredResource options
                    resourceOptions.options = resourceList.ToArray<String>();
                    //If we already have selected a resource, don't reassign it to the default
                    monitoredResource = (resourceList.Contains(monitoredResource) ? monitoredResource : resourceList[0]);
                }
                else {
                    //If there are no resources in the monitored part, set monitoredResource to "Empty"
                    resourceOptions.options = new string[] { "Empty" };
                    monitoredResource = "Empty";
                }
            }
        }

        [PartMessageListener(typeof(PartResourceListChanged), scenes: GameSceneFilter.AnyEditor, relations: PartRelationship.Parent)]
        [PartMessageListener(typeof(PartResourceListChanged), scenes: GameSceneFilter.AnyEditor, relations: PartRelationship.Self)]
        private void changeListener() {
            print("KM Stager: Monitored part resoruces changed. Updating.");
            findObservedPart();
            updateList();
        }

        private void OnEditorAttach() {
            RenderingManager.AddToPostDrawQueue(99, updateEditor);
            findObservedPart();
            updateList();
        }

        private void OnEditorDetach() {
            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
        }

        private void OnEditorDestroy() {
            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
        }

        private void updateEditor() {
        }

        #endregion
    }
}

