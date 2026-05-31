using UnityEngine;
using UnityEngine.UI;

namespace SunShadow
{
    /// <summary>
    /// 12 小组答题板 — 大屏幕展示（预留，答题阶段启用）
    /// 3x4 网格，每组显示：组名 + 状态圆圈
    /// </summary>
    public class AnswerBoardManager : MonoBehaviour
    {
        [Header("答题板")]
        [SerializeField] private GameObject boardPanel;            // 整个答题板
        [SerializeField] private Text boardTitle;                  // "各小组答题情况"
        [SerializeField] private GroupSlotUI[] groupSlots;         // 12 个位置
        [SerializeField] private GroupAnswerData answerData;

        [Header("颜色")]
        public Color unansweredColor = new Color(0.6f, 0.6f, 0.6f);   // 灰
        public Color correctColor     = new Color(0.2f, 0.8f, 0.3f);   // 绿
        public Color wrongColor       = new Color(0.95f, 0.25f, 0.2f); // 红
        public Color answeringColor   = new Color(1f, 0.85f, 0.2f);    // 黄

        void Start()
        {
            if (answerData != null)
                answerData.Initialize();

            if (boardTitle != null)
                boardTitle.text = "各小组答题情况";

            ResetAllSlots();
        }

        /// <summary>重置所有位置为未作答</summary>
        public void ResetAllSlots()
        {
            for (int i = 0; i < groupSlots.Length; i++)
            {
                if (groupSlots[i] != null)
                    SetSlotVisual(groupSlots[i], i + 1, GroupAnswerData.AnswerState.Unanswered);
            }
        }

        /// <summary>设置某个小组的状态 — 答题阶段调用</summary>
        public void SetSlotState(int groupId, GroupAnswerData.AnswerState state)
        {
            int idx = groupId - 1;
            if (idx < 0 || idx >= groupSlots.Length) return;
            SetSlotVisual(groupSlots[idx], groupId, state);
        }

        private void SetSlotVisual(GroupSlotUI slot, int groupId, GroupAnswerData.AnswerState state)
        {
            Color c = state switch
            {
                GroupAnswerData.AnswerState.Correct    => correctColor,
                GroupAnswerData.AnswerState.Wrong      => wrongColor,
                GroupAnswerData.AnswerState.Answering  => answeringColor,
                _                                      => unansweredColor
            };

            if (slot.bgImage != null)
                slot.bgImage.color = c;

            if (slot.groupNameText != null)
                slot.groupNameText.text = slot.groupNames != null && groupId - 1 < slot.groupNames.Length
                    ? slot.groupNames[groupId - 1]
                    : $"第{groupId}组";

            if (slot.statusText != null)
            {
                slot.statusText.text = state switch
                {
                    GroupAnswerData.AnswerState.Correct    => "✓",
                    GroupAnswerData.AnswerState.Wrong      => "✗",
                    GroupAnswerData.AnswerState.Answering  => "…",
                    _                                      => ""
                };
            }
        }
    }

    /// <summary>
    /// 单个小组的 UI 元素
    /// 挂载在每个 GroupSlot Prefab 上
    /// </summary>
    [System.Serializable]
    public class GroupSlotUI : MonoBehaviour
    {
        public Image bgImage;
        public Text groupNameText;
        public Text statusText;
        public string[] groupNames = {
            "第一组","第二组","第三组","第四组",
            "第五组","第六组","第七组","第八组",
            "第九组","第十组","十一组","十二组"
        };
    }
}
