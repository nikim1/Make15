// Fig. 28.1: IWelcomeSOAPXMLService.cs
// WCF web service interface that returns a welcome message through SOAP 
// protocol and XML data format.
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Make15_WebService
{

    [ServiceContract(CallbackContract = typeof(IClient))]
    public interface IMake15Service
    {
        /// <summary>
        ///creates a new player object with given name.
        /// </summary>
        [OperationContract]
        string MakePlayer(string name);
        /// <summary>
        /// Indicated that player with that ID has made a number pick.
        /// </summary>
        [OperationContract]
        void MarkAsTaken(byte number, string playerID);
        /// <summary>
        /// Returns the available numbers.
        /// </summary>
        [OperationContract]
        List<byte> GetUntaken();
        /// <summary>
        /// Returns picked numbers by a player with given ID.
        /// </summary>
        [OperationContract]
        List<byte> GetTakenBy(string playerID);
        /// <summary>
        /// Returns picked numbers by a player with different than given ID.
        /// </summary>
        [OperationContract]
        List<byte> GetTakenByOpponent(string playerID);
        /// <summary>
        /// Implements chat service messege.
        /// </summary>
        [OperationContract]
        void SendMsgToOpponent(string playerID, string yourName,string msg);
        /// <summary>
        /// Removes data and connection for given player ID.
        /// </summary>
        [OperationContract]
        void RemovePlayer(string playerID);
       
    }

    [ServiceContract]
    public interface IClient
    {
        /// <summary>
        /// Send message to player from other user.
        /// </summary>
        [OperationContract]
        void ReceiveMsg(string user, string msg);
        /// <summary>
        /// Indicates that change has been made in server.
        /// </summary>
        [OperationContract]
        void RefreshStat();
        /// <summary>
        /// Pulsing to check if player is active. Otherwise player will be removed from data.
        /// </summary>
        [OperationContract]
        void IsActive();
        /// <summary>
        /// Indicates that player turn has started/ended.
        /// </summary>
        /// <param name="isMyTurn">If it's that player's turn</param>
        [OperationContract]
        void IsHisTurn(bool isMyTurn);
        /// <summary>
        /// Sends opponent player name whenever he's joined.
        /// </summary>
        [OperationContract]
        void SetNamesWindow(string otherPlayerName);
    }
}
