using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using Tag.Models;

namespace Tag.DB
{
    /// <summary>
    /// DBデータ操作
    /// </summary>
    public class DBAccessor
    {
        /// <summary>
        /// カラム
        /// </summary>
        public enum COLUMN
        {
            /// <summary>
            /// 更新日
            /// </summary>
            UPDATE_AT
        }

        /// <summary>
        /// 順序
        /// </summary>
        public enum ORDER
        {
            /// <summary>
            /// 昇順
            /// </summary>
            ASC,

            /// <summary>
            /// 降順
            /// </summary>
            DESC
        }

        /// <summary>
        /// データソースキー
        /// </summary>
        private const string DATA_SOURCE_KEY = "data_source";

        /// <summary>
        /// 優先度情報テーブル DML
        /// </summary>
        private const string DML_PRIORITY_INFO_SELECT = "SELECT id, priority, contents, del_flg, created_at, updated_at FROM priority_info";

        /// <summary>
        /// 予定日情報テーブル DML 未削除データ
        /// </summary>
        private const string DML_PRIORITY_INFO_SELECT_DEL = DML_PRIORITY_INFO_SELECT + " WHERE del_flg = 0";

        /// <summary>
        /// 予定日情報テーブル DML
        /// </summary>
        private const string DML_SCHEDULE_DAY_INFO_SELECT = "SELECT id, schedule_date, priority, title, contents, del_id, del_flg, created_at, updated_at FROM schedule_day_info";

        /// <summary>
        /// 予定日情報テーブル DML IDキー
        /// </summary>
        private const string DML_SCHEDULE_DAY_INFO_SELECT_ID = DML_SCHEDULE_DAY_INFO_SELECT + " WHERE id = @ID";

        /// <summary>
        /// 予定日情報テーブル DML 最新データ
        /// </summary>
        private const string DML_SCHEDULE_DAY_INFO_SELECT_MAX_ID = "SELECT s.id, s.schedule_date, s.priority, s.title, s.contents, s.del_id, s.del_flg, s.created_at, s.updated_at"
                                                                 + "  FROM schedule_day_info s INNER JOIN (SELECT MAX(id) as id FROM schedule_day_info WHERE del_flg = 0) m"
                                                                 + "    ON s.id = m.id";

        /// <summary>
        /// 予定日情報テーブル DML 未削除データ
        /// </summary>
        private const string DML_SCHEDULE_DAY_INFO_SELECT_DEL = DML_SCHEDULE_DAY_INFO_SELECT + " WHERE del_flg = 0";

        /// <summary>
        /// 更新日順
        /// </summary>
        private const string ADD_ORDER_UPDATE_AT = " order by updated_at";

        /// <summary>
        /// 昇順
        /// </summary>
        private const string ADD_ORDER_ASC = " asc";

        /// <summary>
        /// 降順
        /// </summary>
        private const string ADD_ORDER_DESC = " desc";

        /// <summary>
        /// データソース
        /// </summary>
        private readonly string dataSource;

        /// <summary>
        /// 接続コネクション文字列
        /// </summary>
        private readonly SQLiteConnectionStringBuilder sqlConnectionSB;

        /// <summary>
        /// 追加条件
        /// </summary>
        private string addConditions;

        /// <summary>
        /// コンストラクタ処理
        /// </summary>
        public DBAccessor()
        {
            dataSource = (string)Properties.Settings.Default[DATA_SOURCE_KEY];

            sqlConnectionSB = new SQLiteConnectionStringBuilder { DataSource = dataSource };

            Init();

            addConditions = string.Empty;

            AddConditions(COLUMN.UPDATE_AT, ORDER.DESC);
        }

        /// <summary>
        /// 条件を追加
        /// </summary>
        /// <param name="column">カラム</param>
        /// <param name="order">順序</param>
        public void AddConditions(COLUMN column, ORDER order)
        {
            switch (column)
            {
                case COLUMN.UPDATE_AT:
                    addConditions = ADD_ORDER_UPDATE_AT;
                    break;
            }

            switch (order)
            {
                case ORDER.ASC:
                    addConditions += ADD_ORDER_ASC;
                    break;
                case ORDER.DESC:
                    addConditions += ADD_ORDER_DESC;
                    break;
            }
        }

        /// <summary>
        /// 予定日情報リストを返す
        /// </summary>
        /// <param name="scheduleDate">予定日</param>
        /// <param name="delInclude">削除含むフラグ</param>
        /// <returns>予定日情報リスト</returns>
        public List<ScheduleDayInfo> GetScheduleDayInfoList(DateTime scheduleDate, bool delInclude = false)
        {
            List<ScheduleDayInfo> scheduleDayInfoList = new List<ScheduleDayInfo>();
            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    string query = null;

                    if (delInclude)
                    {
                        query = DML_SCHEDULE_DAY_INFO_SELECT + " WHERE schedule_date = @SCHEDULE_DATE";
                    }
                    else
                    {
                        query = DML_SCHEDULE_DAY_INFO_SELECT_DEL + " AND schedule_date = @SCHEDULE_DATE";
                    }

                    command.CommandText = query + addConditions;

                    command.Parameters.Add(new SQLiteParameter("@SCHEDULE_DATE", scheduleDate.ToString(Constant.DATE_FORMAT)));

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ScheduleDayInfo scheduleDayInfo = new ScheduleDayInfo
                            {
                                Id = int.Parse(reader["id"].ToString()),
                                ScheduleDate = DateTime.Parse(reader["schedule_date"].ToString()),
                                Priority = reader["priority"].ToString(),
                                Title = (string)reader["title"],
                                Contents = (string)reader["contents"],
                                DelId = int.Parse(reader["del_id"].ToString()),
                                DelFlg = int.Parse(reader["del_flg"].ToString()),
                                CreatedAt = DateTime.Parse(reader["created_at"].ToString()),
                                UpdatedAt = DateTime.Parse(reader["updated_at"].ToString())
                            };
                            scheduleDayInfo.ScheduleDateTitle = scheduleDayInfo.Title + " (" + scheduleDayInfo.UpdatedAt.ToString(Constant.DATE_TIME_FORMAT) + ")";

                            scheduleDayInfoList.Add(scheduleDayInfo);
                        }
                    }
                }
            }

            return scheduleDayInfoList;
        }

        /// <summary>
        /// 予定日情報を返す
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>予定日情報</returns>
        public ScheduleDayInfo GetScheduleDayInfo(int id = 0)
        {
            ScheduleDayInfo scheduleDayInfo = null;

            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    if (0 < id)
                    {
                        command.CommandText = DML_SCHEDULE_DAY_INFO_SELECT_ID;

                        command.Parameters.Add(new SQLiteParameter("@ID", id));
                    }
                    else
                    {
                        command.CommandText = DML_SCHEDULE_DAY_INFO_SELECT_MAX_ID;
                    }

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            scheduleDayInfo = new ScheduleDayInfo
                            {
                                Id = int.Parse(reader["id"].ToString()),
                                ScheduleDate = DateTime.Parse(reader["schedule_date"].ToString()),
                                Priority = reader["priority"].ToString(),
                                Title = (string)reader["title"],
                                Contents = (string)reader["contents"],
                                DelId = int.Parse(reader["del_id"].ToString()),
                                DelFlg = int.Parse(reader["del_flg"].ToString()),
                                CreatedAt = DateTime.Parse(reader["created_at"].ToString()),
                                UpdatedAt = DateTime.Parse(reader["updated_at"].ToString())
                            };

                            scheduleDayInfo.ScheduleDateTitle = scheduleDayInfo.Title + " (" + scheduleDayInfo.UpdatedAt.ToString(Constant.DATE_TIME_FORMAT) + ")";

                            break;
                        }
                    }
                }
            }

            return scheduleDayInfo;
        }

        /// <summary>
        /// 予定日情報リストを返す
        /// </summary>
        /// <param name="delInclude">削除含むフラグ</param>
        /// <returns>予定日情報リスト</returns>
        public List<ScheduleDayInfo> GetScheduleDayInfoList(bool delInclude = false)
        {
            List <ScheduleDayInfo> scheduleDayInfoList = new List<ScheduleDayInfo>();
            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    if (delInclude)
                    {
                        command.CommandText = DML_SCHEDULE_DAY_INFO_SELECT;
                    }
                    else
                    {
                        command.CommandText = DML_SCHEDULE_DAY_INFO_SELECT_DEL;
                    }

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ScheduleDayInfo scheduleDayInfo = new ScheduleDayInfo
                            {
                                Id = int.Parse(reader["id"].ToString()),
                                ScheduleDate = DateTime.Parse(reader["schedule_date"].ToString()),
                                Priority = reader["priority"].ToString(),
                                Title = (string)reader["title"],
                                Contents = (string)reader["contents"],
                                DelId = int.Parse(reader["del_id"].ToString()),
                                DelFlg = int.Parse(reader["del_flg"].ToString()),
                                CreatedAt = DateTime.Parse(reader["created_at"].ToString()),
                                UpdatedAt = DateTime.Parse(reader["updated_at"].ToString())
                            };
                            scheduleDayInfo.ScheduleDateTitle = scheduleDayInfo.Title + " (" + scheduleDayInfo.UpdatedAt.ToString(Constant.DATE_TIME_FORMAT) + ")";

                            scheduleDayInfoList.Add(scheduleDayInfo);
                        }
                    }
                }
            }

            return scheduleDayInfoList;
        }

        /// <summary>
        /// 予定日情報を削除
        /// </summary>
        /// <param name="scheduleDayInfo">予定日情報</param>
        /// <returns>削除結果</returns>
        public bool DeleteScheduleDayInfo(ScheduleDayInfo scheduleDayInfo)
        {
            if (null == scheduleDayInfo)
            {
                return false;
            }

            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append("UPDATE schedule_day_info SET del_flg = @DEL_FLG WHERE id = @ID");

                    command.CommandText = sb.ToString();

                    command.Parameters.Add(new SQLiteParameter("@DEL_FLG", 1));
                    command.Parameters.Add(new SQLiteParameter("@ID", scheduleDayInfo.Id));

                    command.ExecuteNonQuery();
                }
            }

            return true;
        }

        /// <summary>
        /// 予定日情報の優先度を更新
        /// </summary>
        /// <param name="scheduleDayInfo">予定日情報</param>
        /// <returns>更新結果</returns>
        public bool UpdatePriorityOfScheduleDayInfo(ScheduleDayInfo scheduleDayInfo)
        {
            if (null == scheduleDayInfo)
            {
                return false;
            }

            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append("UPDATE schedule_day_info SET priority = @PRIORITY WHERE id = @ID");

                    command.CommandText = sb.ToString();

                    command.Parameters.Add(new SQLiteParameter("@PRIORITY", scheduleDayInfo.Priority));
                    command.Parameters.Add(new SQLiteParameter("@ID", scheduleDayInfo.Id));

                    command.ExecuteNonQuery();
                }
            }

            return true;
        }

        /// <summary>
        /// 予定日情報を登録
        /// </summary>
        /// <param name="scheduleDayInfo">予定日情報</param>
        /// <returns>登録結果</returns>
        public bool DeleteInsertScheduleDayInfo(ScheduleDayInfo scheduleDayInfo)
        {
            if (null == scheduleDayInfo)
            {
                return false;
            }

            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    if (0 < scheduleDayInfo.Id)
                    {
                        sb.Append("UPDATE schedule_day_info SET del_flg = @DEL_FLG WHERE id = @ID AND schedule_date = @SCHEDULE_DATE");

                        command.CommandText = sb.ToString();

                        command.Parameters.Add(new SQLiteParameter("@DEL_FLG", 1));
                        command.Parameters.Add(new SQLiteParameter("@ID", scheduleDayInfo.Id));
                        command.Parameters.Add(new SQLiteParameter("@SCHEDULE_DATE", scheduleDayInfo.ScheduleDate.ToString(Constant.DATE_FORMAT)));

                        command.ExecuteNonQuery();

                        sb.Clear();
                        command.Parameters.Clear();
                    }

                    sb.Append("INSERT INTO schedule_day_info (schedule_date, priority, title, contents, del_id, del_flg, created_at, updated_at) VALUES (");
                    sb.Append("@SCHEDULE_DATE");
                    sb.Append(",");
                    sb.Append("@PRIORITY");
                    sb.Append(",");
                    sb.Append("@TITLE");
                    sb.Append(",");
                    sb.Append("@CONTENTS");
                    sb.Append(",");
                    sb.Append("@DEL_ID");
                    sb.Append(",");
                    sb.Append("@DEL_FLG");
                    sb.Append(",");
                    sb.Append("@CREATED_AT");
                    sb.Append(",");
                    sb.Append("@UPDATED_AT");
                    sb.Append(")");

                    command.CommandText = sb.ToString();

                    command.Parameters.Add(new SQLiteParameter("@SCHEDULE_DATE", scheduleDayInfo.ScheduleDate.ToString(Constant.DATE_FORMAT)));
                    command.Parameters.Add(new SQLiteParameter("@PRIORITY", scheduleDayInfo.Priority));
                    command.Parameters.Add(new SQLiteParameter("@TITLE", scheduleDayInfo.Title));
                    command.Parameters.Add(new SQLiteParameter("@CONTENTS", scheduleDayInfo.Contents));
                    command.Parameters.Add(new SQLiteParameter("@DEL_ID", scheduleDayInfo.Id));
                    command.Parameters.Add(new SQLiteParameter("@DEL_FLG", scheduleDayInfo.DelFlg));
                    command.Parameters.Add(new SQLiteParameter("@CREATED_AT", scheduleDayInfo.CreatedAt.ToString(Constant.DATE_TIME_FORMAT)));
                    command.Parameters.Add(new SQLiteParameter("@UPDATED_AT", scheduleDayInfo.UpdatedAt.ToString(Constant.DATE_TIME_FORMAT)));

                    command.ExecuteNonQuery();
                }
            }

            return true;
        }

        /// <summary>
        /// 優先度情報リストを返す
        /// </summary>
        /// <param name="delInclude">削除含むフラグ</param>
        /// <returns>優先度情報リスト</returns>
        public List<PriorityInfo> GetPriorityInfoList(bool delInclude = false)
        {
            List<PriorityInfo> priorityInfoList = new List<PriorityInfo>();
            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    if (delInclude)
                    {
                        command.CommandText = DML_PRIORITY_INFO_SELECT;
                    }
                    else
                    {
                        command.CommandText = DML_PRIORITY_INFO_SELECT_DEL;
                    }

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PriorityInfo priorityInfo = new PriorityInfo
                            {
                                Id = int.Parse(reader["id"].ToString()),
                                Priority = (string)reader["priority"],
                                Contents = (string)reader["contents"],
                                DelFlg = int.Parse(reader["del_flg"].ToString()),
                                CreatedAt = DateTime.Parse(reader["created_at"].ToString()),
                                UpdatedAt = DateTime.Parse(reader["updated_at"].ToString())
                            };
                            priorityInfoList.Add(priorityInfo);
                        }
                    }
                }
            }

            return priorityInfoList;
        }

        /// <summary>
        /// 優先度情報を登録
        /// </summary>
        /// <param name="priorityInfo">優先度情報</param>
        /// <returns>登録結果</returns>
        public bool DeleteInsertPriorityInfo(PriorityInfo priorityInfo)
        {
            if (null == priorityInfo)
            {
                return false;
            }

            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    if (0 < priorityInfo.Id)
                    {
                        sb.Append("UPDATE priority_info SET del_flg = @DEL_FLG WHERE id = @ID");

                        command.CommandText = sb.ToString();

                        command.Parameters.Add(new SQLiteParameter("@DEL_FLG", 1));
                        command.Parameters.Add(new SQLiteParameter("@ID", priorityInfo.Id));

                        command.ExecuteNonQuery();

                        sb.Clear();
                        command.Parameters.Clear();
                    }

                    sb.Append("INSERT INTO priority_info (priority, contents, del_flg, created_at, updated_at) VALUES (");
                    sb.Append("@PRIORITY");
                    sb.Append(",");
                    sb.Append("@CONTENTS");
                    sb.Append(",");
                    sb.Append("@DEL_FLG");
                    sb.Append(",");
                    sb.Append("@CREATED_AT");
                    sb.Append(",");
                    sb.Append("@UPDATED_AT");
                    sb.Append(")");

                    command.CommandText = sb.ToString();

                    command.Parameters.Add(new SQLiteParameter("@PRIORITY", priorityInfo.Priority));
                    command.Parameters.Add(new SQLiteParameter("@CONTENTS", priorityInfo.Contents));
                    command.Parameters.Add(new SQLiteParameter("@DEL_FLG", priorityInfo.DelFlg));
                    command.Parameters.Add(new SQLiteParameter("@CREATED_AT", priorityInfo.CreatedAt.ToString(Constant.DATE_TIME_FORMAT)));
                    command.Parameters.Add(new SQLiteParameter("@UPDATED_AT", priorityInfo.UpdatedAt.ToString(Constant.DATE_TIME_FORMAT)));

                    command.ExecuteNonQuery();
                }
            }

            return true;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Init()
        {
            using (var conn = new SQLiteConnection(sqlConnectionSB.ToString()))
            {
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append("CREATE TABLE IF NOT EXISTS schedule_day_info (");
                    sb.Append("   id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
                    sb.Append(" , schedule_date TEXT NOT NULL");
                    sb.Append(" , priority TEXT");
                    sb.Append(" , title TEXT NOT NULL");
                    sb.Append(" , contents TEXT");
                    sb.Append(" , del_id INTEGER");
                    sb.Append(" , del_flg INTEGER NOT NULL");
                    sb.Append(" , created_at TEXT NOT NULL");
                    sb.Append(" , updated_at TEXT NOT NULL");
                    sb.Append(")");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();

                    sb.Clear();

                    sb.Append("CREATE TABLE IF NOT EXISTS priority_info (");
                    sb.Append("   id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT");
                    sb.Append(" , priority TEXT NOT NULL");
                    sb.Append(" , contents TEXT");
                    sb.Append(" , del_flg INTEGER NOT NULL");
                    sb.Append(" , created_at TEXT NOT NULL");
                    sb.Append(" , updated_at TEXT NOT NULL");
                    sb.Append(")");

                    command.CommandText = sb.ToString();
                    command.ExecuteNonQuery();
                }
            }

            InitPriorityInfo();
        }

        /// <summary>
        /// 優先度情報初期化
        /// </summary>
        private void InitPriorityInfo()
        {
            List<PriorityInfo> priorityInfoList = GetPriorityInfoList();

            if (0 < priorityInfoList.Count)
            {
                return;
            }

            DateTime dateTime = DateTime.Now;

            PriorityInfo priorityInfo = new PriorityInfo
            {
                Priority = "A",
                Contents = "最優先",
                CreatedAt = dateTime,
                UpdatedAt = dateTime
            };

            DeleteInsertPriorityInfo(priorityInfo);

            priorityInfo = new PriorityInfo
            {
                Priority = "B",
                Contents = "優先",
                CreatedAt = dateTime,
                UpdatedAt = dateTime
            };

            DeleteInsertPriorityInfo(priorityInfo);

            priorityInfo = new PriorityInfo
            {
                Priority = "C",
                Contents = "一般",
                CreatedAt = dateTime,
                UpdatedAt = dateTime
            };

            DeleteInsertPriorityInfo(priorityInfo);
        }
    }
}
