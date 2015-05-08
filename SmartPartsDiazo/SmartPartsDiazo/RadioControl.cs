/*
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
using KSPAPIExtensions;

namespace Lib
{
    internal static class Channel
    {
        public static List<RadioControl> radioListeners = new List<RadioControl>();
    }

    public class RadioControl : PartModule
    {
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false, guiName = "Sync. Head."),
            UI_Toggle(disabledText = "Disabled", enabledText = "Enabled")]
        private bool enableSync = false;
        private int updateCounter = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Channel"), UI_FloatRange(minValue = 1f, maxValue = 20f, stepIncrement = 1f)]
        public float channel = 1;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Group"),
            UI_ChooseOption(
            options = new String[] {
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                "10",
                "11",
                "12",
                "13",
                "14",
                "15"
            },
            display = new String[] {
                "Stage",
                "AG1",
                "AG2",
                "AG3",
                "AG4",
                "AG5",
                "AG6",
                "AG7",
                "AG8",
                "AG9",
                "AG10",
                "Lights",
                "RCS",
                "SAS",
                "Brakes",
                "Abort"
            }
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

        double fireTime = 0;
        double lightOnTime = 2;
        private string groupLastUpdate = "0"; //AGX: What was our selected group last update frame? Top slider.

        [KSPField]
        public string rcv_sound = "";

        [KSPAction("Transmit")]
        public void transmit_AG(KSPActionParam param) {
            if (AGXInterface.AGExtInstalled())
            {
                transmitCommand(float.Parse(agxGroupType), agxGroupNum);
            }
            else
            {
                transmitCommand(float.Parse(group));
            }
        }

        [KSPAction("Transmit Stage")]
        public void transmit_AG0(KSPActionParam param) {
            transmitCommand(0);
        }

        [KSPAction("Transmit AG1")]
        public void transmit_AG1(KSPActionParam param) {
            transmitCommand(1);
        }

        [KSPAction("Transmit AG2")]
        public void transmit_AG2(KSPActionParam param) {
            transmitCommand(2);
        }

        [KSPAction("Transmit AG3")]
        public void transmit_AG3(KSPActionParam param) {
            transmitCommand(3);
        }

        [KSPAction("Transmit AG4")]
        public void transmit_AG4(KSPActionParam param) {
            transmitCommand(4);
        }

        [KSPAction("Transmit AG5")]
        public void transmit_AG5(KSPActionParam param) {
            transmitCommand(5);
        }

        [KSPAction("Transmit AG6")]
        public void transmit_AG6(KSPActionParam param) {
            transmitCommand(6);
        }

        [KSPAction("Transmit AG7")]
        public void transmit_AG7(KSPActionParam param) {
            transmitCommand(7);
        }

        [KSPAction("Transmit AG8")]
        public void transmit_AG8(KSPActionParam param) {
            transmitCommand(8);
        }

        [KSPAction("Transmit AG9")]
        public void transmit_AG9(KSPActionParam param) {
            transmitCommand(9);
        }

        [KSPAction("Transmit AG10")]
        public void transmit_AG10(KSPActionParam param) {
            transmitCommand(10);
        }


        [KSPAction("Transmit Light")]
        public void transmit_Light(KSPActionParam param) {
            transmitCommand(11);
        }

        [KSPAction("Transmit RCS")]
        public void transmit_RCS(KSPActionParam param) {
            transmitCommand(12);
        }

        [KSPAction("Transmit SAS")]
        public void transmit_SAS(KSPActionParam param) {
            transmitCommand(13);
        }

        [KSPAction("Transmit Brakes")]
        public void transmit_Brakes(KSPActionParam param) {
            transmitCommand(14);
        }

        [KSPAction("Transmit Abort")]
        public void transmit_Abort(KSPActionParam param) {
            transmitCommand(15);
        }

        [KSPEvent(guiName = "Transmit Command", guiActive = true)]
        public void transmit_GUI() {
            if (AGXInterface.AGExtInstalled())
            {
                transmitCommand(float.Parse(agxGroupType), agxGroupNum);
            }
            else
            {
                transmitCommand(float.Parse(group));
            }
        }

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Throttle"), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.05f)]
        public float throttle = 1;


        [KSPAction("Transmit Throttle")]
        public void transmit_ThrottleAG(KSPActionParam param) {
            transmitThrottle();
        }

        [KSPEvent(guiName = "Transmit Throttle", guiActive = true)]
        public void transmit_Throttle() {
            transmitThrottle();
        }

        [KSPAction("Match Heading")]
        public void transmit_pro_rotationAG(KSPActionParam param) {
            transmitRotation(this.vessel.GetTransform().rotation, true);
        }

        [KSPEvent(guiName = "Match Heading", guiActive = true)]
        public void transmit_pro_rotation() {
            transmitRotation(this.vessel.GetTransform().rotation, true);
        }
        /*
        [KSPAction("Head to vessel")]
        public void transmit_rotateTowards (KSPActionParam param)
        {
            transmitRotationTo (vessel.findWorldCenterOfMass(), true);
        }

        [KSPEvent(guiName = "Head to vessel", guiActive = true)]
        public void transmit_rotateTowardsAG ()
        {
            transmitRotationTo (vessel.findWorldCenterOfMass(), true);
        }
        */

        public void transmitRotation(Quaternion rotation, bool playSound) {
            foreach (var listener in Channel.radioListeners) {
                if (listener != null && listener.vessel != null)
                    listener.receiveRotation(this, rotation, (int)channel, playSound);
            }

            indicateSend();

        }
        /*
        public void transmitRotationTo(Vector3 target, bool playSound){
            foreach (var listener in Utility.radioListeners) {  
                if(listener != null && listener.vessel!= null)  
                    listener.receiveRotation (this, Quaternion.LookRotation((target-listener.vessel.findWorldCenterOfMass()).normalized), (int) channel, playSound);            }
            indicateSend ();
      
        
        }*/

        public void receiveRotation(RadioControl sender, Quaternion targetUp, int transmitChannel, bool playSound) {


            if (this == sender || channel != transmitChannel || this.vessel == FlightGlobals.ActiveVessel) {
                MonoBehaviour.print("I am the active vessel or the sender or channels are not equal:" + channel + ", " + transmitChannel);
                return;
            }

            this.vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
            print("Listener:" + vessel.vesselName + "received command" + group);
            this.vessel.Autopilot.SAS.LockHeading(targetUp, true);
            this.vessel.Autopilot.SAS.Update();
            indicateReceive(playSound);
        }

        public void transmitCommand(float groupID) //backwards compatibility placeholder
        {
            transmitCommand(groupID, 1); //if agx is installed and groupID is 1, it will acivate agx so send 1 to activate correct group.
        }
        
        public void transmitCommand(float groupID, float agxGroupNumB) { //agxGroupNum only used when AGX installed, ignore otherwise
            foreach (var listener in Channel.radioListeners) {
                if (listener != null && listener.vessel != null)
                { 
                    if(AGXInterface.AGExtInstalled()) //AGX Edited
                    {
                        listener.receiveCommand(this, (int)groupID, (int)channel, (int)agxGroupNumB);
                    }
                    else
                    {
                        listener.receiveCommand(this, (int)groupID, (int)channel, (int)agxGroupNumB);
                    }
                }
            }

            indicateSend();
        }

        public void transmitThrottle() {
            foreach (var listener in Channel.radioListeners) {
                if (listener != null && listener.vessel != null)
                    listener.receiveThrottle(this, throttle, (int)channel);
            }
            indicateSend();

        }

        private void indicateSend() {
            Utility.switchLight(this.part, "light-go", true);
            Utility.playAnimationSetToPosition(this.part, "glow", 1);
            fireTime = Time.fixedTime;
            print("Fire Time:" + fireTime);
        }

        private void indicateReceive(bool playSound) {
            Utility.switchLight(this.part, "light-go", true);
            Utility.playAnimationSetToPosition(this.part, "glow", 1);
            fireTime = Time.fixedTime;
            if (playSound)
                Utility.playAudio(this.part, rcv_sound);
            print("Fire Time:" + fireTime);
        }

        public void receiveCommand(RadioControl sender, int group, int transmitChannel, int agxGroup) //AGX Edited, agxGroup only used if AGX installed
        {
            if (this == sender || channel != transmitChannel) {
                MonoBehaviour.print("I am the sender or channels are not equal:" + channel + ", " + transmitChannel);
                return;
            }
            print("Listener:" + vessel.vesselName + "received command" + group + "|" + agxGroup);
            Helper.fireEvent(this.part, (int)group, (int)agxGroup);
            indicateReceive(true);
        }

        public void receiveThrottle(RadioControl sender, float throttle, int transmitChannel) {
            if (this == sender || channel != transmitChannel) {
                MonoBehaviour.print("I am the sender or channels are not equal:" + channel + ", " + transmitChannel);
                return;
            }
            print("Listener:" + vessel.vesselName + "received command" + group);
            this.vessel.ctrlState.mainThrottle = throttle;
            indicateReceive(true);
        }

        ~RadioControl() {
            MonoBehaviour.print("Destructor called");
        }

        public override void OnInactive() {
            MonoBehaviour.print("OnInactive called");
            base.OnInactive();
        }

        public override void OnStart(StartState state) {
            if (state == StartState.Editor) {
                this.part.OnEditorAttach += OnEditorAttach;
                this.part.OnEditorDetach += OnEditorDetach;
                this.part.OnEditorDestroy += OnEditorDestroy;
                this.part.OnJustAboutToBeDestroyed += OnJustAboutToBeDestroyed;



                OnEditorAttach();
            }
            else {
                Channel.radioListeners.Add(this);
                foreach (var listener in Channel.radioListeners) {
                    if (listener.vessel != null)
                        print("Listener found:" + listener.vessel.vesselName);
                }
            }
            updateButtons();
        }



        public override void OnUpdate() {

            if (enableSync && updateCounter % 30 == 0 && this.vessel == FlightGlobals.ActiveVessel) {
                transmitRotation(this.vessel.GetTransform().rotation, false);
            }
            updateCounter++;

            if (fireTime != 0 && fireTime + lightOnTime <= Time.time) {
                Utility.switchLight(this.part, "light-go", false);
                Utility.playAnimationSetToPosition(this.part, "glow", 0);
            }

            //if (fireTime != 0 && fireTime + 10 <= Time.time) {
            //    this.vessel.vesselSAS.Update ();
            //}

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


        public void OnDetach(bool first) {
            Channel.radioListeners.Remove(this);
            MonoBehaviour.print("OnDetach");
        }

        private void OnJustAboutToBeDestroyed() {
            Channel.radioListeners.Remove(this);
            MonoBehaviour.print("OnJustAboutToBeDestroyed");
        }

        private void OnEditorAttach() {
            RenderingManager.AddToPostDrawQueue(99, updateEditor);
        }

        private void OnEditorDetach() {

            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);
        }

        private void OnEditorDestroy() {
            RenderingManager.RemoveFromPostDrawQueue(99, updateEditor);

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

        private void updateEditor() {

        }
    }
}

