using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using World;

namespace Tutorial
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private TutorialData[] tutorialDatas = new TutorialData[0];
        [SerializeField] private TutorialArrow arrow;

        private Player _player;
        private TutorialData _currentTutorial;
        private bool _inProcess = false;

        public void Init(GameController gameController, Player p)
        {
            _player = p;
            gameController.OnStartGameEvent += OnStartGame;

            for (int i = 0; i < tutorialDatas.Length; i++)
            {
                var tutorialData = tutorialDatas[i];

                if (tutorialData.Condition == TutorialShowCondition.ByFarm)
                {
                    Action unsubscribe = null;
                    unsubscribe = () => tutorialData.FarmSource.OnCompleteFarmEvent -= StartTutorialAction;
                    tutorialData.FarmSource.OnCompleteFarmEvent += StartTutorialAction;
                    
                    void StartTutorialAction()
                    {
                        StartTutorial(tutorialData);
                        unsubscribe?.Invoke();
                    }
                }
            }
        }
        
        private void OnStartGame()
        {
            // check start tutorial
            var tutor = Array.Find(tutorialDatas, info => info.Condition == TutorialShowCondition.ByStart);
            if (tutor == null) return;

            StartTutorial(tutor);
        }

        private void UpdateInCurrentTutorial()
        {
            arrow.PointTo(_player.transform, _currentTutorial.GoalObject.transform);
        }

        private void StartTutorial(TutorialData tutorial)
        {
            if (_inProcess) return;
            
            _inProcess = true;
            
            _currentTutorial = tutorial;
            tutorial.GoalObject.OnEnterEvent += StopTutorial;
            
            arrow.gameObject.SetActive(true);
            arrow.StartAnim();
            UpdateInCurrentTutorial();
            
            _player.OnMoveEvent += UpdateInCurrentTutorial;
        }
        
        private void StopTutorial()
        {
            _currentTutorial.GoalObject.OnEnterEvent -= StopTutorial;
            
            _inProcess = false;
            
            arrow.gameObject.SetActive(false);
            arrow.StopAnim();
            _currentTutorial = null;
            
            _player.OnMoveEvent -= UpdateInCurrentTutorial;
        }
    }

    [Serializable]
    public class TutorialData
    {
        [SerializeField] private GameWorldCiObject goalObject;
        [SerializeField] private TutorialShowCondition condition;

        [ShowIf("@condition == TutorialShowCondition.ByFarm")] [SerializeField]
        private ResourceSource farmSource;
        
        public GameWorldCiObject GoalObject => goalObject;
        public TutorialShowCondition Condition => condition;
        public ResourceSource FarmSource => farmSource;
    }

    public enum TutorialShowCondition
    {
        Never,
        ByStart,
        ByFarm
    }
}