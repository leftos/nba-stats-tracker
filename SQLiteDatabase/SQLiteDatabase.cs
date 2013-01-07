#region Copyright Notice

// Created by brennydoogles, (c) 2010
// Source: http://www.dreamincode.net/forums/topic/157830-using-sqlite-with-c%23/
// Adapted from Mike Duncan's tutorial
// Source: http://www.mikeduncan.com/sqlite-on-dotnet-in-3-mins/
//
//
// Improved by Lefteris Aslanoglou, (c) 2011-2012
// as a Class Library for the implementation of thesis
// "Application Development for Basketball Statistical Analysis in Natural Language"
// under the supervision of Prof. Athanasios Tsakalidis & MSc Alexandros Georgiou,
// Computer Engineering & Informatics Department, University of Patras, Greece.
// 
// All rights reserved. Unless specifically stated otherwise, the code in this file should 
// not be reproduced, edited and/or republished without explicit permission from the 
// author.

#endregion

#region Using Directives

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;

#endregion

namespace SQLite_Database
{
    public class SQLiteDatabase
    {
        private readonly String dbConnection;

        /// <summary>
        ///     Default Constructor for SQLiteDatabase Class. Connects to the "D:\test.sqlite" database file.
        /// </summary>
        public SQLiteDatabase()
        {
            dbConnection = @"Data Source=D:\test.sqlite";
        }

        /// <summary>
        ///     Single Param Constructor for specifying the DB file.
        /// </summary>
        /// <param name="inputFile">The File containing the DB</param>
        public SQLiteDatabase(String inputFile)
        {
            dbConnection = String.Format("Data Source={0}; PRAGMA cache_size=20000; PRAGMA page_size=32768", inputFile);
        }

        /// <summary>
        ///     Single Param Constructor for specifying advanced connection options.
        /// </summary>
        /// <param name="connectionOpts">A dictionary containing all desired options and their values</param>
        public SQLiteDatabase(Dictionary<String, String> connectionOpts)
        {
            String str = connectionOpts.Aggregate("", (current, row) => current + String.Format("{0}={1}; ", row.Key, row.Value));
            str = str.Trim().Substring(0, str.Length - 1);
            dbConnection = str;
        }

        /// <summary>
        ///     Allows the programmer to run a query against the Database.
        /// </summary>
        /// <param name="sql">The SQL to run</param>
        /// <returns>A DataTable containing the result set.</returns>
        public DataTable GetDataTable(string sql)
        {
            using (var dt = new DataTable())
            {
                try
                {
                    using (var cnn = new SQLiteConnection(dbConnection))
                    {
                        cnn.Open();
                        SQLiteDataReader reader;
                        using (var mycommand = new SQLiteCommand(cnn))
                        {
                            mycommand.CommandText = sql;
                            reader = mycommand.ExecuteReader();
                        }
                        dt.Load(reader);
                        reader.Close();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message + "\n\nQuery: " + sql);
                }
                return dt;
            }
        }

        /// <summary>
        ///     Allows the programmer to interact with the database for purposes other than a query.
        /// </summary>
        /// <param name="sql">The SQL to be run.</param>
        /// <returns>An Integer containing the number of rows updated.</returns>
        public int ExecuteNonQuery(string sql)
        {
            SQLiteConnection cnn;
            int rowsUpdated;
            using (cnn = new SQLiteConnection(dbConnection))
            {
                cnn.Open();
                SQLiteTransaction sqLiteTransaction = cnn.BeginTransaction();
                SQLiteCommand mycommand;
                using (mycommand = new SQLiteCommand(cnn))
                {
                    mycommand.Transaction = sqLiteTransaction;
                    mycommand.CommandText = sql;
                    rowsUpdated = mycommand.ExecuteNonQuery();
                    sqLiteTransaction.Commit();
                }
                cnn.Close();
            }
            return rowsUpdated;
        }

        /// <summary>
        ///     Allows the programmer to retrieve single items from the DB.
        /// </summary>
        /// <param name="sql">The query to run.</param>
        /// <returns>A string.</returns>
        public string ExecuteScalar(string sql)
        {
            object value;
            using (var cnn = new SQLiteConnection(dbConnection))
            {
                cnn.Open();
                using (var mycommand = new SQLiteCommand(cnn))
                {
                    mycommand.CommandText = sql;
                    value = mycommand.ExecuteScalar();
                }
            }
            if (value != null)
            {
                return value.ToString();
            }
            return "";
        }

        /// <summary>
        ///     Allows the programmer to easily update rows in the DB.
        /// </summary>
        /// <param name="tableName">The table to update.</param>
        /// <param name="data">A dictionary containing Column names and their new values.</param>
        /// <param name="where">The where clause for the update statement.</param>
        /// <returns>An integer that represents the amount of rows updated, or -1 if the query failed.</returns>
        public int Update(String tableName, Dictionary<String, String> data, String where)
        {
            string sql = "";
            String vals = "";
            int returnCode;
            if (data.Count >= 1)
            {
                vals = data.Aggregate(vals, (current, val) => current + String.Format(" {0} = \"{1}\",", val.Key, val.Value));
                vals = vals.Substring(0, vals.Length - 1);
            }
            try
            {
                sql = String.Format("update {0} set {1} where {2};", tableName, vals, where);
                returnCode = ExecuteNonQuery(sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\nQuery: " + sql);
                returnCode = -1;
            }
            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily update multiple records into the DB via transaction command-wrapping
        /// </summary>
        /// <param name="tableName">The table into which we update the data.</param>
        /// <param name="dataList">A list of dictionaries containing the column names and data for the update.</param>
        /// <param name="whereList">A list of strings containing the according where criteria for each update.</param>
        public void UpdateManyTransaction(String tableName, List<Dictionary<String, String>> dataList, List<String> whereList)
        {
            SQLiteConnection cnn;
            String vals = "";
            using (cnn = new SQLiteConnection(dbConnection))
            {
                cnn.Open();
                using (var cmd = new SQLiteCommand(cnn))
                {
                    using (SQLiteTransaction transaction = cnn.BeginTransaction())
                    {
                        for (int i = 0; i < dataList.Count; i++)
                        {
                            Dictionary<string, string> data = dataList[i];
                            if (data.Count >= 1)
                            {
                                vals = data.Aggregate("", (current, val) => current + String.Format(" {0} = \"{1}\",", val.Key, val.Value));
                                vals = vals.Substring(0, vals.Length - 1);
                            }
                            try
                            {
                                cmd.CommandText = String.Format("update {0} set {1} where {2};", tableName, vals, whereList[i]);
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception fail)
                            {
                                MessageBox.Show(fail.Message + "\n\nIndex: " + i + "\n\nQuery: " + cmd.CommandText);
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        ///     Allows the programmer to easily delete rows from the DB.
        /// </summary>
        /// <param name="tableName">The table from which to delete.</param>
        /// <param name="where">The where clause for the delete.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Delete(String tableName, String where)
        {
            Boolean returnCode = true;
            try
            {
                ExecuteNonQuery(String.Format("delete from {0} where {1};", tableName, where));
            }
            catch (Exception fail)
            {
                MessageBox.Show(fail.Message);
                returnCode = false;
            }
            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily insert into the DB
        /// </summary>
        /// <param name="tableName">The table into which we insert the data.</param>
        /// <param name="data">A dictionary containing the column names and data for the insert.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Insert(String tableName, Dictionary<String, String> data)
        {
            String columns = "";
            String values = "";
            string sql = "";
            Boolean returnCode = true;
            foreach (var val in data)
            {
                columns += String.Format(" {0},", val.Key);
                values += String.Format(" \"{0}\",", val.Value);
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);
            try
            {
                sql = String.Format("insert into {0}({1}) values({2});", tableName, columns, values);
                ExecuteNonQuery(sql);
            }
            catch (Exception fail)
            {
                MessageBox.Show(fail.Message + "\n\nQuery: " + sql);
                returnCode = false;
            }
            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily insert multiple records into the DB via transaction command-wrapping
        /// </summary>
        /// <param name="tableName">The table into which we insert the data.</param>
        /// <param name="dataList">A list of dictionaries containing the column names and data for the insert.</param>
        public void InsertManyTransaction(String tableName, List<Dictionary<String, String>> dataList)
        {
            SQLiteConnection cnn;
            using (cnn = new SQLiteConnection(dbConnection))
            {
                cnn.Open();
                using (var cmd = new SQLiteCommand(cnn))
                {
                    using (SQLiteTransaction transaction = cnn.BeginTransaction())
                    {
                        for (int i = 0; i < dataList.Count; i++)
                        {
                            Dictionary<string, string> data = dataList[i];
                            String columns = "";
                            String values = "";
                            foreach (var val in data)
                            {
                                columns += String.Format(" {0},", val.Key);
                                values += String.Format(" \"{0}\",", val.Value);
                            }
                            columns = columns.Substring(0, columns.Length - 1);
                            values = values.Substring(0, values.Length - 1);
                            try
                            {
                                cmd.CommandText = String.Format("insert into {0}({1}) values({2});", tableName, columns, values);
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception fail)
                            {
                                MessageBox.Show(fail.Message + "\n\nIndex: " + i + "\n\nQuery: " + cmd.CommandText);
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        ///     Allows the programmer to easily insert multiple records into the DB
        /// </summary>
        /// <param name="tableName">The table into which we insert the data.</param>
        /// <param name="data">A list of dictionaries containing the column names and data for the insert.
        ///                     All dictionaries must have the same order of inserted pairs. 
        ///                     The dictionary MUST NOT be more than 500 pairs in length; an exception is thrown if it is.</param>
        /// <returns>The number of lines affected by this query.</returns>
        public int InsertManyUnion(String tableName, List<Dictionary<String, String>> data)
        {
            if (data.Count > 500)
                throw new Exception("SQLite error: Tried to insert more than 500 rows at once.");

            int returnCode;

            string sql = "insert into " + tableName + " SELECT";

            sql = data[0].Aggregate(sql, (current, val) => current + String.Format(" \"{0}\" AS {1},", val.Value, val.Key));
            sql = sql.Remove(sql.Length - 1);
            data.RemoveAt(0);
            foreach (var dict in data)
            {
                sql += " UNION SELECT";
                sql = dict.Aggregate(sql, (current, val) => current + String.Format(" \"{0}\",", val.Value));
                sql = sql.Remove(sql.Length - 1);
            }

            try
            {
                returnCode = ExecuteNonQuery(sql);
            }
            catch (Exception fail)
            {
                MessageBox.Show(fail.Message + "\n\nQuery: " + sql);
                returnCode = 0;
            }
            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily delete all data from the DB.
        /// </summary>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool ClearDB()
        {
            try
            {
                DataTable tables = GetDataTable("select NAME from SQLITE_MASTER where type='table' order by NAME;");
                foreach (DataRow table in tables.Rows)
                {
                    ClearTable(table["NAME"].ToString());
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Allows the user to easily clear all data from a specific table.
        /// </summary>
        /// <param name="table">The name of the table to clear.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool ClearTable(String table)
        {
            try
            {
                ExecuteNonQuery(String.Format("delete from {0};", table));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts a DateTime object into an SQLite-compatible date string.
        /// </summary>
        /// <param name="dt">The DateTime object.</param>
        /// <returns></returns>
        public static string ConvertDateTimeToSQLite(DateTime dt)
        {
            return String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
        }

        /// <summary>
        /// Adds a date range to an SQL query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="dStart">The starting date.</param>
        /// <param name="dEnd">The ending.</param>
        /// <param name="addWhere">if set to <c>true</c> add WHERE to the query.</param>
        /// <returns></returns>
        public static string AddDateRangeToSQLQuery(string query, DateTime dStart, DateTime dEnd, bool addWhere = false)
        {
            if (query.EndsWith(";"))
                query = query.Remove(query.Length - 1);

            if (!addWhere)
            {
                query = String.Concat(query,
                                      String.Format(" AND (Date >= '{0}' AND Date <= '{1}')", ConvertDateTimeToSQLite(dStart),
                                                    ConvertDateTimeToSQLite(dEnd)));
            }
            else
            {
                query = String.Concat(query,
                                      String.Format(" WHERE (Date >= '{0}' AND Date <= '{1}')", ConvertDateTimeToSQLite(dStart),
                                                    ConvertDateTimeToSQLite(dEnd)));
            }
            return query;
        }
    }
}