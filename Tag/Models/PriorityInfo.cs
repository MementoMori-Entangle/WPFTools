using System;

namespace Tag.Models
{
    /// <summary>
    /// 優先度情報
    /// </summary>
    public class PriorityInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 優先度
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public int DelFlg { get; set; }

        /// <summary>
        /// 作成日
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新日
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
