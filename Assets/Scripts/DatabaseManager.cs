using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite;

public static class DatabaseManager
{
    private static SQLiteConnection db => new SQLiteConnection(Application.streamingAssetsPath + "/Database/InfiniteRunner.db");

    public static List<(string username, int score, int coins)> GetTopScores(int amount)
    {
        string query = $@"SELECT P.username AS username, S.score, S.coins
                      FROM Score S
                      JOIN Player P ON S.id_Player = P.id_Player
                      ORDER BY S.score DESC
                      LIMIT {amount}";

        return db.Query<(string username, int score, int coins)>(query);
    }


    public static void AddNewScore(string name, int score, int coins)
    {
        db.Execute($"INSERT INTO Player (username) VALUES ('{name}')");

        int playerId = db.Table<Player>().OrderByDescending(p => p.id_Player).First().id_Player;
        db.Execute($"INSERT INTO Score (score, coins, id_Player) VALUES ('{score}', '{coins}', '{playerId}')");
    }

    public class Player
    {
        public int id_Player { get; set; }
        public string username { get; set; }
    }

    public class Score
    {
        public int id_Score { get; set; }
        public int score { get; set; }
        public int coins { get; set; }
        public int id_Player { get; set; }
    }
}
