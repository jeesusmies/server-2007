using server_2007.http;
using server_2007.mysql;
using System;

namespace server_2007
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer server = new HttpServer("http://127.0.0.1:80/", new SqlInit("", "", "", ""));
        }
    }
}
