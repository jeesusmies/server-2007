﻿using MySql.Data.MySqlClient;
using server_2007.mysql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
// mysql -u jeesus -p  server2007 < osu.sql
namespace server_2007.http
{
    class HttpServer
    {
        private string _url;
        private SqlInit _sql;
        private static HttpListener _HttpListener;

        public HttpServer(string url, SqlInit sql) {
            _url = url;
            _sql = sql;

            _HttpListener = new HttpListener(); 
            _HttpListener.Prefixes.Add(url);
            _HttpListener.Start();

            Thread thr = new Thread(ListenForConnections);
            thr.Start();
        }

        public void ListenForConnections() {
            Console.WriteLine("SERVER!2007: RUNNING");

            while (true) {
                HttpListenerContext context = _HttpListener.GetContext();

                if (context.Request.RemoteEndPoint != null) {
                    byte[] response;
                    MySqlConnection con = new MySqlConnection(_sql.ConnectionString());
                    con.Open();

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = con;

                    Console.WriteLine("{0} from {1}: {2}", context.Request.HttpMethod, context.Request.RemoteEndPoint.Address, context.Request.RawUrl);
                    Uri uri = new Uri("http://127.0.0.1" + context.Request.RawUrl);

                    switch (context.Request.Url.AbsolutePath) {
                        case "/web/osu-login.php":             

                            response = Encoding.UTF8.GetBytes("1");

                            context.Response.OutputStream.Write(response, 0, response.Length);
                            context.Response.Close();

                            break;

                        //   /web/osu-getscores.php?c=6c68e827d9786e9d7d1063d192a7ca15
                        case "/web/osu-getscores.php":
                            string text = "";
                            var users = new List<string>();

                            string mapHash = HttpUtility.ParseQueryString(uri.Query).Get("c");

                            var cmd2 = new MySqlCommand(String.Format("SELECT * from osu_scores WHERE osuhash = '{0}' and pass = True ORDER BY score DESC;", mapHash), con);
                            MySqlDataReader rdr = cmd2.ExecuteReader();

                            int c = 0;
                            while (rdr.Read()) {
                                if (users.FirstOrDefault(x => x.Contains(rdr.GetString(3))) == null) {
                                    text += String.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:{11}\n", rdr.GetString(0), rdr.GetString(3), rdr.GetString(11), rdr.GetString(12), rdr.GetString(7), rdr.GetString(6), rdr.GetString(5), rdr.GetString(10), rdr.GetString(9), rdr.GetString(8), rdr.GetBoolean(13), rdr.GetString(15));
                                    users.Add(rdr.GetString(3));
                                }
                                c++;
                            }
                                
                            
                            response = Encoding.UTF8.GetBytes(text);

                            context.Response.OutputStream.Write(response, 0, response.Length);
                            context.Response.Close();

                            break;

                        //   /web/osu-submit.php?score=6c68e827d9786e9d7d1063d192a7ca15:sdfsf:68c6b1d1013c9182f630ed869612cb59:5:5:0:0:1:0:3240:10:True:D:0:True&pass=d58e3582afa99040e27b92b13c8f2280
                        case "/web/osu-submit.php":
                            string[] ParsedScore = HttpUtility.ParseQueryString(uri.Query).Get("score").Split(":");

                            cmd.CommandText = String.Format("INSERT INTO osu_scores (scoreid, replay, osuhash, username, submithash, hit300, hit100, hit50, hitgeki, hitkatu, hitmiss, score, combo, perfect, grade, mods, pass) VALUES (0, '0', '{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', {9}, '{10}', {11}, '{12}', '{13}', {14})", ParsedScore[0], ParsedScore[1], ParsedScore[2], ParsedScore[3], ParsedScore[4], ParsedScore[5], ParsedScore[6], ParsedScore[7], ParsedScore[8], ParsedScore[9], ParsedScore[10], ParsedScore[11], ParsedScore[12], ParsedScore[13], ParsedScore[14]);
                            cmd.ExecuteNonQuery();

                            break;

                        case "/web/osu-getreplay.php":
                            
                            break;
                    }
                }
                Thread.Sleep(1);
            }
        }
    }
}
