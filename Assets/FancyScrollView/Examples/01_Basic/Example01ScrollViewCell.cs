﻿using UnityEngine;
using UnityEngine.UI;

namespace FancyScrollView
{
    public class Example01ScrollViewCell : FancyScrollViewCell<Example01CellData>
    {
        [SerializeField] Animator animator;
        [SerializeField] Text message;

        static readonly int ScrollTriggerHash = Animator.StringToHash("scroll");

        public override void UpdateContent(Example01CellData cellData)
        {
            message.text = cellData.Message;
        }

        public override void UpdatePosition(float position)
        {
            currentPosition = position;
            animator.Play(ScrollTriggerHash, -1, position);
            animator.speed = 0;
        }

        // GameObject が非アクティブになると Animator がリセットされてしまうため
        // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
        float currentPosition = 0;

        void OnEnable() => UpdatePosition(currentPosition);
    }
}
