using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace HabboEnvironment_R3.Kernel.Storage
{
    public class MySQLManager
    {
        /// <summary>
        /// String to activate Streams with.
        /// </summary>
       public string QueryHandlerString
       {
           get;
           private set;
       }

        public MySQLManager(string Host, uint Port, string Username, string Password,
            string Database, bool Pooling, uint MinimumPoolSize, uint MaximumPoolSize)
        {
            MySqlConnectionStringBuilder sb = new MySqlConnectionStringBuilder();
            sb.Server = Host;
            sb.Port = Port;
            sb.UserID = Username;
            sb.Password = Password;
            sb.Database = Database;
            sb.Pooling = Pooling;
            sb.MinimumPoolSize = MinimumPoolSize;
            sb.MaximumPoolSize = MaximumPoolSize;

            this.QueryHandlerString = sb.ConnectionString;
        }

        /// <summary>
        /// Invokes the query, there is no output.
        /// </summary>
        /// <param name="Query"></param>
        public void InvokeQuery(Query Query)
        {
            GetObject(Query);
        }

        /// <summary>
        /// Returns an quick Obj.
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public QueryObject GetObject(Query Query)
        {
            using (var Stream = new QueryStream())
            {
                Stream.Push(Query);
                return Stream.Pop(this);
            }
        }

        /// <summary>
        /// Returns an quick stack.
        /// </summary>
        /// <param name="Querys"></param>
        /// <returns></returns>
        public Stack<QueryObject> GetObjects(params Query[] Querys)
        {
            Stack<QueryObject> Stack = new Stack<QueryObject>();

            using (var Stream = new QueryStream())
            {
                foreach (var Query in Querys)
                {
                    Stream.Push(Query);
                }

                for (int i = 0; i <= Stream.Querys.Count; i++)
                {
                    Stack.Push(Stream.Pop(this));
                }
            }

            return Stack;
        }
    }
}
