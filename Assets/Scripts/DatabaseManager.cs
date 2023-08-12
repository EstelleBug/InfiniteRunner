using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite;

public static class DatabaseManager
{
    private static SQLiteConnection db => new SQLiteConnection(Application.streamingAssetsPath + "/Database/InfiniteRunner.db");
    public static List<DBScore> topScores(int amount) => db.Query<DBScore>($"SELECT * FROM Player ORDER BY score DESC LIMIT {amount}");

    public static void AddNewScore(string name, int score, int coins) => db.Execute($"INSERT INTO Player (username, score, coins) VALUES ('{name}', '{score}', '{coins}')");
    public static void UpdateScore(string name, int score, int coins) => db.Execute($"UPDATE Player SET score = {score}, coins = {coins} WHERE username = '{name}'");
    
    public class DBScore
    {
        public int id { get; set; }
        public string username { get; set; }
        public int score { get; set; }
        public int coins { get; set; }
    }
}
