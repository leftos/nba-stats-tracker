using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows;

internal class SQLiteDatabase
{
    private readonly String dbConnection;

    /// <summary>
    ///     Default Constructor for SQLiteDatabase Class.
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
        dbConnection = String.Format("Data Source={0}", inputFile);
    }

    /// <summary>
    ///     Single Param Constructor for specifying advanced connection options.
    /// </summary>
    /// <param name="connectionOpts">A dictionary containing all desired options and their values</param>
    public SQLiteDatabase(Dictionary<String, String> connectionOpts)
    {
        String str = "";
        foreach (var row in connectionOpts)
        {
            str += String.Format("{0}={1}; ", row.Key, row.Value);
        }
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
        var dt = new DataTable();
        try
        {
            var cnn = new SQLiteConnection(dbConnection);
            cnn.Open();
            var mycommand = new SQLiteCommand(cnn);
            mycommand.CommandText = sql;
            SQLiteDataReader reader = mycommand.ExecuteReader();
            dt.Load(reader);
            reader.Close();
            cnn.Close();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message + "\n\nQuery: " + sql);
        }
        return dt;
    }

    /// <summary>
    ///     Allows the programmer to interact with the database for purposes other than a query.
    /// </summary>
    /// <param name="sql">The SQL to be run.</param>
    /// <returns>An Integer containing the number of rows updated.</returns>
    public int ExecuteNonQuery(string sql)
    {
        var cnn = new SQLiteConnection(dbConnection);
        cnn.Open();
        var mycommand = new SQLiteCommand(cnn);
        mycommand.CommandText = sql;
        int rowsUpdated = mycommand.ExecuteNonQuery();
        cnn.Close();
        return rowsUpdated;
    }

    /// <summary>
    ///     Allows the programmer to retrieve single items from the DB.
    /// </summary>
    /// <param name="sql">The query to run.</param>
    /// <returns>A string.</returns>
    public string ExecuteScalar(string sql)
    {
        var cnn = new SQLiteConnection(dbConnection);
        cnn.Open();
        var mycommand = new SQLiteCommand(cnn);
        mycommand.CommandText = sql;
        object value = mycommand.ExecuteScalar();
        cnn.Close();
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
    /// <returns>A boolean true or false to signify success or failure.</returns>
    public bool Update(String tableName, Dictionary<String, String> data, String where)
    {
        string sql = "";
        String vals = "";
        Boolean returnCode = true;
        if (data.Count >= 1)
        {
            foreach (var val in data)
            {
                vals += String.Format(" {0} = '{1}',", val.Key, val.Value);
            }
            vals = vals.Substring(0, vals.Length - 1);
        }
        try
        {
            sql = String.Format("update {0} set {1} where {2};", tableName, vals, where);
            ExecuteNonQuery(sql);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + "\n\nQuery: " + sql);
            returnCode = false;
        }
        return returnCode;
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
            values += String.Format(" '{0}',", val.Value);
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
    ///     Allows the programmer to easily delete all data from the DB.
    /// </summary>
    /// <returns>A boolean true or false to signify success or failure.</returns>
    public bool ClearDB()
    {
        DataTable tables;
        try
        {
            tables = GetDataTable("select NAME from SQLITE_MASTER where type='table' order by NAME;");
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

    public static string ConvertDateTimeToSQLite(DateTime dt)
    {
        return String.Format("{0:yyyy-MM-dd HH:mm:ss}", dt);
    }

    public static string AddDateRangeToSQLQuery(string query, DateTime dStart, DateTime dEnd, bool addWhere = false)
    {
        if (query.EndsWith(";"))
            query = query.Remove(query.Length - 1);

        if (!addWhere)
        {
            query = String.Concat(query, String.Format(" AND (Date >= '{0}' AND Date <= '{1}')",
                                                       ConvertDateTimeToSQLite(dStart),
                                                       ConvertDateTimeToSQLite(dEnd)));
        }
        else
        {
            query = String.Concat(query, String.Format(" WHERE (Date >= '{0}' AND Date <= '{1}')",
                                                       ConvertDateTimeToSQLite(dStart),
                                                       ConvertDateTimeToSQLite(dEnd)));
        }
        return query;
    }
}