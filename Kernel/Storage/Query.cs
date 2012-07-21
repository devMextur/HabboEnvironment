using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace HabboEnvironment_R3.Kernel.Storage
{
    public class Query
    {
        /// <summary>
        /// Command to use in the Stream.
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Parameters to invoke.
        /// </summary>
        public List<MySqlParameter> Parameters { get; private set; }

        /// <summary>
        /// The type of the result
        /// </summary>
        public QueryType OutType { get; private set; }

        /// <summary>
        /// Start the query.
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="OutType"> </param>
        public void Listen(string Command, QueryType OutType)
        {
            this.Command = Command;
            this.OutType = OutType;
            this.Parameters = new List<MySqlParameter>();
        }

        /// <summary>
        /// Pushes an parameter into the list.
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Item"></param>
        public void Push(string Name, object Item)
        {
            Parameters.Add(new MySqlParameter(string.Format("@{0}", Name), Item));
        }

        /// <summary>
        /// Releases all recources.
        /// </summary>
        public void Dispose()
        {
            this.Command = null;
            this.Parameters = null;
        }
    }
}
