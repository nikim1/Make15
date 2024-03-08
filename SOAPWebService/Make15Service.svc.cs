using Make15_WebService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;

namespace Make15Service
{

    #region DatabaseClass
    /// <summary>
    /// A singleton class that keeps all of the information for a game.
    /// </summary>
    public sealed class Database
    {
        #region Fields
        private static Database instance = null;
        private List<byte> untakenNumbers;
        public List<Player> players;
        #endregion

        #region Constructors
        //
        private Database()
        {
            players = new List<Player>(2);
            ResetNumbers();
        }
        #endregion

        #region Properties

        public List<byte> UntakenNumbers
        {
            get
            {
                List<byte> untaken = new List<byte>(untakenNumbers.Count);
                for (int i = 0; i < untakenNumbers.Count; i++)
                {
                    untaken.Add(untakenNumbers[i]);
                }
                return untaken;
            }
            set
            {
                if (value != null)
                {
                    untakenNumbers = value;
                }

            }
        }

        public static Database Instance
        {
            get
            {
                // lock (padlock)
                // {
                if (instance == null)
                {
                    instance = new Database();
                }
                return instance;
                // }
            }
        }
        #endregion

        #region ClassMethods

        public void ResetNumbers()
        {
            untakenNumbers = new List<byte>(9);
            for (byte i = 1; i <= 9; i++)
            {
                untakenNumbers.Add(i);//all starting values
            }

        }
        /// <summary>
        /// Finds and return a player object with given ID.
        /// </summary>

        public Player GetPlayer(string playerID)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Identifier == playerID)
                {
                    return players[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the other player
        /// </summary>
        public Player GetOther(string ID)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Identifier != ID)
                {
                    return players[i];
                }
            }
            return null;
        }

        public bool RemoveFromUntaken(byte number)
        {
            return untakenNumbers.Remove(number);
        }

        #endregion

    }
    // 
    #endregion
    //
    #region ServiceClass

    /// <summary>
    /// Web service class to communicate with Players.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant,//to prevent deadlocks
   InstanceContextMode = InstanceContextMode.Single)]
    public class Make15Service : IMake15Service
    {

        #region Fields
        Dictionary<string, IClient> users = new Dictionary<string, IClient>();//string is ID
        static readonly string systemName = "System";
        #endregion

        #region ClassMethods
        /// <summary>
        /// Creates a new player with given name and returns it's unique identifier.
        /// Checks for inactive players and remove them from the database and user connections.
        /// If there are 2 active players, no new game will be made but further new players won't be connected.
        /// </summary>
        public string MakePlayer(string name)
        {
            //pulsing,check for inactive users and remove them
            List<string> toRemove = new List<string>();
            foreach (var item in users)
            {
                try
                {
                    item.Value.IsActive();
                }
                catch (Exception)
                {
                    toRemove.Add(item.Key);
                    continue;
                }
            }
            //remove inactive
            foreach (var item in toRemove)
            {
                Database.Instance.players.Remove(Database.Instance.GetPlayer(item));//remove player
                users.Remove(item);//remove connection
            }
            //removing done
            if (users.Count >= 2)
            {//this service is currently restricted to one game of 2 players - do not register the next players!
                var conn = OperationContext.Current.GetCallbackChannel<IClient>();
                conn.ReceiveMsg(systemName, "Unfortunately, all game rooms are full. Please, try again later!");
                return "undef";
            }
            //make player
            bool isFirst = Database.Instance.players.Count == 0;
            Player newPlayer = new Player(name, isFirst);
            Database.Instance.players.Add(newPlayer);
            //register connection
            var connection = OperationContext.Current.GetCallbackChannel<IClient>();
            users[newPlayer.Identifier] = connection;
            connection.IsHisTurn(isFirst);
            //announce the connection
            foreach (var user in users)
            {
                if (user.Key != newPlayer.Identifier)
                {
                    user.Value.ReceiveMsg(systemName, string.Format("{0} has joined the game! You go first.", name));
                }
                else
                {
                    user.Value.ReceiveMsg(systemName, string.Format("{0} welcome! {1}",
                        name, (users.Count == 1 ? "Searching for an opponent..." : "Your opponent goes first.")));
                }

            }
            //
            if (Database.Instance.players.Count == 2)
            {
                SendNamesInfo();
            }
            //
            return newPlayer.Identifier;
        }

        /// <summary>
        /// Sends both players the name of their opponent 
        /// </summary>
        private void SendNamesInfo()
        {
            foreach (var conn in users)
            {//send other player's name
                try
                {
                    conn.Value.SetNamesWindow(Database.Instance.GetOther(conn.Key).Name);
                }
                catch (Exception)
                {
                    RemovePlayer(Database.Instance.GetPlayer(conn.Key).Identifier);
                    return;
                }
            }
        }
        /// <summary>
        /// Marks a number as taken by a player with given ID. Also checks for a winner.
        /// </summary>
        public void MarkAsTaken(byte number, string playerID)
        {
            Player targetPlayer = Database.Instance.GetPlayer(playerID);
            if (targetPlayer != null && targetPlayer.IsHisTurn && Database.Instance.players.Count >= 2)//if we are in a game 
            {
                targetPlayer.AddNewNumber(number);
                Database.Instance.RemoveFromUntaken(number);
                foreach (var item in users.Keys)
                {
                    users[item].RefreshStat();
                }
                targetPlayer.FlipTurn();
                try
                {
                    users[playerID]?.IsHisTurn(false);
                }
                catch (Exception)//lost connection
                {
                    RemovePlayer(playerID);
                    return;
                }
                if (targetPlayer.isWinner())
                {
                    foreach (var user in users)
                    {
                        if (user.Key == targetPlayer.Identifier)
                        {
                            try
                            {
                                user.Value.ReceiveMsg(systemName, string.Format("Congratulations,{0} you won! Outstanding game!",
                                    targetPlayer.Name));
                            }
                            catch (Exception)//lost connection
                            {
                                RemovePlayer(targetPlayer.Identifier);
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                user.Value.ReceiveMsg(systemName, string.Format("{0} is the winner! Don't give up, you'll get him next time!",
                                    targetPlayer.Name));
                            }
                            catch (Exception)//lost connection
                            {
                                RemovePlayer(user.Key);
                                return;
                            }
                        }
                    }
                    return;//end
                }
                else if (Database.Instance.UntakenNumbers.Count == 0) //we have no more choices
                {
                    foreach (var user in users)
                    {
                        try
                        {
                            user.Value.ReceiveMsg(systemName, "Draw! No more numbers left to pick.");
                        }
                        catch (Exception)//lost connection
                        {
                            RemovePlayer(user.Key);
                            return;
                        }
                        user.Value.IsHisTurn(false);
                    }
                    return;//end
                }
                Player other = Database.Instance.GetOther(playerID);
                try
                {
                    other.FlipTurn();//next player turn 
                    users[other.Identifier].IsHisTurn(true);
                }
                catch (Exception)//lost connection
                {
                    RemovePlayer(other.Identifier);
                    return;
                }
            }
        }
        /// <summary>
        /// Returns untaken numbers.
        /// </summary>
        public List<byte> GetUntaken()
        {
            return Database.Instance.UntakenNumbers;
        }
        /// <summary>
        /// Returns taken by player with given ID.
        /// </summary>

        public List<byte> GetTakenBy(string playerID)
        {
            return Database.Instance.GetPlayer(playerID)?.TakenNumList;
        }

        ///Returns opponents list
        public List<byte> GetTakenByOpponent(string playerID)
        {
            return Database.Instance.GetOther(playerID)?.TakenNumList;
        }
        /// <summary>
        /// Removes player with given ID from users connection and player database.
        /// </summary>
        public void RemovePlayer(string playerID)
        {
            string otherPlrName = "";
            for (int i = 0; i < Database.Instance.players.Count; i++)
            {
                if (playerID == Database.Instance.players[i].Identifier)
                {
                    otherPlrName = Database.Instance.players[i].Name;
                    users.Remove(playerID);
                    Database.Instance.players.RemoveAt(i);
                    break;
                }
            }
            if (Database.Instance.players.Count < 2)
            {
                Database.Instance.ResetNumbers();
                foreach (var player in Database.Instance.players)
                {//announce the other players for the dissconnection
                    try
                    {
                        users[player.Identifier]?.ReceiveMsg(systemName, $"{otherPlrName} has disconnected...");
                        users[player.Identifier]?.IsHisTurn(true);
                        users[player.Identifier]?.RefreshStat();
                    }
                    catch (Exception)//lost connection
                    {
                        RemovePlayer(player.Identifier);
                        return;
                    }
                    player.IsHisTurn = true;
                    player.ResetNum();
                }
            }
        }
        /// <summary>
        /// Chat function.
        /// </summary>
        public void SendMsgToOpponent(string playerID, string yourName, string msg)
        {
            if (Database.Instance.players.Count >= 2 && Database.Instance.GetPlayer(playerID) != null)
            {
                Player other = Database.Instance.GetOther(playerID);
                if (other != null)
                {
                    try
                    {
                        users[other.Identifier].ReceiveMsg(yourName, msg);
                    }
                    catch (Exception)//lost connection
                    {
                        RemovePlayer(playerID);
                        return;
                    }
                }
            }
        } 
        #endregion

    }

    #endregion
    //
    #region PlayerClass

    /// <summary>
    /// Keeps the information for a single player.
    /// </summary>
    public class Player
    {
        #region Fields

        private List<byte> takenNumList;//his chosen numbers
        private string ID;
        private static int counter = 0; //used for ID
        //
        private string name;
        private bool isHisTurn;
        // 
        #endregion

        #region Constructors
        public Player(string _name, bool hisTurn = false)
        {
            name = _name;
            takenNumList = new List<byte>(5);
            isHisTurn = hisTurn;
            ID = "Player_" + counter;
            ++counter;
        }

        #endregion

        #region Properties

        public List<byte> TakenNumList
        {
            get
            {
                List<byte> newList = new List<byte>(takenNumList.Count);
                for (int i = 0; i < takenNumList.Count; i++)
                {
                    newList.Add(takenNumList[i]);
                }
                return newList;
            }
        }

        public string Identifier
        {
            get
            {
                return ID;
            }
        }

        public bool IsHisTurn
        {
            get
            {
                return isHisTurn;
            }
            set
            {
                isHisTurn = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value ?? ID;
            }
        }

        #endregion

        #region ClassMethods
        public void ResetNum()
        {
            takenNumList = new List<byte>(5);
        }
        public void FlipTurn()
        {
            isHisTurn = !isHisTurn;
        }

        //Adds new number choice in the list
        public void AddNewNumber(byte newNum)
        {
            takenNumList.Add(newNum);//add to list
        }

        /// <summary>
        /// Returns if the player has the numbers to be a winner
        /// </summary>
        public bool isWinner()
        {//we will fix the last number as first as we assume that if new numbers have been added, 
         //the player was not a winner before that
            if (takenNumList.Count == 0) return false;
            //
            var first = takenNumList.Last();
            //
            for (var i = 0; i < takenNumList.Count - 2; i++)
            {//second number
                for (var j = i + 1; j < takenNumList.Count - 1; j++)
                {//third
                    if (first + takenNumList[i] + takenNumList[j] == 15)
                    {
                        return true;//winner!
                    }
                }
            }
            return false;

        } 
        #endregion

    }//player 
    #endregion

}//namespace
