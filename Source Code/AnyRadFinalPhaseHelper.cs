using UnityEngine;
using SFCore.Utils;
using HutongGames.PlayMaker.Actions;

namespace Easier_Pantheon_Practice {
    public class AnyRadFinalPhaseHelper : MonoBehaviour {
        private HealthManager _hm;
        private PlayMakerFSM _phaseControl;
        private PlayMakerFSM _attackCommands;
        private PlayMakerFSM _attackChoices;
        public bool onePlatSet = false;
        public bool isAnyRad2 = false;

        private void Awake() {
            _hm = base.gameObject.GetComponent<HealthManager>();
            _phaseControl = base.gameObject.LocateMyFSM("Phase Control");
            _attackCommands = base.gameObject.LocateMyFSM("Attack Commands");
            _attackChoices = base.gameObject.LocateMyFSM("Attack Choices");
        }

        public void Update() {
            if (_hm.hp < _phaseControl.FsmVariables.GetFsmInt("P5 Acend").Value) {
                if (isAnyRad2 && !onePlatSet) {
                    onePlatSet = true;
                    _attackCommands.GetAction<Wait>("FinalOrb Pause", 0).time.Value = 0.25f;
                }
            } else if (_hm.hp < _phaseControl.FsmVariables.GetFsmInt("P5 Acend").Value - 500) {
                if (!onePlatSet && !isAnyRad2) {
                    onePlatSet = true;
                    _attackCommands.GetAction<Wait>("Orb Antic", 0).time = 0.01f;
                    _attackCommands.GetAction<SetIntValue>("Orb Antic", 1).intValue = 5;
                    _attackCommands.GetAction<RandomInt>("Orb Antic", 2).min = 4;
                    _attackCommands.GetAction<RandomInt>("Orb Antic", 2).max = 6;
                    _attackCommands.GetAction<Wait>("Orb Summon", 2).time = 0.01f;
                    _attackCommands.GetAction<Wait>("Orb Pause", 0).time = 0.01f;
                    _attackChoices.GetAction<Wait>("Orb Recover", 0).time = 0.1f;
                }
            }
        }
    }
}