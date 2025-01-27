using UnityEngine;
using System.Collections.Generic;
using ModCommon.Util;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace Easier_Pantheon_Practice
{
    internal class BossNerf : MonoBehaviour
    {
        private HealthManager health;
        private PlayMakerFSM _control;
        private PlayMakerFSM _attackCommands;


        Dictionary<string, int> Health_CurrentBoss = new Dictionary<string, int>() //dict for boss healths
        {
            {"Giant Buzzer Col", 190},
            {"Giant Fly", 650},
            {"False Knight New", 260},
            {"Mega Moss Charger", 480},
            {"Hornet Boss 1", 900},
            {"Ghost Warrior Slug", 650},
            {"Dung Defender", 800},
            {"Mage Knight", 750},
            {"Mawlek Body", 750},
            {"Oro", 500},
            {"Ghost Warrior Xero", 650},
            {"Mega Zombie Beam Miner (1)", 650},
            {"Mage Lord", 600},
            {"Mega Fat Bee", 450},
            {"Mantis Lord", 500},
            {"Ghost Warrior Marmu", 416},
            {"Fluke Mother", 500},
            {"Infected Knight", 700},
            {"Ghost Warrior Galien", 650},
            {"Sheo Boss", 950},
            {"Hive Knight", 850},
            {"Ghost Warrior Hu", 600},
            {"Jar Collector", 900},
            {"Lancer", 750},
            {"Grimm Boss", 1000},
            {"Black Knight ", 350},
            {"Mega Jellyfish GG", 350},
            {"Hornet Nosk", 750},
            {"Nosk", 680},
            {"Sly Boss", 800},
            {"Hornet Boss 2", 800},
            {"Zombie Beam Miner Rematch", 650},
            {"Lost Kin", 1200},
            {"Ghost Warrior No Eyes", 570},
            {"Mantis Traitor Lord", 800},
            {"White Defender", 1600},
            {"Dream Mage Lord", 900},
            {"Ghost Warrior Markoth", 650},
            {"Grey Prince", 1400},
            {"False Knight Dream", 360},
            {"Nightmare Grimm Boss", 1250},
            {"HK Prime", 1600},
            {"Absolute Radiance", 3000},
        };

        Dictionary<string, int> Health_CurrentBoss_1 = new Dictionary<string, int>()
        {
            {"Giant Buzzer Col (1)", 450},
            {"Dream Mage Lord Phase2", 350},
            {"Lobster", 750},
            {"Mega Fat Bee (1)", 450},
            {"Mage Lord Phase2", 350},
            {"Mato", 1000},
            {"Mantis Lord S", 750},
            {"Black Knight ", 350},
        };



        private void Awake()
        {
            health = gameObject.GetComponent<HealthManager>();
        }

        private void Start()
        {
            
            if (FindBoss.altered == false)//if this isnt there then all bosses in scene get their health changed to the boss that is in the main dict
            {
                health.hp = Health_CurrentBoss[FindBoss.CurrentBoss];
                FindBoss.altered = true;
                if (!FindBoss.SOB) health.hp = 400;
            }
                
            else
            {
                health.hp = Health_CurrentBoss_1[FindBoss.CurrentBoss_1];
                if (!FindBoss.SOB) health.hp = 350;
            }

            ChangeFSM();
        }


        #region Changing FSMS

        private void ChangeFSM()
        {
            switch (FindBoss.CurrentBoss)
            {
                case "Grimm Boss":
                case "Nightmare Grimm Boss":
                    Grimms();
                    break;
                case "Jar Collector":
                    Collector();
                    break;
                case "HK Prime":
                    PV();
                    break;
                case "Oro":
                    NailMasters();
                    break;
                case "Dung Defender":
                    DungDefender();
                    break;
                case "Sly Boss":
                    Sly();
                    break;
                case "False Knight Dream":
                    FailedChampion();
                    break;
                case "False Knight New":
                    FalseKnight();
                    break;
                case "Absolute Radiance":
                    AbsoluteRadiance();
                    break;
            }
        }

        private void PV()
        {
            _control = gameObject.LocateMyFSM("Control");
            _control.Fsm.GetFsmInt("Half HP").Value = health.hp * 2 / 3; 
            //WHY IS THIS NAMED HALF HP???
            _control.Fsm.GetFsmInt("Quarter HP").Value = health.hp * 1 / 3; 
            //WHY IS THIS NAMED QUATER HP???
        }

        private void Collector()
        {
            _control = gameObject.LocateMyFSM("Phase Control");
            _control.Fsm.GetFsmInt("Phase 2 HP").Value = 350;
        }

        private void Grimms()
        {
            _control = gameObject.LocateMyFSM("Control");
            _control.Fsm.GetFsmInt("Rage HP 1").Value = health.hp * 3 / 4;
            _control.Fsm.GetFsmInt("Rage HP 2").Value = health.hp * 2 / 4;
            _control.Fsm.GetFsmInt("Rage HP 3").Value = health.hp * 1 / 4;
        }

        private void NailMasters()
        {
            _control = gameObject.LocateMyFSM("nailmaster");
            _control.Fsm.GetFsmInt("P2 HP").Value = 600;
        }

        private void DungDefender()
        {
            _control = gameObject.LocateMyFSM("Dung Defender");
            _control.Fsm.GetFsmInt("Rage HP").Value = 350;
        }

        private void Sly()
        {
            _control = gameObject.LocateMyFSM("Control");
            _control.Fsm.GetFsmInt("Ascended HP").Value = 250; 
            //WHY IS THIS NAMED ASCENDED HP IT LITERALLY MAKES NO SENSE!!
        }

        private void FailedChampion()
        {
            _control = gameObject.LocateMyFSM("FalseyControl");
            _control.Fsm.GetFsmInt("Recover HP").Value = 360;
        }

        private void FalseKnight()
        {
            _control = gameObject.LocateMyFSM("Check Health"); 
            //WHY DOES FK AND FC HAVE THEIR RECOVER HEALTH IN DIFFERENT FSMs
            _control.Fsm.GetFsmInt("Recover HP").Value = 260;
        }

        private void AbsoluteRadiance() {
            _attackCommands = gameObject.LocateMyFSM("Attack Commands");
            _control = gameObject.LocateMyFSM("Control");
            if (_attackCommands.GetState("CW Double").Actions.Length == 2) {
                _attackCommands.InsertAction("CW Double", new CallMethod {
                    behaviour = this,
                    methodName = "SwordBurstRepeatCheck",
                    parameters = new FsmVar[0],
                    everyFrame = false
                }, 0);
            }
            if (_attackCommands.GetState("CCW Double").Actions.Length == 2) {
                _attackCommands.InsertAction("CCW Double", new CallMethod {
                    behaviour = this,
                    methodName = "SwordBurstRepeatCheck",
                    parameters = new FsmVar[0],
                    everyFrame = false
                }, 0);
            }
            if (_control.GetState("Arena 1 Start").Actions.Length == 1) {
                _control.AddAction("Arena 1 Start", new CallMethod {
                    behaviour = this,
                    methodName = "ResetSwordBurstRepeats",
                    parameters = new FsmVar[0],
                    everyFrame = false
                });
            }
        }
        #endregion

        // Any Radiance mods manually add more sword burst waves
        // https://github.com/EarlyHemisphere/HollowKnight.AnyRadiance2-1.5/blob/main/Radiance.cs#L460
        // This function is strategically inserted right at the start of the repeat check state to
        // override the any radiance internal counter with our own counter that takes into account the possibility
        // of resetting plats phase in the middle of a sword burst attack (BossNerf L372)
        public void SwordBurstRepeatCheck() {
            _attackCommands = gameObject.LocateMyFSM("Attack Commands");
            if (FindBoss.swordBurstRepeats == 0) {
                _attackCommands.FsmVariables.GetFsmBool("Repeated").Value = true;
                FindBoss.swordBurstRepeats =  4;
            } else {
                _attackCommands.FsmVariables.GetFsmBool("Repeated").Value = false;
                FindBoss.swordBurstRepeats -= 1;
            }
        }

        public void ResetSwordBurstRepeats() {
            FindBoss.swordBurstRepeats = 4;
        }
    }
}
