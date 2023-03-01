using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace World
{
    public class ResourceSource : MonoBehaviour
    {
        [SerializeField] private PlayerEntranceTrigger trigger;

        public void Init()
        {
            trigger.Init(OnEnter, OnExit);
        }

        private void OnEnter()
        {
            AnimateMine();
        }
        
        private void OnExit()
        {
            StopAnim();
        }

        #region Anim

        private int animLTID = -1;
        
        private void AnimateMine()
        {
            animLTID =
            LeanTween.scaleY(gameObject, 0.85f, 0.1f).setOnComplete(() =>
            {
                animLTID =
                LeanTween.scaleY(gameObject, 1f, 0.4f).id;
            }).id;
        }

        private void StopAnim()
        {
            transform.localScale = Vector3.one;
            LeanTween.cancel(animLTID);
        }

        #endregion
    }
}