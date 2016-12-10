﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace _4craftThreadWatcher
{
    class Program
    {
        public delegate void CommandFunc(params string[] args);
        private static Dictionary<string, CommandFunc> Commands = new Dictionary<string, CommandFunc>();
        public static bool KeepListening = true;
        public static string CachedResult = "[]";

        static void Main(string[] args)
        {
            WebStuff.SetupWebStuff();

            var scanThread = new Thread(HandleScanning);
            scanThread.Start(); 

            var listenerThread = new Thread(() => SimpleListenerExample("http://127.0.0.1:4000/")); 
            listenerThread.Start();
            
            // commands 
            Commands.Add("help", (arguments) =>
            {
                var sb = new StringBuilder();
                foreach (var command in Commands)
                    sb.Append(string.Format("\n{0}", command.Key));
                Console.WriteLine("Available commands: {0}", sb.ToString());
                Console.WriteLine("Enter 'exit' to close the program");
            });

            Commands.Add("rescan", (arguments) =>
            {
                Console.WriteLine("Forcing rescan.");
                scanThread.Interrupt(); 
            });

            // read from cli 
            while (true)
            {
                System.Threading.Thread.Sleep(1000 / 25);
                string input;

                try
                {
                    input = Console.ReadLine();
                } catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine(e.ToString());
                    continue;
                }

                if (input.IndexOf("exit") > -1)
                    break;

                var arguments = input.Split(' ');
                if (arguments.Length > 0)
                {
                    var command = arguments[0];
                    CommandFunc Command;
                    var found = Commands.TryGetValue(command, out Command);
                    if (!found) continue;

                    try
                    {
                        Command.Invoke(arguments);
                    } catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }

                }

            }

            KeepListening = false;
        }

        public static void SimpleListenerExample(params string[] prefixes)
        {
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");
            
            HttpListener listener = new HttpListener();
            foreach (string s in prefixes)
                listener.Prefixes.Add(s);

            listener.Start();
            Console.WriteLine("Listening...");

            while (KeepListening)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;

                response.AddHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
                response.AddHeader("Access-Control-Allow-Methods", "GET"); 
                
                var responseString = CachedResult; 
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            
                response.ContentLength64 = buffer.Length;
                var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            
            listener.Stop();
        }
        
        public static void HandleScanning()
        {
            while (KeepListening)
            {
                try
                {
                    var boardString = WebStuff.FetchDataFromURLBlocking("https://a.4cdn.org/boards.json");
                    Thread.Sleep(10);

                    var boardData = new JSONObject(boardString);
                    var boardArr = boardData.GetField("boards").list;
                    var searchTerm = "4craft";
                    var foundThreads = new JSONObject(JSONObject.Type.ARRAY);

                    foreach (var board in boardArr)
                    {
                        var boardCode = board.GetField("board").str;
                        var pages = (int)board.GetField("pages").i;

                        Console.WriteLine(string.Format("Scanning /{0}/", boardCode));

                        for (var i = 1; i < pages; ++i)
                        {
                            var pageUrl = string.Format("https://a.4cdn.org/{0}/{1}.json", boardCode, i);
                            var catalogString = WebStuff.FetchDataFromURLBlocking(pageUrl);
                            var catalogJson = new JSONObject(catalogString);
                            var catalogThreads = catalogJson.GetField("threads").list;

                            Console.WriteLine(string.Format("Scanning /{0}/ - {1}%", boardCode, ((float)i / (float)pages) * 100));

                            foreach (var thread in catalogThreads)
                            {
                                var posts = thread.GetField("posts").list;
                                if (posts.Count == 0)
                                    continue;

                                var post = posts[0];
                                if (!post.HasField("com"))
                                    continue;

                                var firstComment = post.GetField("com").str;
                                if (firstComment.Contains(searchTerm))
                                {
                                    post.AddField("board", boardCode); 
                                    foundThreads.Add(post);
                                    continue;
                                }
                            }
                        }
                    }

                    Console.WriteLine("Returning fresh result!");
                    CachedResult = foundThreads.ToString();
                } catch (Exception e)
                {
                    Console.WriteLine("Scan failed? Trying again in a sec.");
                    Thread.Sleep(500); 
                    continue; 
                }

                try
                {
                    Thread.Sleep(1000 * 60 * 5); // scan once every 5 minutes 
                } catch(ThreadInterruptedException e)
                {
                    Console.WriteLine("Scan thread woken up!"); 
                }
            }
        }
    }
}
