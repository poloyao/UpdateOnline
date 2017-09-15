using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpdateOnline
{
    public class OracleHelper
    {
        /// <summary>
        /// sqlConnection
        /// </summary>
        private static Oracle.ManagedDataAccess.Client.OracleConnection MyOracleConnection;

        /// <summary>
        /// 获取OracleConnection
        /// </summary>
        /// <returns></returns>
        private static Oracle.ManagedDataAccess.Client.OracleConnection GetCon()
        {
            string path = Application.StartupPath + "\\UpdateVer.ini";
            var ip = OperateIniFile.ReadIniData("info", "IP", "", path);
            string mysqlConnectionString = "";
            if (MyOracleConnection == null)
                MyOracleConnection = new Oracle.ManagedDataAccess.Client.OracleConnection(mysqlConnectionString);

            if (MyOracleConnection.State == System.Data.ConnectionState.Closed)
                MyOracleConnection.Open();

            if (MyOracleConnection.State == System.Data.ConnectionState.Broken)
            {
                MyOracleConnection.Close();
                MyOracleConnection.Open();
            }

            return MyOracleConnection;
        }



        #region 执行MySQL语句或存储过程,返回受影响的行数
        /// <summary>
        /// 执行MySQL语句或存储过程
        /// </summary>
        /// <param name="type">命令类型</param>
        /// <param name="sqlString">sql语句</param>
        /// <param name="pstmt">参数</param>
        /// <returns>执行结果</returns>
        private static int ExecuteNonQuery(String sqlString, CommandType type = CommandType.Text, Oracle.ManagedDataAccess.Client.OracleParameter[] para = null)
        {
            try
            {
                using (OracleCommand com = new OracleCommand())
                {
                    com.Connection = GetCon();
                    com.CommandText = @sqlString;
                    com.CommandType = type;
                    if (para != null)
                        com.Parameters.AddRange(para);

                    int val = com.ExecuteNonQuery();
                    com.Parameters.Clear();

                    return val;
                }
            }
            catch (Exception ex)
            {
                // Logger.Error("执行MySQL语句或存储过程,异常！", ex);

                return 0;
            }
            finally
            {
                if (MyOracleConnection.State != ConnectionState.Closed)
                    MyOracleConnection.Close();
            }
        }


        /// <summary>
        /// 执行带事务的SQL语句或存储过程
        /// </summary>
        /// <param name="trans">事务</param>
        /// <param name="type">命令类型</param>
        /// <param name="sqlString">SQL语句</param>
        /// <param name="pstmt">参数</param>
        /// <returns>执行结果</returns>
        private static int ExecuteNonQuery(Oracle.ManagedDataAccess.Client.OracleTransaction trans, String sqlString, CommandType type = CommandType.Text, Oracle.ManagedDataAccess.Client.OracleParameter[] para = null)
        {
            try
            {
                using (OracleCommand com = new OracleCommand())
                {
                    com.Connection = MyOracleConnection;
                    com.CommandText = @sqlString;
                    com.CommandType = type;
                    if (para != null)
                        com.Parameters.AddRange(para);
                    if (trans != null)
                        com.Transaction = trans;

                    int val = com.ExecuteNonQuery();
                    com.Parameters.Clear();

                    return val;
                }
            }
            catch (Exception ex)
            {
                //Logger.Error("执行MySQL语句或存储过程2,异常！", ex);

                return 0;
            }
            finally
            {
                if (MyOracleConnection.State != ConnectionState.Closed)
                    MyOracleConnection.Close();
            }
        }
        #endregion




        #region 执行SQL语句或存储过程,返回 DataTable

        /// <summary>
        /// 执行SQL语句或存储过程,返回 DataTable
        /// </summary>
        /// <param name="type">命令类型</param>
        /// <param name="sqlString">SQL语句</param>
        /// <returns>执行结果</returns>
        private static DataTable ExecuteReaderToDataTable(String sqlString, CommandType type = CommandType.Text, Oracle.ManagedDataAccess.Client.OracleParameter[] para = null)
        {
            DataTable dt = new DataTable();
            OracleDataReader dr = null;

            try
            {
                using (OracleCommand com = new OracleCommand())
                {
                    com.Connection = GetCon();
                    com.CommandText = @sqlString;
                    com.CommandType = type;
                    if (para != null)
                        com.Parameters.AddRange(para);

                    using (dr = com.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (dr != null)
                            dt.Load(dr);

                        com.Parameters.Clear();
                    }

                    return dt;
                }
            }
            catch (Exception ex)
            {
                //Logger.Error("执行SQL语句或存储过程,返回 DataTable,异常！", ex);

                return null;
            }
            finally
            {
                if (dr != null && !dr.IsClosed)
                    dr.Close();

                if (MyOracleConnection.State != ConnectionState.Closed)
                    MyOracleConnection.Close();
            }
        }
        #endregion

        /// <summary>
        /// 获取版本列表
        /// </summary>
        /// <returns></returns>
        public static DataTable GetVersion(string localVersion)
        {
            var localID = ExecuteReaderToDataTable($@"select id from RLSBXT.UPDATEVERSION where VERSION  = '{localVersion}'");
            if (localID.Rows.Count == 0)
            {
                return ExecuteReaderToDataTable($@"select id,VERSION from RLSBXT.UPDATEVERSION  order by id");
            }
            else
            {
                return ExecuteReaderToDataTable($@"select id,VERSION from RLSBXT.UPDATEVERSION where id > {localID.Rows[0][0]} order by id");
            }
        }

        /// <summary>
        /// 下载升级文件
        /// </summary>
        /// <returns></returns>
        public static DataTable GetUpdateFile(int versionID)
        {
            return ExecuteReaderToDataTable($@"select * from rlsbxt.updatefile where VERSIONID = {versionID}");
        }

    }
}
