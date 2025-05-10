using System.Collections.Generic;
using Tag.Models;

namespace Tag.ViewModels
{
    /// <summary>
    /// ビューモデル
    /// </summary>
    public class MainViewModel
    {
        /// <summary>
        /// 優先度情報リスト
        /// </summary>
        public List<PriorityInfo> PriorityInfoList { get; set; }

        /// <summary>
        /// 予定日情報リスト
        /// </summary>
        public List<ScheduleDayInfo> ScheduleDayInfoList { get; set; }
    }
}
