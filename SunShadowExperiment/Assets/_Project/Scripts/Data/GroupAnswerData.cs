using System;
using UnityEngine;

namespace SunShadow
{
    /// <summary>
    /// 小组答题数据 — ScriptableObject（答题阶段再启用）
    /// 预留完整的 12 组答题数据结构和接口
    /// </summary>
    [CreateAssetMenu(fileName = "GroupAnswerData", menuName = "太阳影子实验/小组答题数据")]
    public class GroupAnswerData : ScriptableObject
    {
        [System.Serializable]
        public struct GroupEntry
        {
            public int groupId;                // 1-12
            public string groupDisplayName;    // "第一组" / "第二组" ...
            public AnswerState state;
            public string submittedAnswer;     // 预留：学生提交的答案
            public float answerTimestamp;      // 预留：提交时间戳
        }

        public enum AnswerState
        {
            Unanswered,   // 未作答 — 灰色
            Correct,      // 正确 — 绿色
            Wrong,        // 错误 — 红色
            Answering     // 正在作答 — 黄色闪烁
        }

        [Tooltip("12 个小组的数据")]
        public GroupEntry[] groups = new GroupEntry[12];

        /// <summary>
        /// 初始化 12 组名字
        /// </summary>
        public void Initialize()
        {
            string[] names = {
                "第一组","第二组","第三组","第四组",
                "第五组","第六组","第七组","第八组",
                "第九组","第十组","十一组","十二组"
            };
            for (int i = 0; i < 12; i++)
            {
                groups[i].groupId = i + 1;
                groups[i].groupDisplayName = names[i];
                groups[i].state = AnswerState.Unanswered;
            }
        }

        // === 预留 API（答题阶段接入）===

        public void SubmitAnswer(int groupId, string answer)
        {
            int idx = groupId - 1;
            if (idx < 0 || idx >= 12) return;
            groups[idx].submittedAnswer = answer;
            groups[idx].answerTimestamp = Time.time;
            groups[idx].state = AnswerState.Answering;
            // 未来: 验证答案，设置 Correct/Wrong
        }

        public void MarkCorrect(int groupId)   { int i = groupId - 1; if (i >= 0 && i < 12) groups[i].state = AnswerState.Correct; }
        public void MarkWrong(int groupId)     { int i = groupId - 1; if (i >= 0 && i < 12) groups[i].state = AnswerState.Wrong; }

        public void ResetAllAnswers()
        {
            for (int i = 0; i < 12; i++)
            {
                groups[i].state = AnswerState.Unanswered;
                groups[i].submittedAnswer = "";
                groups[i].answerTimestamp = 0f;
            }
        }

        public int GetCorrectCount()
        {
            int c = 0;
            for (int i = 0; i < 12; i++)
                if (groups[i].state == AnswerState.Correct) c++;
            return c;
        }

        public int GetWrongCount()
        {
            int c = 0;
            for (int i = 0; i < 12; i++)
                if (groups[i].state == AnswerState.Wrong) c++;
            return c;
        }
    }
}
