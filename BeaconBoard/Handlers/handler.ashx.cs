using System;
using System.Web;
using Newtonsoft.Json.Linq;
using WebMatrix.Data;
using BeaconBoard.CoreClasses;

namespace BeaconBoard.handlers
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class Handler1 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {

            string schema = BBCore.currentSchema;
            JObject jResult = new JObject();
            try
            {
                switch (context.Request["requestType"])
                {
                    case "ListUsers":
                        using (Database db = Database.Open("Beacon"))
                        {
                            JArray jaUsers = new JArray();
                            foreach (var row in db.Query($"EXEC {schema}.usp_ListUsers"))
                            {
                                JObject jUser = new JObject();
                                jUser.Add("ID", row["user_ID"]);
                                jUser.Add("userName", row["user_name"]);
                                jaUsers.Add(jUser);
                            }
                            jResult.Add("status", "OK");
                            jResult.Add("data", jaUsers);
                        }
                        break;
                    case "ListThreads":
                        using (Database db = Database.Open("Beacon"))
                        {
                            JArray jaThreads = new JArray();
                            foreach (var row in db.Query($"EXEC {schema}.usp_ListThreads"))
                            {
                                JObject jThread = new JObject();
                                jThread.Add("ID", row["thread_ID"]);
                                jThread.Add("author", row["user_name"]);
                                jThread.Add("threadTitle", row["thread_title"]);
                                jThread.Add("createdDate", row["created_date"]);
                                jThread.Add("modifiedDate", row["modified_date"]);
                                jaThreads.Add(jThread);
                            }
                            jResult.Add("status", "OK");
                            jResult.Add("data", jaThreads);
                        }
                        break;
                    case "ListPosts":
                        using (Database db = Database.Open("Beacon"))
                        {
                            int threadID = Convert.ToInt32(context.Request["threadID"]);
                            JArray jaThreads = new JArray();
                            foreach (var row in db.Query($"EXEC {schema}.usp_ListPosts @0", threadID))
                            {
                                JObject jThread = new JObject();
                                jThread.Add("ID", row["post_ID"]);
                                jThread.Add("author", row["user_name"]);
                                jThread.Add("threadTitle", row["thread_title"]);
                                jThread.Add("postContent", row["post_content"]);
                                jThread.Add("createdDate", row["created_date"]);
                                jThread.Add("modifiedDate", row["modified_date"]);
                                jaThreads.Add(jThread);
                            }
                            jResult.Add("status", "OK");
                            jResult.Add("data", jaThreads);
                        }
                        break;
                    case "OpenThread":
                        using (Database db = Database.Open("Beacon"))
                        {
                            string threadTitle = context.Request["threadTitle"];
                            string firstPostContent = context.Request["firstPostContent"];
                            if (String.IsNullOrWhiteSpace(threadTitle) || String.IsNullOrWhiteSpace(firstPostContent))
                            {
                                jResult.Add("status", "ERROR");
                                jResult.Add("data", "Input invalid.");
                            }
                            else
                            {
                                db.Execute($"EXEC {schema}.usp_OpenThread @0, @1, @2", BBCore.currentUserID, threadTitle, firstPostContent);
                                jResult.Add("status", "OK");
                                jResult.Add("data", null);
                            }
                        }
                        break;
                    case "ReplyThread":
                        using (Database db = Database.Open("Beacon"))
                        {
                            string threadID = context.Request["threadID"];
                            string postContent = context.Request["postContent"];
                            if (String.IsNullOrWhiteSpace(postContent))
                            {
                                jResult.Add("status", "ERROR");
                                jResult.Add("data", "Input invalid.");
                            }
                            else
                            {
                                db.Execute($"EXEC {schema}.usp_ReplyThread @0, @1, @2", BBCore.currentUserID, threadID, postContent);
                                jResult.Add("status", "OK");
                                jResult.Add("data", null);
                            }
                        }
                        break;
                    case "EditPost":
                        using (Database db = Database.Open("Beacon"))
                        {
                            string postID = context.Request["postID"];
                            string postContent = context.Request["postContent"];
                            if (String.IsNullOrWhiteSpace(postContent))
                            {
                                jResult.Add("status", "ERROR");
                                jResult.Add("data", "Input invalid.");
                            }
                            else
                            {
                                db.Execute($"EXEC {schema}.usp_EditPost @0, @1, @2", BBCore.currentUserID, postID, postContent);
                                jResult.Add("status", "OK");
                                jResult.Add("data", null);
                            }
                        }
                        break;
                    case "DeletePost":
                        using (Database db = Database.Open("Beacon"))
                        {
                            string postID = context.Request["postID"];
                            db.Execute($"EXEC {schema}.usp_DeletePost @0, @1", BBCore.currentUserID, postID);
                            jResult.Add("status", "OK");
                            jResult.Add("data", null);
                        }
                        break;
                    case "DeleteThread":
                        using (Database db = Database.Open("Beacon"))
                        {
                            string threadID = context.Request["threadID"];
                            db.Execute($"EXEC {schema}.usp_DeleteThread @0, @1", BBCore.currentUserID, threadID);
                            jResult.Add("status", "OK");
                            jResult.Add("data", null);
                        }
                        break;
                    default:
                        {
                            jResult.Add("status", "ERROR");
                            jResult.Add("data", $"Unknown request type {context.Request["requestType"]}.");
                        }
                        break;
                }
                context.Response.ContentType = "application/json";
                context.Response.Write(jResult);
            }
            catch (Exception ex)
            {
                jResult.Add("status", "ERROR");
                jResult.Add("data", "Unhandled exception thrown from handler: " + ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.Write(jResult);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
