using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Tag.DB;
using Tag.Models;

namespace Tag
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 日付未選択フラグ
        /// </summary>
        private bool isDateUnSelect;

        /// <summary>
        /// 編集フラグ
        /// </summary>
        private bool isEdit;

        /// <summary>
        /// 予定日情報コピー用
        /// </summary>
        private ScheduleDayInfo copyScheduleDayInfo;

        /// <summary>
        /// コンストラクタ処理
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            DBAccessor dba = new DBAccessor();

            List<PriorityInfo> priorityInfoList = dba.GetPriorityInfoList();

            ScheduleDayPriorityCombo.ItemsSource = priorityInfoList;

            DateTime dateTime = ScheduleCalendar.SelectedDate ?? DateTime.Now;

            List<ScheduleDayInfo> scheduleDayInfoList = dba.GetScheduleDayInfoList(dateTime);

            ScheduleDayList.ItemsSource = scheduleDayInfoList;

            isDateUnSelect = true;
            isEdit = true;

            copyScheduleDayInfo = new ScheduleDayInfo();
        }

        /// <summary>
        /// 予定日のタイトル選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDayList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScheduleDayItemText.DataContext = (ScheduleDayInfo)ScheduleDayList.SelectedItem;
            ScheduleDayPriorityCombo.DataContext = (ScheduleDayInfo)ScheduleDayList.SelectedItem;
            ScheduleText.DataContext = (ScheduleDayInfo)ScheduleDayList.SelectedItem;

            isEdit = true;
        }

        /// <summary>
        /// 優先度選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDayPriorityCombo_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!isEdit)
            {
                return;
            }

            ScheduleDayInfo scheduleDayInfo = (ScheduleDayInfo)ScheduleDayList.SelectedItem;

            if (null != scheduleDayInfo)
            {
                scheduleDayInfo.Priority = ScheduleDayPriorityCombo.Text;
                scheduleDayInfo.UpdatedAt = DateTime.Now;

                DBAccessor dba = new DBAccessor();

                dba.UpdatePriorityOfScheduleDayInfo(scheduleDayInfo);
            }
        }

        /// <summary>
        /// タイトル値変更時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDayItemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            //ScheduleDayItemText.Text = ScheduleDayItemText.Text.Replace("マスク", "***");
        }

        /// <summary>
        /// タイトル値入力後移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDayItemText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!isEdit)
            {
                return;
            }

            DBAccessor dba = new DBAccessor();

            ScheduleDayInfo scheduleDayInfo = (ScheduleDayInfo)ScheduleDayList.SelectedItem;

            string title = "Not Title"; // TODO : 空で削除も可

            if (string.Empty != ScheduleDayItemText.Text)
            {
                title = ScheduleDayItemText.Text;
            }

            if (string.Empty != title)
            {
                if (isDateUnSelect && null == scheduleDayInfo)
                {
                    scheduleDayInfo = dba.GetScheduleDayInfo();
                }

                DateTime dateTime = DateTime.Now;

                if (null == scheduleDayInfo)
                {
                    scheduleDayInfo = new ScheduleDayInfo
                    {
                        ScheduleDate = ScheduleCalendar.SelectedDate ?? dateTime,
                        Title = title,
                        Contents = ScheduleText.Text,
                        CreatedAt = dateTime,
                        UpdatedAt = dateTime
                    };
                }
                else
                {
                    scheduleDayInfo.ScheduleDate = ScheduleCalendar.SelectedDate ?? dateTime;
                    scheduleDayInfo.Title = title;
                    scheduleDayInfo.Contents = ScheduleText.Text;
                    scheduleDayInfo.UpdatedAt = dateTime;
                }

                if (!IsScheduleDayInfoChange(scheduleDayInfo))
                {
                    return;
                }

                dba.DeleteInsertScheduleDayInfo(scheduleDayInfo);

                IEnumerable scheduleDayList = ScheduleDayList.ItemsSource;

                ScheduleDayInfo newData = dba.GetScheduleDayInfo();

                if (null != newData)
                {
                    foreach (ScheduleDayInfo oldData in scheduleDayList)
                    {
                        if (oldData.Id == newData.DelId)
                        {
                            oldData.Id = newData.Id;
                            oldData.ScheduleDate = newData.ScheduleDate;
                            oldData.Title = newData.Title;
                            oldData.Contents = newData.Contents;
                            oldData.DelId = newData.DelId;
                            oldData.DelFlg = newData.DelFlg;
                            oldData.CreatedAt = newData.CreatedAt;
                            oldData.UpdatedAt = newData.UpdatedAt;

                            break;
                        }
                    }
                }
            }
            else
            {
                if (null == scheduleDayInfo)
                {
                    return;
                }

                dba.DeleteScheduleDayInfo(scheduleDayInfo);

                IEnumerable<ScheduleDayInfo> scheduleDayList = ScheduleDayList.ItemsSource as IEnumerable<ScheduleDayInfo>;

                List<ScheduleDayInfo> delList = new List<ScheduleDayInfo>(scheduleDayList);

                delList.RemoveAt(ScheduleDayList.SelectedIndex);
            }

            ReloadScheduleCalendarData();
        }

        /// <summary>
        /// 内容値入力後移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!isEdit)
            {
                return;
            }

            DBAccessor dba = new DBAccessor();

            ScheduleDayInfo scheduleDayInfo = (ScheduleDayInfo)ScheduleDayList.SelectedItem;

            DateTime dateTime = DateTime.Now;

            string title = "Not Title";

            if (string.Empty != ScheduleDayItemText.Text)
            {
                title = ScheduleDayItemText.Text;
            }

            if (isDateUnSelect && null == scheduleDayInfo)
            {
                scheduleDayInfo = dba.GetScheduleDayInfo();
            }

            if (null == scheduleDayInfo)
            {
                scheduleDayInfo = new ScheduleDayInfo
                {
                    ScheduleDate = ScheduleCalendar.SelectedDate ?? dateTime,
                    Title = title,
                    Contents = ScheduleText.Text,
                    CreatedAt = dateTime,
                    UpdatedAt = dateTime
                };
            }
            else
            {
                scheduleDayInfo.ScheduleDate = ScheduleCalendar.SelectedDate ?? dateTime;
                scheduleDayInfo.Title = title;
                scheduleDayInfo.Contents = ScheduleText.Text;
                scheduleDayInfo.UpdatedAt = dateTime;
            }

            if (!IsScheduleDayInfoChange(scheduleDayInfo))
            {
                return;
            }

            dba.DeleteInsertScheduleDayInfo(scheduleDayInfo);

            IEnumerable scheduleDayList = ScheduleDayList.ItemsSource;

            ScheduleDayInfo newData = dba.GetScheduleDayInfo();

            if (null != newData)
            {
                foreach (ScheduleDayInfo oldData in scheduleDayList)
                {
                    if (oldData.Id == newData.DelId)
                    {
                        oldData.Id = newData.Id;
                        oldData.ScheduleDate = newData.ScheduleDate;
                        oldData.Title = newData.Title;
                        oldData.Contents = newData.Contents;
                        oldData.DelId = newData.DelId;
                        oldData.DelFlg = newData.DelFlg;
                        oldData.CreatedAt = newData.CreatedAt;
                        oldData.UpdatedAt = newData.UpdatedAt;

                        break;
                    }
                }
            }

            ReloadScheduleCalendarData();
        }

        /// <summary>
        /// カレンダー日付選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            ReloadScheduleCalendarData();

            ScheduleDayList.SelectedItem = null;
            ScheduleDayItemText.Text = string.Empty;
            ScheduleDayItemText.DataContext = null;
            ScheduleDayPriorityCombo.Text = string.Empty;
            ScheduleDayPriorityCombo.DataContext = null;
            ScheduleText.Text = string.Empty;
            ScheduleText.DataContext = null;

            isDateUnSelect = false;
        }

        /// <summary>
        /// 予定日情報追加選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            DBAccessor dba = new DBAccessor();

            DateTime dateTime = DateTime.Now;

            ScheduleDayInfo scheduleDayInfo = new ScheduleDayInfo
            {
                ScheduleDate = ScheduleCalendar.SelectedDate ?? dateTime,
                Title = "New",
                Contents = string.Empty,
                CreatedAt = dateTime,
                UpdatedAt = dateTime
            };

            dba.DeleteInsertScheduleDayInfo(scheduleDayInfo);

            ReloadScheduleCalendarData();
        }

        /// <summary>
        /// 予定日情報削除選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            ScheduleDayInfo scheduleDayInfo = (ScheduleDayInfo)ScheduleDayList.SelectedItem;

            if (null == scheduleDayInfo)
            {
                MessageBox.Show("Please Delete Select.", "message", MessageBoxButton.OK);
            }
            else
            {
                DBAccessor dba = new DBAccessor();

                dba.DeleteScheduleDayInfo(scheduleDayInfo);

                ReloadScheduleCalendarData();

                isDateUnSelect = false;
            }
        }

        /// <summary>
        /// 予定日コピー選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyItem_Click(object sender, RoutedEventArgs e)
        {
            ScheduleDayInfo scheduleDayInfo = (ScheduleDayInfo)ScheduleDayList.SelectedItem;

            if (null == scheduleDayInfo)
            {
                MessageBox.Show("Please Copy Select.", "message", MessageBoxButton.OK);
            }
            else
            {
                DateTime dateTime = DateTime.Now;

                copyScheduleDayInfo = new ScheduleDayInfo
                {
                    ScheduleDate = scheduleDayInfo.ScheduleDate,
                    Title = scheduleDayInfo.Title,
                    Contents = scheduleDayInfo.Contents,
                    CreatedAt = dateTime,
                    UpdatedAt = dateTime
                };
            }
        }

        /// <summary>
        /// 予定日タイトルクリップボードコピー選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDayItemCCopy_Click(object sender, RoutedEventArgs e)
        {
            string selectedText = ScheduleDayItemText.SelectedText;

            if (string.Empty == selectedText)
            {
                selectedText = ScheduleDayItemText.Text;
            }

            Clipboard.SetText(selectedText);
        }

        /// <summary>
        /// 予定日タイトルクリップボード貼り付け選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDayItemCPaste_Click(object sender, RoutedEventArgs e)
        {
            ScheduleDayItemText.Text = ScheduleDayItemText.Text.Substring(0, ScheduleDayItemText.SelectionStart)
                                     + Clipboard.GetText()
                                     + ScheduleDayItemText.Text.Substring(ScheduleDayItemText.SelectionStart + ScheduleDayItemText.SelectionLength);
        }

        /// <summary>
        /// 予定日タイトル貼り付け選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDayItemPaste_Click(object sender, RoutedEventArgs e)
        {
            ScheduleDayItemText.Text = copyScheduleDayInfo.Title;
        }

        /// <summary>
        /// 予定日タイトルクリア選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDayItemClear_Click(object sender, RoutedEventArgs e)
        {
            ScheduleDayItemText.Text = string.Empty;
        }

        /// <summary>
        /// 予定内容クリップボードコピー選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleCCopy_Click(object sender, RoutedEventArgs e)
        {
            string selectedText = ScheduleText.SelectedText;

            if (string.Empty == selectedText)
            {
                selectedText = ScheduleText.Text;
            }

            Clipboard.SetText(selectedText);
        }

        /// <summary>
        /// 予定内容クリップボード貼り付け選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleCPaste_Click(object sender, RoutedEventArgs e)
        {
            string insertText = ScheduleText.Text.Substring(0, ScheduleText.SelectionStart)
                              + Clipboard.GetText();

            ScheduleText.Text = insertText + ScheduleText.Text.Substring(ScheduleText.SelectionStart + ScheduleText.SelectionLength);

            ScheduleText.SelectionStart = insertText.Length;
        }

        /// <summary>
        /// 予定内容貼り付け選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SchedulePaste_Click(object sender, RoutedEventArgs e)
        {
            ScheduleText.Text = copyScheduleDayInfo.Contents;
        }

        /// <summary>
        /// 予定内容クリア選択処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleClear_Click(object sender, RoutedEventArgs e)
        {
            ScheduleText.Text = string.Empty;
        }

        /// <summary>
        /// 予定日データ再読み込み
        /// </summary>
        private void ReloadScheduleCalendarData()
        {
            ScheduleDayInfo scheduleDayInfo = (ScheduleDayInfo)ScheduleDayList.SelectedItem;

            DateTime dateTimeNow = DateTime.Now;

            int id = 0;

            if (null != scheduleDayInfo)
            {
                id = scheduleDayInfo.Id;
                dateTimeNow = scheduleDayInfo.ScheduleDate;
            }

            ScheduleDayList.SelectedItem = null;

            DBAccessor dba = new DBAccessor();

            DateTime dateTime = ScheduleCalendar.SelectedDate ?? dateTimeNow;

            List<ScheduleDayInfo> scheduleDayInfoList = dba.GetScheduleDayInfoList(dateTime);

            ScheduleDayList.ItemsSource = scheduleDayInfoList;

            ScheduleDayList.SelectedItem = GetScheduleDayInfo(scheduleDayInfoList, id);

            ScheduleDayItemText.DataContext = ScheduleDayList.SelectedItem;
            ScheduleDayPriorityCombo.DataContext = ScheduleDayList.SelectedItem;
            ScheduleText.DataContext = ScheduleDayList.SelectedItem;

            isDateUnSelect = true;
        }

        /// <summary>
        /// 予定日情報を返す
        /// </summary>
        /// <param name="scheduleDayInfoList">予定日情報リスト</param>
        /// <param name="id">ID</param>
        /// <returns>予定日情報</returns>
        private ScheduleDayInfo GetScheduleDayInfo(List<ScheduleDayInfo> scheduleDayInfoList, int id)
        {
            ScheduleDayInfo scheduleDayInfo = null;

            if (null == scheduleDayInfoList || 0 == scheduleDayInfoList.Count)
            {
                return scheduleDayInfo;
            }

            foreach (ScheduleDayInfo data in scheduleDayInfoList)
            {
                if (data.Id == id)
                {
                    scheduleDayInfo = data;

                    break;
                }
            }

            return scheduleDayInfo;
        }

        /// <summary>
        /// 予定日情報が変更されているか確認
        /// </summary>
        /// <param name="scheduleDayInfo">予定日情報</param>
        /// <returns>変更確認結果</returns>
        private bool IsScheduleDayInfoChange(ScheduleDayInfo scheduleDayInfo)
        {
            bool isChange = false;

            if (0 == scheduleDayInfo.Id)
            {
                return true;
            }

            DBAccessor dba = new DBAccessor();

            ScheduleDayInfo checkData = dba.GetScheduleDayInfo(scheduleDayInfo.Id);

            if (null == checkData)
            {
                return true;
            }

            if (scheduleDayInfo.Title != checkData.Title)
            {
                isChange = true;
            }

            if (scheduleDayInfo.Contents != checkData.Contents)
            {
                isChange = true;
            }

            return isChange;
        }
    }
}
