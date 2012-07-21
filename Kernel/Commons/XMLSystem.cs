using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HabboEnvironment_R3.Kernel.Commons
{
    public class XMLSystem
    {
        public IReadOnlyDictionary<string, object> Settings
        {
            get;
            private set;
        }

        public IReadOnlyDictionary<Type, MethodInfo> Convertors
        {
            get;
            private set;
        }

        public void Serialize(string XMLConfigPath)
        {
            Dictionary<string, object> Items = new Dictionary<string, object>();

            foreach (string Line in File.ReadAllLines(XMLConfigPath))
            {
                var Split = Line.Split('<', '>');

                if (!Line.StartsWith("<") && !Line.EndsWith(">") && Split.Count() != 5)
                {
                    continue;
                }

                Items.Add(Split[1], Split[2]);
            }

            Settings = new Dictionary<string, object>(Items);

            Dictionary<Type, MethodInfo> ConvertorItems = new Dictionary<Type, MethodInfo>();

            foreach (MethodInfo Method in typeof(Convertors).GetMethods())
            {
                if (Method.IsStatic)
                {
                    ConvertorItems.Add(Method.ReturnType, Method);
                }
            }

            Convertors = new Dictionary<Type, MethodInfo>(ConvertorItems);
        }

        public bool TryPop<T>(string Key, out T Output)
        {
            Output = default(T);

            object Value;

            Settings.TryGetValue(Key, out Value);

            if (Value != null)
            {
                if (Convertors.ContainsKey(typeof(T)))
                {
                    Output = (T)Convertors[typeof(T)].Invoke(null, new object[] { Value });
                }
            }

            return !Output.Equals(default(T));
        }
    }

    static class Convertors
    {
        public static string ConvertString(object Input)
        {
            return Input.ToString();
        }

        public static int ConvertInteger(object Input)
        {
            int Output = default(int);
            int.TryParse(Input.ToString(), out Output);
            return Output;
        }

        public static bool ConvertBoolean(object Input)
        {
            bool Output = default(bool);
            bool.TryParse(Input.ToString(), out Output);
            return Output;
        }

        public static IPAddress ConvertIPAddress(object Input)
        {
            IPAddress Output = default(IPAddress);
            IPAddress.TryParse(Input.ToString(), out Output);
            return Output;
        }
    }
}
