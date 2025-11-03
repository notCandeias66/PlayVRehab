using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Mono.Data.Sqlite;
using System.IO;
using System.Linq;

public class SQLiteManager
{
    private string dbName = "PlayVRehab.db";
    private string dbPath;

    // Singleton instance
    private static SQLiteManager instance;

    // Private constructor to prevent instantiation from outside
    private SQLiteManager()
    {
        // Initialize the database path here (you can modify this as per your requirements)
        dbPath = Path.Combine(@"C:\Users\cande\Desktop\PlayVRehab\", dbName);  // Default path
        CreateDatabase();  // Ensure the database is created or checked
    }

    // Property to access the instance
    // Public static method to get the singleton instance
    public static SQLiteManager GetInstance()
    {
        if (instance == null)
        {
            instance = new SQLiteManager(); // Create the instance if it doesn't exist
        }
        return instance;
    }

    // Check if the database exists or not, creates it if it doesn't
    // Also creates the tables if needed
    private void CreateDatabase()
    {
        if (!File.Exists(dbPath))
        {
            using (var dbConnection = OpenConnection()) { } // Ensure SQLite initializes properly
            Debug.Log("Database created at " + dbPath);

        } else {
            Debug.Log("Database already exists with the path: " + dbPath);
        }

        CheckAndCreateTables();
    }

    private IDbConnection OpenConnection()
    {
        string connectionString = "URI=file:" + dbPath;
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();
        return dbConnection;
    }

    private bool TableExists(IDbConnection dbConnection, string tableName)
    {
        using (IDbCommand dbCommand = dbConnection.CreateCommand())
        {
            dbCommand.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'";

            using (IDataReader reader = dbCommand.ExecuteReader())
            {
                return reader.Read(); // If the reader returns a row, the table exists.
            }
        }
    }

    private void CheckAndCreateTables()
    {
        using (IDbConnection dbConnection = OpenConnection())
        {
            try
            {
                if (!TableExists(dbConnection, "User"))
                {
                    using (IDbCommand dbCommand = dbConnection.CreateCommand())
                    {
                        string createUserTable = @"
                        CREATE TABLE Users (
                            id INTEGER UNIQUE,
                            name TEXT,
                            email TEXT UNIQUE,
                            role TEXT,
                            password_hash TEXT NOT NULL,
                            physio_id INTEGER,
                            qrcode_number INTEGER DEFAULT 0,
                            PRIMARY KEY (id),
                            FOREIGN KEY (physio_id) REFERENCES User(id)
                        );";

                        dbCommand.CommandText = createUserTable;
                        dbCommand.ExecuteNonQuery();
                        Debug.Log("Table 'User' was missing and has been created.");
                    }

                } else {
                    Debug.Log("The table User already exists");
                }

                if (!TableExists(dbConnection, "UserLevels"))
                {
                    using (IDbCommand dbCommand = dbConnection.CreateCommand())
                    {
                        string createUserLevelsTable = @"
                        CREATE TABLE UserLevels (
	                        user_id	INTEGER NOT NULL,
	                        level_id INTEGER NOT NULL,
	                        parameters	TEXT DEFAULT 'No parameters specified',
	                        score	INTEGER DEFAULT 0,
	                        time_taken	INTEGER DEFAULT -1,
	                        status	TEXT DEFAULT 'Not Created',
	                        observations	TEXT DEFAULT 'Nothing to observe!',
	                        progression	TEXT,
                            PRIMARY KEY (user_id, level_id),
	                        FOREIGN KEY(level_id) REFERENCES Level(id),
	                        FOREIGN KEY(user_id) REFERENCES User(id)
                    );";

                        dbCommand.CommandText = createUserLevelsTable;
                        dbCommand.ExecuteNonQuery();
                        Debug.Log("Table 'UserLevels' was missing and has been created.");
                    }

                } else {
                    Debug.Log("The table Level already exists");
                }

                if (!TableExists(dbConnection, "Level"))
                {
                    using (IDbCommand dbCommand = dbConnection.CreateCommand())
                    {
                        string createLevelTable = @"
                        CREATE TABLE Level (
	                        id	INTEGER UNIQUE,
	                        level_string TEXT,
                            description TEXT,
                            PRIMARY KEY (id, AUTOINCREMENT)
                    );";

                        dbCommand.CommandText = createLevelTable;
                        dbCommand.ExecuteNonQuery();
                        Debug.Log("Table 'Level' was missing and has been created.");
                    }
                }
                else
                {
                    Debug.Log("The table Level already exists");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to check/create tables: " + ex.Message);
            }
        }
    }

    // All the bellow methods are for use in other scripts to be able to get database info

    // Get all table data
    public void GetTableData(string tableName)
    {
        using (IDbConnection dbConnection = OpenConnection())
        {
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = $"SELECT * FROM {tableName}";

                try
                {
                    using (IDataReader reader = dbCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string row = "";

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row += reader.GetName(i) + ": " + reader[i].ToString() + " | ";
                            }

                            Debug.Log(row);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error fetching data from {tableName}: {ex.Message}");
                }
            }
        }
    }

    // Get entries in the table based on the value in the specific field
    public List<Dictionary<string, object>> GetFilteredData(string tableName, string columnName, object value)
    {
        List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();

        using (IDbConnection dbConnection = OpenConnection())
        {
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = $"SELECT * FROM {tableName} WHERE {columnName} = @value";

                // Use parameterized queries to prevent SQL injection
                var parameter = dbCommand.CreateParameter();
                parameter.ParameterName = "@value";
                parameter.Value = value;
                dbCommand.Parameters.Add(parameter);

                try
                {
                    using (IDataReader reader = dbCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, object> row = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader[i];
                            }

                            //Debug.Log(row);
                            results.Add(row);
                        }
                    }
                } 
                catch (Exception ex)
                {
                    Debug.LogError($"Error fetching filtered data from {tableName}: {ex.Message}");
                }
            }
        }

        return results;
    }

    public void InsertData(string tableName, Dictionary<string, object> data)
    {
        using (IDbConnection dbConnection = OpenConnection())
        {
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                try
                {
                    // Generate column names and values dynamically
                    string columns = string.Join(", ", data.Keys);
                    string values = string.Join(", ", data.Keys.Select(k => "@" + k));

                    dbCommand.CommandText = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

                    // Add parameters dynamically
                    foreach (var pair in data)
                    {
                        var parameter = dbCommand.CreateParameter();
                        parameter.ParameterName = "@" + pair.Key;
                        parameter.Value = pair.Value;
                        dbCommand.Parameters.Add(parameter);
                    }

                    dbCommand.ExecuteNonQuery();
                    Debug.Log($"Inserted into {tableName}: {string.Join(", ", data.Select(kv => kv.Key + "=" + kv.Value))}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error inserting data into {tableName}: {ex.Message}");
                }
            }
        }
    }

    public void DeleteData(string tableName, string columnName, object value)
    {
        using (IDbConnection dbConnection = OpenConnection())
        {
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                try
                {
                    dbCommand.CommandText = $"DELETE FROM {tableName} WHERE {columnName} = @value";

                    var parameter = dbCommand.CreateParameter();
                    parameter.ParameterName = "@value";
                    parameter.Value = value;
                    dbCommand.Parameters.Add(parameter);

                    int rowsAffected = dbCommand.ExecuteNonQuery();
                    Debug.Log($"{rowsAffected} row(s) deleted from {tableName} where {columnName} = {value}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error deleting data from {tableName}: {ex.Message}");
                }
            }
        }
    }

    public bool LoginUser(int id, string name, string password_hash, int qrcode_number)
    {
        using (IDbConnection dbConnection = OpenConnection())
        {
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = @"
                SELECT COUNT(*) 
                FROM User 
                WHERE id = @id AND name = @name AND password_hash = @passwordHash AND qrcode_number = @qrcodeNumber";

                // Add parameters to prevent SQL injection
                var idParam = dbCommand.CreateParameter();
                idParam.ParameterName = "@id";
                idParam.Value = id;
                dbCommand.Parameters.Add(idParam);

                var nameParam = dbCommand.CreateParameter();
                nameParam.ParameterName = "@name";
                nameParam.Value = name;
                dbCommand.Parameters.Add(nameParam);

                var passwordParam = dbCommand.CreateParameter();
                passwordParam.ParameterName = "@passwordHash";
                passwordParam.Value = password_hash;
                dbCommand.Parameters.Add(passwordParam);

                var qrcodeParam = dbCommand.CreateParameter();
                qrcodeParam.ParameterName = "@qrcodeNumber";
                qrcodeParam.Value = qrcode_number;
                dbCommand.Parameters.Add(qrcodeParam);

                try
                {
                    // Execute the query and check if there's at least one matching user
                    var result = dbCommand.ExecuteScalar();
                    return Convert.ToInt32(result) > 0;  // Returns true if user found
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during user login: {ex.Message}");
                    return false;
                }
            }
        }
    }

    public void UpdateUserLevelData(int userId, int levelId, int attemptNumber, int score, int time_taken, string status, string game_results, string progression)
    {
        using (IDbConnection dbConnection = OpenConnection())
        {
            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                try
                {
                    dbCommand.CommandText = @"
                    UPDATE UserLevels
                    SET 
                        score = @score,
                        time_taken = @time_taken,
                        status = @status,
                        game_results = @game_results,
                        progression = @progression
                    WHERE 
                        user_id = @user_id
                        AND level_id = @level_id
                        AND attempt = @attempt;
                ";

                // Add parameters
                var parameters = new Dictionary<string, object>
                {
                    { "@score", score },
                    { "@time_taken", time_taken },
                    { "@status", status },
                    { "@game_results", game_results },
                    { "@progression", progression },
                    { "@user_id", userId },
                    { "@level_id", levelId },
                    { "@attempt", attemptNumber }
                };

                    foreach (var param in parameters)
                    {
                        var dbParam = dbCommand.CreateParameter();
                        dbParam.ParameterName = param.Key;
                        dbParam.Value = param.Value ?? DBNull.Value;
                        dbCommand.Parameters.Add(dbParam);
                    }

                    int rowsAffected = dbCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Debug.Log($"Updated UserLevels: user_id={userId}, level_id={levelId}, attempt={attemptNumber}");
                    }
                    else
                    {
                        Debug.LogWarning($"No rows updated in UserLevels for user_id={userId}, level_id={levelId}, attempt={attemptNumber}. Record might not exist.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error updating UserLevels: {ex.Message}");
                }
            }
        }
    }





}
