using Modding;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using HutongGames.PlayMaker;
using UObject = UnityEngine.Object;
using HutongGames.PlayMaker.Actions;
using System.Reflection;
using ModCommon.Util;
using static UnityEngine.ParticleSystem;

namespace Easier_Pantheon_Practice
{
    public class FindBoss : MonoBehaviour
    {
        private const int x_value = 0, y_value = 1;             //to make the move position function more readable
        private static int current_move;
        public static bool altered, SOB;                        //to allow some functions to be readable
        private static bool loop;
        private static string PreviousScene, SceneToLoad;
        private string MapZone;
        public static string CurrentBoss, CurrentBoss_1;
        private static Vector3 OldPosition, PosToMove;
        public static int swordBurstRepeats = 4;

        private readonly Dictionary<int, List<float>> MoveAround = new Dictionary<int, List<float>>
        {
            { 0 , new List<float>{11f,36.4f } },//bench
            { 1 , new List<float>{207f,6.4f } },//gpz
        };

        
        //Dict for bosses that have only 1 boss in them
        private static readonly Dictionary<string, string> _BossSceneName = new Dictionary<string, string>()
        {
            {"GG_Gruz_Mother_V", "Giant Fly"},
            {"GG_False_Knight","False Knight New" },
            {"GG_Mega_Moss_Charger", "Mega Moss Charger"},
            {"GG_Hornet_1", "Hornet Boss 1"},
            {"GG_Ghost_Gorb_V", "Ghost Warrior Slug"},
            {"GG_Dung_Defender", "Dung Defender"},
            {"GG_Mage_Knight_V", "Mage Knight"},
            {"GG_Brooding_Mawlek_V", "Mawlek Body"},
            {"GG_Ghost_Xero_V", "Ghost Warrior Xero"},
            {"GG_Crystal_Guardian", "Mega Zombie Beam Miner (1)"},
            {"GG_Ghost_Marmu_V", "Ghost Warrior Marmu"},
            {"GG_Flukemarm", "Fluke Mother"},
            {"GG_Broken_Vessel", "Infected Knight"},
            {"GG_Ghost_Galien", "Ghost Warrior Galien"},
            {"GG_Painter", "Sheo Boss"},
            {"GG_Hive_Knight", "Hive Knight"},
            {"GG_Ghost_Hu", "Ghost Warrior Hu"},
            {"GG_Collector_V", "Jar Collector"},
            {"GG_Grimm", "Grimm Boss"},
            {"GG_Uumuu_V", "Mega Jellyfish GG"},
            {"GG_Nosk_Hornet", "Hornet Nosk"},
            {"GG_Sly", "Sly Boss" },
            {"GG_Hornet_2", "Hornet Boss 2"},
            {"GG_Crystal_Guardian_2", "Zombie Beam Miner Rematch"},
            {"GG_Lost_Kin", "Lost Kin"},
            {"GG_Ghost_No_Eyes_V", "Ghost Warrior No Eyes"},
            {"GG_Traitor_Lord", "Mantis Traitor Lord"},
            {"GG_White_Defender", "White Defender"},
            {"GG_Ghost_Markoth_V", "Ghost Warrior Markoth"},
            {"GG_Grey_Prince_Zote", "Grey Prince"},
            {"GG_Failed_Champion", "False Knight Dream"},
            {"GG_Grimm_Nightmare", "Nightmare Grimm Boss"},
            {"GG_Hollow_Knight", "HK Prime"},
            {"GG_Radiance", "Absolute Radiance"},
            {"GG_Nosk", "Nosk"},
            {"GG_Nosk_V", "Nosk"},
            {"GG_Vengefly","Giant Buzzer Col"},
            {"GG_Gruz_Mother", "Giant Fly"},
            {"GG_Ghost_Gorb", "Ghost Warrior Slug"},
            {"GG_Mage_Knight", "Mage Knight"},
            {"GG_Brooding_Mawlek", "Mawlek Body"},
            {"GG_Ghost_Xero", "Ghost Warrior Xero"},
            {"GG_Ghost_Marmu", "Ghost Warrior Marmu"},
            {"GG_Collector", "Jar Collector"},
            {"GG_Uumuu", "Mega Jellyfish GG"},
            {"GG_Ghost_No_Eyes", "Ghost Warrior No Eyes"},
            {"GG_Ghost_Markoth", "Ghost Warrior Markoth"},
        };

        
        //Dict for bosses that have only 2 boss in them
        private static readonly Dictionary<string, List<string>> SemiExceptions_BossSceneName =
            new Dictionary<string, List<string>>()
            {
                {"GG_Vengefly_V", new List<string>() {"Giant Buzzer Col", "Giant Buzzer Col (1)"}},
                {"GG_Soul_Master", new List<string>() {"Mage Lord", "Mage Lord Phase2"}},
                {"GG_Oblobbles", new List<string>() {"Mega Fat Bee", "Mega Fat Bee (1)"}},
                {"GG_Soul_Tyrant", new List<string>() {"Dream Mage Lord", "Dream Mage Lord Phase2"}},
                {"GG_Nailmasters", new List<string>() {"Oro", "Mato"}},
                {"GG_God_Tamer", new List<string>() {"Lancer", "Lobster"}},
            };
        
        //Dict for bosses that have more than 2 boss in them
        private static readonly List<string> Exceptions_BossSceneName = new List<string>()
        {
            "GG_Mantis_Lords_V",
            "GG_Watcher_Knights",
            "GG_Mantis_Lords",
        };

        public void Awake()
        {
            TryKeys(); 
        }

        private void Start()
        {
            ModHooks.Instance.BeforeSceneLoadHook += BeforeSceneChange;
            USceneManager.sceneLoaded += SceneManager_sceneLoaded;
            On.BossSceneController.DoDreamReturn += DoDreamReturn;
            ModHooks.Instance.HeroUpdateHook += HotKeys;
            ModHooks.Instance.TakeHealthHook += Only1Damage;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            StartCoroutine(SceneLoaded());
            if (!loop)
            {
                if (PreviousScene != "GG_Workshop") return;
            }
            
            altered = false;
            SOB = true;
            CurrentBoss = CurrentBoss_1 = ""; 

            Modding.ReflectionHelper.GetField(typeof(BossSequenceController), "bossIndex", false).SetValue(null, 1);
            
            if (DoesDictContain(arg0.name))
            {
                StartCoroutine(ApplySettings());
                MapZone = GameManager.instance.GetCurrentMapZone();
                if (Exceptions_BossSceneName.Contains(arg0.name))
                {
                    switch (arg0.name)
                    {
                        case "GG_Mantis_Lords_V":
                            StartCoroutine(SistersOfBattle(true));
                            break;
                        case "GG_Mantis_Lords":
                            StartCoroutine(SistersOfBattle(false));
                            break;
                        case "GG_Watcher_Knights":
                            StartCoroutine(WatcherKnight());
                            break;
                    }
                }

                else
                {
                    if (_BossSceneName.ContainsKey(arg0.name)) CurrentBoss = _BossSceneName[arg0.name];
                    else
                    {
                        CurrentBoss = SemiExceptions_BossSceneName[arg0.name][0];
                        CurrentBoss_1 = SemiExceptions_BossSceneName[arg0.name][1];
                    }
                    p5Boss();
                }
            }
        }

        #region Find and alter bosses
        
        private IEnumerator ChangeBoss(string BossName,bool wait = true)
        {
            //Thank you to redfrog for this non-cursed code (before it was a while loop which didnt make sense)
            if (wait) yield return new WaitUntil(() => GameObject.Find(BossName));
            GameObject.Find(BossName).AddComponent<BossNerf>();
        }
        
        private void p5Boss()
        {
            StartCoroutine(ChangeBoss(CurrentBoss));

            if (CurrentBoss_1 != "") StartCoroutine(ChangeBoss(CurrentBoss_1));
        }
        
        private IEnumerator SistersOfBattle(bool isSOB)
        {
            SOB = isSOB;
            CurrentBoss = "Mantis Lord";
            CurrentBoss_1 = "Mantis Lord S";

            StartCoroutine(ChangeBoss(CurrentBoss));
            
            yield return new WaitUntil(() => GameObject.Find(CurrentBoss_1 + "1")); //Waits for phase 2
            
            for (int i = 1; i <= (SOB ? 3 : 2); i++)
            {
                StartCoroutine(ChangeBoss(CurrentBoss_1 + i.ToString(),false));
            }
        }
        
        private IEnumerator WatcherKnight()
        {
            CurrentBoss = CurrentBoss_1 = "Black Knight ";

            yield return new WaitUntil(() => GameObject.Find(CurrentBoss + "1"));
            for (int i = 1; i <= 6; i++)
            {
                StartCoroutine(ChangeBoss(CurrentBoss + i.ToString(),false));
            }
        }

        #endregion

        #region Setup Player
        private IEnumerator ApplySettings()
        {
            //waiting
            yield return new WaitForFinishedEnteringScene();
            
            var instance = EasierPantheonPractice.Instance;
            var settings = instance.settings;
            var BSC = BossSceneController.Instance;
            var HC = HeroController.instance;

            
            //remove health and add lifeblood
            if (!(settings.hitless_practice||BSC.BossLevel == 2))//checks for hitless practice or radiant fights
            {
                HC.TakeHealth(BSC.BossLevel == 0 ? settings.remove_health : 2 * settings.remove_health);

                for (int lifeblood_increment = 0; lifeblood_increment < settings.lifeblood; lifeblood_increment++)
                    EventRegister.SendEvent("ADD BLUE HEALTH");
            }

            //adds soul
            HC.AddMPCharge(settings.soul);
            
            //makes sure the HUD updates
            yield return null;
            HeroController.instance.AddMPCharge(1);
            HeroController.instance.AddMPCharge(-1);
        }
        private static int Only1Damage(int damage)
        {
            if (!DoesDictContain(GameManager.instance.GetSceneNameString())) return damage;

            if (EasierPantheonPractice.Instance.settings.hitless_practice) damage = 1000;
            bool isPlayerDead = damage >= PlayerData.instance.GetInt("health");

            if (EasierPantheonPractice.Instance.settings.infinite_anyrad2_plats_practice
                && isPlayerDead
                && GameManager.instance.sceneName == "GG_Radiance"
                ) {
                if (GameObject.Find("Phase2 Detector")) {
                    GameManager.instance.gameObject.GetComponent<FindBoss>().StartCoroutine(ResetPlatsPhase());
                    return 0;
                }
            }

            if (EasierPantheonPractice.Instance.settings.reload_boss_on_death && isPlayerDead) {
                LoadBossInLoop();
            }

            return damage;
        }

        private static IEnumerator ResetPlatsPhase() {
            GameObject absRad = GameObject.Find("Absolute Radiance");
            if (absRad) {
                HeroController HC = HeroController.instance;
                GameManager GM = GameManager.instance;
                PlayMakerFSM absRadControlFSM = absRad.LocateMyFSM("Control");
                PlayMakerFSM absRadAttackCommandsFSM = absRad.LocateMyFSM("Attack Commands");
                PlayMakerFSM absRadAttackChoicesFSM = absRad.LocateMyFSM("Attack Choices");
                PlayMakerFSM absRadPhaseControlFSM = absRad.LocateMyFSM("Phase Control");
                GameObject phase2Detector = GameObject.Find("Phase2 Detector");
                GameObject stunEyeGlow = absRadControlFSM.GetAction<ActivateGameObject>("Stun1 Out", 5).gameObject.GameObject.Value;

                absRadControlFSM.FsmVariables.GetFsmBool("Ascend Ready").Value = false;
                absRadControlFSM.FsmVariables.GetFsmGameObject("CamLock A1").Value.SetActive(true);
                absRadControlFSM.FsmVariables.GetFsmGameObject("CamLock Main").Value.SetActive(true);

                absRadAttackChoicesFSM.SendEvent("ARENA 1 END");
                absRadAttackChoicesFSM.GetAction<Wait>("Orb Recover", 0).time.Value = 0.5f;

                absRadAttackCommandsFSM.SetState("EB Glow End");
                absRadAttackCommandsFSM.FsmVariables.GetFsmBool("Final Orbs").Value = false;
                absRadAttackCommandsFSM.FsmVariables.GetFsmInt("Spawns").Value = absRadAttackCommandsFSM.GetAction<SetIntValue>("Nail Fan", 4).intValue.Value;
                EmissionModule shotChargeEmission = absRadAttackCommandsFSM.FsmVariables.GetFsmGameObject("Shot Charge").Value.GetComponent<ParticleSystem>().emission;
                EmissionModule shotCharge2Emission = absRadAttackCommandsFSM.FsmVariables.GetFsmGameObject("Shot Charge 2").Value.GetComponent<ParticleSystem>().emission;
                shotChargeEmission.enabled = false;
                shotCharge2Emission.enabled = false;
                if (absRadAttackCommandsFSM.GetState("AB Start").Actions.Length == 2) {
                    FsmOwnerDefault ownerDefault = new FsmOwnerDefault();
                    ownerDefault.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
                    ownerDefault.GameObject.Value = absRadAttackCommandsFSM.FsmVariables.GetFsmGameObject("Ascend Beam").Value;
    
                    absRadAttackCommandsFSM.InsertAction("AB Start", new ActivateGameObject {
                        gameObject = ownerDefault,
                        activate = true,
                        recursive = false,
                        resetOnExit = false,
                        everyFrame = false
                    }, 0);
                }

                absRadControlFSM.GetAction<ActivateGameObject>("Arena 2 Start", 3).gameObject.GameObject.Value.SetActive(true);

                HC.ClearMPSendEvents();
                HC.enterWithoutInput = true;
                HC.AcceptInput();
                HC.SetHazardRespawn(new Vector3(60.1f, 22.3f, 0), true);
                HC.MaxHealth();
                HC.SetMPCharge(0);

                GM.TimePasses();
                GM.ResetSemiPersistentItems();

                phase2Detector?.LocateMyFSM("Detect").SetState("State 1");
                phase2Detector?.SetActive(false);

                stunEyeGlow?.SetActive(true);
                stunEyeGlow?.LocateMyFSM("FSM").SetState("Init");

                iTween.Stop(GameObject.Find("Abyss Pit"));
                GameObject.Find("Abyss Pit").transform.position = new Vector3(61.77f, 18.7f, 0);
                GameObject.Find("Knight").transform.position = new Vector3(60.1f, 22.3f, 0);
                GameObject.Find("Blocker Shield")?.LocateMyFSM("Control").SendEvent("HERO REVIVED");
                GameObject.Find("CamLock A2")?.SetActive(false);
                GameObject.Find("Tendril1")?.LocateMyFSM("Audio").SetState("State 1");
                GameObject.Find("Tendril4")?.LocateMyFSM("Audio").SetState("State 1");
                GameObject.Find("Tendril5")?.LocateMyFSM("Audio").SetState("State 1");
                GameObject.Find("P2 SetA")?.transform.GetComponentsInChildren<Transform>().ToList().ForEach(t => {
                    if (t.gameObject != null && t.gameObject.name.Contains("Radiant Plat") && t.gameObject.LocateMyFSM("radiant_plat") != null) {
                        t.gameObject.LocateMyFSM("radiant_plat").SendEvent("DISAPPEAR");
                        t.gameObject.GetComponent<SpriteFlash>().CancelFlash();
                    }
                });
                GameObject.Find("Hazard Plat")?.transform.GetComponentsInChildren<Transform>().ToList().ForEach(t => {
                    if (t.gameObject != null && t.gameObject.name.Contains("Radiant Plat") && t.gameObject.LocateMyFSM("radiant_plat") != null) {
                        t.gameObject.LocateMyFSM("radiant_plat").SendEvent("DISAPPEAR");
                        t.gameObject.GetComponent<SpriteFlash>().CancelFlash();
                    }
                });
                GameObject.Find("Ascend Set")?.transform.GetComponentsInChildren<Transform>().ToList().ForEach(t => {
                    if (t.gameObject != null && t.gameObject.name.Contains("Radiant Plat") && t.gameObject.LocateMyFSM("radiant_plat") != null) {
                        t.gameObject.LocateMyFSM("radiant_plat").SendEvent("DISAPPEAR");
                        t.gameObject.GetComponent<SpriteFlash>().CancelFlash();
                    }
                });
                GameObject.Find("CamLocks Ascend")?.SetActive(false);
                GameObject.Find("CamLock Final")?.SetActive(false);
                GameObject.Find("Shot Charge").GetComponent("ParticleSystem");
                
                foreach (GameObject trigger in FindObjectsOfType<GameObject>().Where(go => go.name.Contains("Hazard Respawn Trigger v2") && go.transform.position.y >= 50f)) {
                    HazardRespawnTrigger hazardRespawnTrigger = trigger.GetComponent<HazardRespawnTrigger>();
                    hazardRespawnTrigger.GetType().GetField("inactive", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(hazardRespawnTrigger, false);
                }
                GameObject.Find("Ascend Respawns")?.SetActive(false);

                yield return new WaitUntil(() => absRad.LocateMyFSM("Teleport").ActiveStateName == "Idle");

                absRad.transform.position = new Vector3(60.63f, 28.3f, 0.006f);
                absRadControlFSM.SendEvent("STUN 1");
                absRad.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                absRad.GetComponent<Rigidbody2D>().angularVelocity = 0;
                GameObject.Find("Legs")?.SetActive(false);

                swordBurstRepeats = 4;
                
                yield return new WaitForSeconds(5);

                GameObject.Find("P2 SetA")?.transform.GetComponentsInChildren<Transform>().ToList().ForEach(t => {
                    if (t.gameObject != null && t.gameObject.name.Contains("Radiant Plat") && t.gameObject.LocateMyFSM("radiant_plat") != null) {
                        t.gameObject.GetComponent<SpriteFlash>().CancelFlash();
                    }
                });
                GameObject.Find("Hazard Plat")?.transform.GetComponentsInChildren<Transform>().ToList().ForEach(t => {
                    if (t.gameObject != null && t.gameObject.name.Contains("Radiant Plat") && t.gameObject.LocateMyFSM("radiant_plat") != null) {
                        t.gameObject.GetComponent<SpriteFlash>().CancelFlash();
                    }
                });
                GameObject.Find("Ascend Set")?.transform.GetComponentsInChildren<Transform>().ToList().ForEach(t => {
                    if (t.gameObject != null && t.gameObject.name.Contains("Radiant Plat") && t.gameObject.LocateMyFSM("radiant_plat") != null) {
                        t.gameObject.GetComponent<SpriteFlash>().CancelFlash();
                    }
                });
                foreach (GameObject spike in FindObjectsOfType<GameObject>().Where(go => go.name.Contains("Radiant Spike(Clone)") && go.transform.position.y >= 50f)) {
                    spike.LocateMyFSM("Control").SendEvent("DOWN");
                }
                GameObject.Find("Radiant Plat Small (10)").LocateMyFSM("radiant_plat").SendEvent("APPEAR");

                absRad.GetComponent<HealthManager>().hp = absRad.LocateMyFSM("Phase Control").FsmVariables.GetFsmInt("P4 Stun1").Value;
                absRad.LocateMyFSM("Phase Control").SetState("Check 4");
            }
        }

        #endregion


        #region hotkeys

        public void HotKeys()
        {
            var settings = EasierPantheonPractice.Instance.settings;
            var HC = HeroController.instance;
            
            
            string theCurrentScene = GameManager.instance.GetSceneNameString();

            if (settings.Key_return_to_hog != "")
            {
                if (Input.GetKeyDown(settings.Key_return_to_hog))
                {
                    if (HC.acceptingInput)
                    {
                        if (loop||(DoesDictContain(theCurrentScene) && PreviousScene == "GG_Workshop"))
                        {
                            StartCoroutine(LoadWorkshop());
                        }
                    }
                }
            }

            if (settings.Key_teleport_around_HoG != "")
            {
                if (Input.GetKeyDown(settings.Key_teleport_around_HoG))
                {
                    if (theCurrentScene == "GG_Workshop")
                    {
                        current_move++;
                        PosToMove.Set(MoveAround[current_move % 2][x_value], MoveAround[current_move % 2][y_value], 0f);
                        HC.transform.position = PosToMove;
                    }
                }
            }

            if (settings.Key_Reload_Boss != "")
            {
                if (Input.GetKeyUp(settings.Key_Reload_Boss))
                {
                    if (HC.acceptingInput)
                    {
                        if (settings.infinite_anyrad2_plats_practice && GameObject.Find("Phase2 Detector")) {
                            StartCoroutine(ResetPlatsPhase());
                        }
                        else if (loop || (DoesDictContain(theCurrentScene) && PreviousScene == "GG_Workshop"))
                        {
                            LoadBossInLoop();
                        }
                    }
                }
            }
        }

        public static void LoadBossScene()
        {
            var HC = HeroController.instance;
            var GM = GameManager.instance;
            GameObject Inspect = EasierPantheonPractice.PreloadedObjects["Inspect"];
            
            //Copy paste of the FSM that loads a boss from HoG
            PlayerData.instance.dreamReturnScene = "GG_Workshop";
            PlayMakerFSM.BroadcastEvent("BOX DOWN DREAM");
            PlayMakerFSM.BroadcastEvent("CONVO CANCEL");
            var Transition = Inspect.LocateMyFSM("GG Boss UI").GetAction<CreateObject>("Transition", 0).gameObject;

            foreach (var FSMObject in Transition.Value.GetComponentsInChildren<PlayMakerFSM>())
            {
                FSMObject.SendEvent("GG TRANSITION OUT");
            }

            HC.ClearMPSendEvents();
            GM.TimePasses();
            GM.ResetSemiPersistentItems();
            HC.enterWithoutInput = true;
            HC.AcceptInput();
            
            GM.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = SceneToLoad,
                EntryGateName = "door_dreamEnter",
                EntryDelay = 0,
                Visualization = GameManager.SceneLoadVisualizations.GodsAndGlory,
                PreventCameraFadeOut = true
            });
            GameManager.instance.gameObject.GetComponent<FindBoss>().StartCoroutine(FixSoul(BossSceneController.Instance.BossLevel));
        }

        private static IEnumerator FixSoul(int bossLevel)
        {
            yield return new WaitForFinishedEnteringScene();
            yield return null;
            yield return new WaitForSeconds(1f);//this line differenciates this function from ApplySettings
            HeroController.instance.AddMPCharge(1);
            HeroController.instance.AddMPCharge(-1);
            BossSceneController.Instance.BossLevel = bossLevel;
        }

        private IEnumerator LoadWorkshop()
        {
            loop = false;
            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                IsFirstLevelForPlayer = false,
                SceneName = "GG_Workshop",
                HeroLeaveDirection = GlobalEnums.GatePosition.unknown,
                EntryGateName = "left1",
                PreventCameraFadeOut = false,
                WaitForSceneTransitionCameraFade = true,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = false
            });

            yield return new WaitForFinishedEnteringScene();
            yield return new WaitForSceneLoadFinish();
            yield return new WaitUntil(() => HeroController.instance.acceptingInput);
            HeroController.instance.transform.position = OldPosition;
            //var HC = HeroController.instance;
            //var pd = PlayerData.instance;
            //GameManager.instance.gameMap.GetComponent<GameMap>().SetDoorValues(OldPosition.x, OldPosition.y,"GG_WorkShop",MapZone);
            //pd.gMap_doorX =OldPosition.x;
           // pd.gMap_doorY = OldPosition.y;

            //HC.RelinquishControl();
            //HC.StopAnimationControl();
            //HC.enterWithoutInput = true;

            /*GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = "GG_Workshop",
                EntryDelay = 0,
                PreventCameraFadeOut = true,
                Visualization = GameManager.SceneLoadVisualizations.GodsAndGlory,
            });
            yield return new WaitForFinishedEnteringScene();
            HC.RegainControl();
            HC.StartAnimationControl();*/

        }
        
        #endregion

        public static void LoadBossInLoop() {
            SceneToLoad = GameManager.instance.GetSceneNameString();
            loop = true;
            swordBurstRepeats = 4;
            LoadBossScene();
        }
        private void OnDestroy()
        {
            ModHooks.Instance.BeforeSceneLoadHook -= BeforeSceneChange;
            USceneManager.sceneLoaded -= SceneManager_sceneLoaded;
            On.BossSceneController.DoDreamReturn -= DoDreamReturn;
            ModHooks.Instance.HeroUpdateHook -= HotKeys;
        }
            
        #region Misc Functions

        private void TryKeys()
        {
            // checks if all the binded keys in settings file is actually an input. if not then clears the bind
            var settings = EasierPantheonPractice.Instance.settings;
            
            try {Input.GetKeyDown(settings.Key_return_to_hog);}
            catch {settings.Key_return_to_hog = "";}
            try {Input.GetKeyDown(settings.Key_Reload_Boss);}
            catch {settings.Key_Reload_Boss = "";}
            try {Input.GetKeyDown(settings.Key_teleport_around_HoG);}
            catch {settings.Key_teleport_around_HoG = "";}
        }
        private string BeforeSceneChange(string sceneName)
        {
            PreviousScene = GameManager.instance.sceneName;
            if (PreviousScene == "GG_Workshop") OldPosition = HeroController.instance.transform.position;
            return sceneName;
        }

        private void DoDreamReturn(On.BossSceneController.orig_DoDreamReturn orig, BossSceneController self)
        {
            //this comes to play when the player dies or dreamgates
            loop = false;
            orig(self);
        }
        private static bool DoesDictContain(string KeyToSearch)
        {
            return Exceptions_BossSceneName.Contains(KeyToSearch) || _BossSceneName.ContainsKey(KeyToSearch) ||
                   SemiExceptions_BossSceneName.ContainsKey(KeyToSearch);
        }
        
        private IEnumerator SceneLoaded() {yield return null;}
        
        
        #endregion
            
    }
}
