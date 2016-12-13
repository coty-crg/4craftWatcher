using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using System.IO;

namespace _4craftThreadWatcher
{
    public class MongoHelper 
    {
        public static MongoHelper Instance; // singleton reference 

        public MongoClient Client;
        public IMongoDatabase Database;
        public IMongoCollection<BsonDocument> Threads;

        public MongoHelper()
        {
            Instance = this;

            // connection string contains username/password for the database 
            // looks like: mongodb://user:pass@ipaddress:port
            var connectionString = System.IO.File.ReadAllText(System.Environment.CurrentDirectory + @"/connection-string.txt");
            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase("WanderingCorgi");
            Threads = Database.GetCollection<BsonDocument>("Threads");

            // custom indexing 
            var ThreadNumber = Builders<BsonDocument>.IndexKeys.Ascending("ThreadNumber");
            var BoardCode = Builders<BsonDocument>.IndexKeys.Ascending("BoardCode");
            var Name = Builders<BsonDocument>.IndexKeys.Ascending("Name");

            Console.WriteLine("Initialized connection with local MongoDB server.");
        }
        
        public bool AddThread(Thread thread)
        {
            var threadDoc = thread.ToBsonDocument();
            Threads.InsertOne(threadDoc);
            return true;
        }

        public long GetThreadCount()
        {
            var filter = new BsonDocument();
            var totalResults = Threads.Find(filter).Count();
            return totalResults;
        }

        public bool ThreadExists(long postNumber)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("ThreadNumber", postNumber);
            var count = Threads.Find(filter).Count();
            return count > 0;
        }

        public List<Thread> GetAllThreads()
        {
            var filter = new BsonDocument();
            var firstResult = Threads.Find(filter);
            var list = firstResult.ToList();

            var finalList = new List<Thread>();
            foreach (var item in list)
            {
                var thread = BsonSerializer.Deserialize<Thread>(item);
                finalList.Add(thread);
            }

            return finalList;
        }
    }

    [BsonDiscriminator("Thread")]
    public class Thread
    {
        [BsonId] public ObjectId Id;
        public long ThreadNumber;
        public string BoardCode;
        public string Name;
        public int UniqueIPs;
        public int Replies;
        public int Images;
        public DateTime Time;
        public string Now;

        public Thread()
        {

        }
        
        public Thread(JSONObject json, string boardCode)
        {
            ThreadNumber = json.GetField("no").i;
            BoardCode = boardCode;
            Name = json.GetField("name").str;
            Time = new DateTime(json.GetField("time").i);
            Now = json.GetField("now").str;

            if (json.HasField("unique_ips"))
                UniqueIPs = (int) json.GetField("unique_ips").i;

            if (json.HasField("replies"))
                Replies = (int)json.GetField("replies").i;

            if (json.HasField("images"))
                Images = (int)json.GetField("images").i;
        }

        public JSONObject GetJson()
        {
            var json = new JSONObject();
            json.AddField("no", ThreadNumber);
            json.AddField("board", BoardCode); 
            json.AddField("name", Name);
            json.AddField("time", Time.Millisecond);
            json.AddField("now", Now);
            json.AddField("unique_ips", UniqueIPs);
            json.AddField("replies", Replies);
            json.AddField("images", Images);
            return json; 
        }
    }
}
