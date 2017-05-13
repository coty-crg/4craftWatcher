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
        public IMongoCollection<BsonDocument> VillagerComments;
        public IMongoCollection<BsonDocument> DiscordAttachments;

        public MongoHelper()
        {
            Instance = this;

            // connection string contains username/password for the database 
            // looks like: mongodb://user:pass@ipaddress:port
            var connectionString = System.IO.File.ReadAllText(System.Environment.CurrentDirectory + @"/connection-string.txt");
            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase("WanderingCorgi");
            Threads = Database.GetCollection<BsonDocument>("Threads");
            VillagerComments = Database.GetCollection<BsonDocument>("VillagerComments");
            DiscordAttachments = Database.GetCollection<BsonDocument>("DiscordAttachments"); 

            // custom indexing 
            var ThreadNumber = Builders<BsonDocument>.IndexKeys.Ascending("ThreadNumber");
            var BoardCode = Builders<BsonDocument>.IndexKeys.Ascending("BoardCode");
            var Name = Builders<BsonDocument>.IndexKeys.Ascending("Name");

            Console.WriteLine("Initialized connection with local MongoDB server.");
        }
        
        public bool HasMessage(VillagerComment comment)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("Message", comment.Message);
            return VillagerComments.Find(filter).Count() > 0; 
        }

        public bool AddMessage(VillagerComment comment)
        {
            if (HasMessage(comment))
                return false; 

            var commentDoc = comment.ToBsonDocument();
            VillagerComments.InsertOne(commentDoc);
            return true; 
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

        public List<VillagerComment> GetAllComments()
        {
            var filter = new BsonDocument();
            var firstResult = VillagerComments.Find(filter);
            var list = firstResult.ToList();

            var finalList = new List<VillagerComment>();
            foreach (var item in list)
            {
                var comment = BsonSerializer.Deserialize<VillagerComment>(item);
                finalList.Add(comment);
            }

            return finalList;
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

        // generics  
        public List<T> GetAll<T>(IMongoCollection<BsonDocument> documents) where T : class
        {
            var filter = new BsonDocument();
            var firstResult = documents.Find(filter);
            var list = firstResult.ToList();

            var finalList = new List<T>();
            foreach (var item in list)
            {
                var instance = BsonSerializer.Deserialize<T>(item);
                finalList.Add(instance);
            }

            return finalList;
        }

        public T Get<T>(IMongoCollection<BsonDocument> documents, string id) where T : class
        {
            if (!Exists(documents, id))
                return null;

            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var firstResult = documents.Find(filter).First();
            var item = BsonSerializer.Deserialize<T>(firstResult);
            return item;
        }

        public T GetKeyValue<T>(IMongoCollection<BsonDocument> documents, string key, string value) where T : class
        {
            if (!ExistsKeyValue(documents, key, value))
                return null;

            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            var firstResult = documents.Find(filter).First();
            var item = BsonSerializer.Deserialize<T>(firstResult);
            return item;
        }

        public bool Exists(IMongoCollection<BsonDocument> documents, string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var count = documents.Find(filter).Count();
            return count > 0;
        }

        public bool ExistsKeyValue(IMongoCollection<BsonDocument> documents, string key, string value)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(key, value);
            var count = documents.Find(filter).Count();
            return count > 0;
        }

        private bool Create<T>(IMongoCollection<BsonDocument> documents, T item) where T : class
        {
            var document = item.ToBsonDocument();
            documents.InsertOne(document);
            return true;
        }

        public bool Put<T>(IMongoCollection<BsonDocument> documents, T item, string id) where T : class
        {
            if (!Exists(documents, id))
                return Create<T>(documents, item);

            var document = item.ToBsonDocument();
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var result = documents.ReplaceOne(filter, document);
            return result.MatchedCount >= 1 || result.ModifiedCount > 0;
        }
    }


    [BsonDiscriminator("DiscordAttachment")]
    public class DiscordAttachment
    {
        [BsonId] public string id;
        public string url;
        public string proxy_url;
        public string filename;
        public int width;
        public int height;
        public int size;

        public string authorId;
        public string authorUsername;
        public string content; 

        public DiscordAttachment(JSONObject serialized, string authorId, string authorUsername, string content)
        {
            id = serialized.GetField("id").str;
            url = serialized.GetField("url").str;
            proxy_url = serialized.GetField("proxy_url").str;
            filename = serialized.GetField("filename").str;
            width = (int)serialized.GetField("width").i;
            height = (int)serialized.GetField("height").i;
            size = (int)serialized.GetField("size").i;

            this.authorId = authorId;
            this.authorUsername = authorUsername;
            this.content = content; 
        }
    }

    [BsonDiscriminator("VillagerComment")]
    public class VillagerComment
    {
        [BsonId] public ObjectId Id;
        public string Message; 

        public VillagerComment(string message)
        {
            if (message.Length > 255)
                message = message.Substring(0, 255);

            Message = message; 
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
