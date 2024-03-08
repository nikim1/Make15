using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RESTService
{
    [DataContract]
    public class Answer
    {
        //Preparing the data to return . Data Contracts
        [DataMember]
        public string playerName { get; set; }

    }

    //and we can have a class that does stuff...-> plays the role of a database
    public partial class Answers
    {
        private static readonly Answers ans= new Answers();

        private Answers() { }

        public static Answers Instance
        {
            get { return ans; }
        }
        private static string dunno = "dunno";
        public static string Dunno { get { return dunno; } }
    }
}