using System;

namespace Tag.Models
{
    /// <summary>
    /// 予定日情報クラス
    /// </summary>
    public class ScheduleDayInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 予定日
        /// </summary>
        public DateTime ScheduleDate { get; set; }

        /// <summary>
        /// 優先度
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 予定日タイトル
        /// </summary>
        public string ScheduleDateTitle { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Contents { get; set; }

        /// <summary>
        /// 論理削除時ID
        /// </summary>
        public int DelId { get; set; }

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
