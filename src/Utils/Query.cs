using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using BlackMagic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Utils
{

    public class Credentials
    {
        public static readonly string Host = "localhost";
        public static readonly string Database = "system";
        public static readonly string User = "root";
        public static readonly string Password = "";
    }

    //Made by: Ricardo Pieper & SI 2012
    public class Query
    {

        private string sql { get; set; }

        public Query() { }
        public Query(string sql)
        {
            this.sql = sql;
        }

        private string m_connectionString
        {
            get
            {
                string cs = "Server=" + Credentials.Host + ";Database=" + Credentials.Database + ";Uid=" + Credentials.User + ";";
                if (!String.IsNullOrEmpty(Credentials.Password))
                {
                    cs += "Pwd=" + System.Text.ASCIIEncoding.ASCII.GetString(System.Text.ASCIIEncoding.ASCII.GetBytes(Credentials.Password)) + ";";
                }

                return cs;
            }
        }

        public MySqlConnection Connection { get; set; }


        private void openConnection()
        {
            if (Connection.State != ConnectionState.Open)
            {
                try
                {
                    Connection.Open();
                }
                catch (MySqlException)
                {
                    //TODO: Implementar tratamento
                    throw;
                }
            }

        }


        private void closeConnection()
        {
            if (Connection.State != ConnectionState.Closed)
            {

                try
                {
                    Connection.Close();
                    Connection.Dispose();
                }
                catch (MySqlException)
                {
                    //TODO: Implementar tratamento
                    throw;
                }

            }

        }

        private List<MySqlParameter> Parameters = new List<MySqlParameter>();
            
            public void RemoveAllParameters()
        {
            Parameters.Clear();
        }
        public Query AddParameter(string name, object value)
        {
            if (name == null) throw new ArgumentNullException("name");
            Parameters.Add(new MySqlParameter(name, value));
            return this;
        }

        public IEnumerable<QueryResult> ExecuteQuery(string sql)
        {

            Connection = new MySqlConnection(this.m_connectionString);

            openConnection();

            using (MySqlCommand command = new MySqlCommand(sql, Connection))
            {
                if (Parameters != null && Parameters.Count > 0)
                {
                    foreach (var param in Parameters)
                    {
                        command.Parameters.Add(param);
                    }
                }
                using (DataTable table = new DataTable())
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(table);
                    }
                    closeConnection();

                    foreach (DataRow row in table.Rows)
                    {
                        QueryResult qrRow = new QueryResult();

                        foreach (DataColumn column in table.Columns)
                        {
                            qrRow.Add(column.ColumnName, row[column]);
                        }

                        yield return qrRow;
                    }

                }
            }
        }

        public IEnumerable<QueryResult> ExecuteQuery()
        {
            return ExecuteQuery(this.sql);
        }

        public static int Insert<T>(T obj)
        {
            Reflector<T> reflect = new Reflector<T>();

            Dictionary<string, object> values = new Dictionary<string, object>();
            AutoIncrement autoinc = new AutoIncrement(reflect.CurrentType.Name);

            foreach (var prop in reflect.Properties)
            {
                var attributes = prop.GetCustomAttributes(true);
                bool attributeIsKey = false;
                object value = null;

                string attributeName = prop.Name;
                /*
                 * Precedência: Key > Column* 
                */
                foreach (object attr in attributes)
                {
                    KeyAttribute key = attr as KeyAttribute;

                    if (key != null)
                    {
                        attributeIsKey = true;
                    }
                    else
                    {
                        ColumnAttribute col = attr as ColumnAttribute;

                        if (col != null)
                        {
                            attributeName = col.Name;
                        }
                    }
                }

                if (attributeIsKey)
                {
                    //TODO: Adicionar controle para que quando for default executar o autoinc, caso contrário adiciona como key
                //    value = autoinc.AddKey(attributeName, 0);
                }
                else
                {
                    value = reflect[prop].Getter(obj);
                }
                values.Add(attributeName, value);
            }


            string tableName = reflect.CurrentType.Name;

            var classAttributes = reflect.CurrentType.GetCustomAttributes(true);

            foreach (var attr in classAttributes)
            {
                TableAttribute tableAttr;
                if ((tableAttr = attr as TableAttribute) != null)
                {
                    tableName = tableAttr.Name;
                    break;
                }
            }

          string fields = string.Join(",", values.Keys);

          Query q  = new Query();
          foreach (var pair in values){
              q.AddParameter(pair.Key, pair.Value);
          }

          return q.ExecuteNonQuery("insert into "+tableName+"("+ string.Join(",", values.Keys)+") values ("+ string.Join(",@", values.Keys)+");");

        }

        private static object defaultValue(Type t)
        {
            var def = typeof(Query).GetMethod("defaultGeneric", System.Reflection.BindingFlags.NonPublic)
                   .MakeGenericMethod(t).Invoke(null, null);
            return def;
        }

        private static TDefault defaultGeneric<TDefault>()
        {
            return default(TDefault);
        }


        public enum UpdateType
        {
            ChangeEverything, ChangeNonDefault
        }

        public static int Update<T>(T obj)
        {
            return Update(obj, UpdateType.ChangeNonDefault);
        }

        public static int Update<T>(T obj, UpdateType updateType)
        {
            Reflector<T> reflect = new Reflector<T>();

            Dictionary<string, object> values = new Dictionary<string, object>();
            Dictionary<string, object> keys = new Dictionary<string, object>();

            foreach (var prop in reflect.Properties)
            {
                var attributes = prop.GetCustomAttributes(true);
                bool attributeIsKey = false;
                object value = null;

                string attributeName = prop.Name;
                /*
                 * Precedência: Key > Column* 
                */
                foreach (object attr in attributes)
                {
                    KeyAttribute key = attr as KeyAttribute;

                    if (key != null)
                    {
                        attributeIsKey = true;
                    }
                    else
                    {
                        ColumnAttribute col = attr as ColumnAttribute;

                        if (col != null)
                        {
                            attributeName = col.Name;
                        }
                    }
                }
                value = reflect[prop].Getter(obj);
                if (value != defaultValue(reflect[prop].Property.PropertyType) || updateType == UpdateType.ChangeEverything)
                {
                    if (attributeIsKey)
                    {
                        keys.Add(attributeName, value);
                    }
                    else
                    {
                        values.Add(attributeName, value);
                    }
                }
               
            }


            string tableName = reflect.CurrentType.Name;

            var classAttributes = reflect.CurrentType.GetCustomAttributes(true);

            foreach (var attr in classAttributes)
            {
                TableAttribute tableAttr;
                if ((tableAttr = attr as TableAttribute) != null)
                {
                    tableName = tableAttr.Name;
                    break;
                }
            }

            string fields = string.Join(",", values.Keys);

            Query q = new Query();
            foreach (var pair in values)
            {
                q.AddParameter(pair.Key, pair.Value);
            }

            foreach (var pair in keys)
            {
                q.AddParameter(pair.Key, pair.Value);
            }



            return q.ExecuteNonQuery("update "
                + tableName + 
                " set " + 
                string.Join(",", values.Select(x => x.Key + "=@" + x.Key)) +
                " where " + string.Join(" and ", keys.Select(x => x.Key + "=@" + x.Key)));




        }
      
        public int ExecuteNonQuery(string sql)
        {

            Connection = new MySqlConnection(this.m_connectionString);

            openConnection();

            using (MySqlCommand command = new MySqlCommand(sql, Connection))
            {
                if (Parameters != null && Parameters.Count > 0)
                {
                    foreach (var param in Parameters)
                    {
                        command.Parameters.Add(param);
                    }

                }
                
               return command.ExecuteNonQuery();

            }
        }


    }
    public class QueryResult : Dictionary<string, object>
    {

        public T As<T>(string name)
        {
            return (T)Convert.ChangeType(this[name], typeof(T));
        }

        public String String(string name)
        {
            return (string)this[name];
        }
        public Int16 Int64(string name)
        {
            return (Int16)this[name];
        }
        public Int32 Int32(string name)
        {
            return (Int32)this[name];
        }
        public Int16 Int16(string name)
        {
            return (Int16)this[name];
        }
        public UInt16 UInt16(string name)
        {
            return (UInt16)this[name];
        }
        public UInt32 UInt32(string name)
        {
            return (UInt32)this[name];
        }
        public UInt64 UInt64(string name)
        {
            return (UInt64)this[name];
        }
        public Decimal Decimal(string name)
        {
            return (Decimal)this[name];
        }
        public Byte[] Bytes(string name)
        {
            return (byte[])this[name];
        }
        public Byte Byte(string name)
        {
            return (Byte)this[name];
        }
        public SByte SByte(string name)
        {
            return (SByte)this[name];
        }
        public Object Object(string name)
        {
            return this[name];
        }
        public Boolean Boolean(string name)
        {
            return (bool)this[name];
        }
        public DateTime DateTime(string name)
        {
            return (DateTime)this[name];
        }
        public TimeSpan TimeSpan(string name)
        {
            return (TimeSpan)this[name];
        }

    }

    public class AutoIncrement
    {
        public Dictionary<string, object> Keys { get; set; }
        public String TableName { get; set; }

        public AutoIncrement(string tableName) { this.TableName = tableName; }

        public AutoIncrement AddKey(string field, object value)
        {

            if (Keys != null)
            {
                Keys = new Dictionary<string, object>();
            }

            if (Keys.ContainsKey(field))
            {
                Keys[field] = value;
            }
            else
            {
                Keys.Add(field, value);
            }

            return this;
        }

        public long GetKey()
        {

            string sql = "select "+Keys.First().Key+" from "+TableName+ " where "+
                string.Join(" and ", 
                    Keys.Select(x=> x.Key+ "=@"+x.Key)
                );

            Query q = new Query(sql);

            foreach (var pair in Keys)
            {
                q.AddParameter(pair.Key, pair.Value);
            }

            return q.ExecuteQuery().MapTo(x=> x.Int64(Keys.First().Key)).First();

        }







    }


    public static class Helpers
    {
        public static IEnumerable<T> MapTo<T>(this IEnumerable<QueryResult> results, Func<QueryResult, T> mappingFunction)
        {
            if (mappingFunction == null) throw new ArgumentNullException("mappingFunction");

            foreach (var result in results)
            {
                yield return mappingFunction(result);
            }

        }

    }

}
