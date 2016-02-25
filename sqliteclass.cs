using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows;
using System.Data.SQLite;

namespace mng
{
    class sqliteclass
    {
        //Конструктор
        public sqliteclass()
        {

        }

        #region iExecuteNonQuery
        public int iExecuteNonQuery(string FileData, string sSql, int where)
        {
            int n = 0;
            try
            {
                using (SQLiteConnection con = new SQLiteConnection())
                {
                    if (where == 0)
                    {
                        con.ConnectionString = @"Data Source=" + FileData + ";New=True;Version=3";
                    }
                    else
                    {
                        con.ConnectionString = @"Data Source=" + FileData + ";New=False;Version=3";
                    }
                    con.Open();
                    using (SQLiteCommand sqlCommand = con.CreateCommand())
                    {
                        sqlCommand.CommandText = sSql;
                        n = sqlCommand.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                n = 0;
            }
            return n;
        }

        #endregion

        #region drExecute
        public DataRow[] drExecute(string FileData, string sSql)
        {
            DataRow[] datarows = null;
            SQLiteDataAdapter dataadapter = null;
            DataSet dataset = new DataSet();
            DataTable datatable = new DataTable();
            try
            {
                using (SQLiteConnection con = new SQLiteConnection())
                {
                    con.ConnectionString = @"Data Source = " + FileData + ";New=False;Version=3";
                    con.Open();
                    using (SQLiteCommand sqlCommand = con.CreateCommand())
                    {
                        dataadapter = new SQLiteDataAdapter(sSql, con);
                        dataset.Reset();
                        dataadapter.Fill(dataset);
                        datatable = dataset.Tables[0];
                        datarows = datatable.Select();

                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {

                datarows = null;
            }
            return datarows;

        }
        #endregion

        public DataSet dataSetLoader(string FileData)
        {
            DataSet dataset = new DataSet();
            using (SQLiteConnection con = new SQLiteConnection())
            {
                con.ConnectionString = @"Data Source= " + FileData + ";Version=3;";
                con.Open();
                using (SQLiteCommand sqlCommand = con.CreateCommand())
                {
                    string sSql = "select * from LST_CLASS;";
                    var da = new SQLiteDataAdapter(sSql, con.ConnectionString);
                    da.Fill(dataset, "LST_ClASS");
                    sSql = "select * from LST_GR";
                    da = new SQLiteDataAdapter(sSql, con.ConnectionString);
                    da.Fill(dataset, "LST_GR");
                    sSql = "select * from LST_PDGR";
                    da = new SQLiteDataAdapter(sSql, con.ConnectionString);
                    da.Fill(dataset, "LST_PDGR");
                    sSql = "select rowid,slLST_tbldescr.* from slLST_tbldescr";
                    da = new SQLiteDataAdapter(sSql, con.ConnectionString);
                    da.Fill(dataset, "slLST_tbldescr");
                }
                con.Close();
            }
            return dataset;
        }

        public DataSet dataSetParamLoader(string FileData)
        {
            DataSet datasetparam = new DataSet();

            using (SQLiteConnection con = new SQLiteConnection())
            {
                con.ConnectionString = @"Data Source= " + FileData + ";Version=3;";
                con.Open();
                using (SQLiteCommand sqlCommand = con.CreateCommand())
                {
                    string sqlcmd = "select * from LST_INPAR";
                    var da = new SQLiteDataAdapter(sqlcmd, con.ConnectionString);
                    da.Fill(datasetparam, "LST_INPAR");

                    sqlcmd = "select * from LST_ITEM";
                    da = new SQLiteDataAdapter(sqlcmd, con.ConnectionString);
                    da.Fill(datasetparam, "LST_ITEM");

                    sqlcmd = "select * from sl_table";
                    da = new SQLiteDataAdapter(sqlcmd, con.ConnectionString);
                    da.Fill(datasetparam, "sl_table");
                }
                con.Close();
            }
            return datasetparam;
        }
    }
}

