using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;

namespace testing_class
{
    public class DataBase
    {
        private MySqlConnection _connection;


        public DataBase(string server, string name, string userName, string password)
        {
            var conStr = $"server={server};user={userName};database={name};password={password};charset=utf8";
            this._connection = new MySqlConnection(conStr);
        }

        public void ExecuteCommand(string sql)
        {
            _connection.Open();
            var command = new MySqlCommand(sql, _connection);
            var result = command.ExecuteScalar();
            _connection.Close();
        }

        public List<string[]> GetResponse(string sql)
        {
            _connection.Open ();
            var repsonse =new  List<string[]>();
            var command = new MySqlCommand(sql, _connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var i = 0;
                var values = new List<string>();
                while (true)
                {
                    try
                    {
                        var value = reader[i].ToString();
                        values.Add(value);
                        i++;
                    }
                    catch
                    {
                        repsonse.Add(values.ToArray());
                        break;
                    }                    
                }
            }
            reader.Close();
            _connection.Close ();
            return repsonse;
        }

    }

    public class DB_object
    {
        public DataBase dataBase;
        public string tableName;
        public int? Id { get; set; }

        public DB_object() { }

        public DB_object(DataBase dataBase, string tableName, int? Id = null)
        {
            this.dataBase = dataBase;
            this.tableName = tableName;
            this.Id = Id;
        }


        public void Save()
        {
            var fields = new StringBuilder();
            var values = new StringBuilder();
            var obj_fields = this.GetType().GetProperties().ToArray();

            foreach (var field in obj_fields)
            {
                fields.Append("`" + field.Name.ToLower().ToString() + "`");
                if (field.PropertyType != typeof(DateTime))
                {
                    values.Append("'" + field.GetValue(this) + "'");
                }
                else
                {
                    values.Append("'" + ((DateTime)field.GetValue(this)).ToString("yyyy-MM-dd HH:mm:ss") + "'");
                }
                if (field != obj_fields.Last())
                {
                    fields.Append(", ");
                    values.Append(", ");
                }
            }
            var sql = $"INSERT INTO {this.tableName} ({fields.ToString()}) VALUES ({values.ToString()})";
            this.dataBase.ExecuteCommand(sql);

            sql = $"SELECT ID FROM `{tableName}` ORDER BY ID DESC LIMIT 1";            
            var result = int.Parse(this.dataBase.GetResponse(sql)[0][0]);
            typeof(DB_object).GetProperty("Id").SetValue(this, result);

        }

        public void Update()
        {
            var query = new StringBuilder();
            var obj_fields = this.GetType().GetProperties().ToArray();
            foreach (var field in obj_fields)
            {
                if (field.Name != "Id")
                {
                    query.Append("`" + field.Name.ToLower().ToString() + "`");
                    query.Append("= '" + field.GetValue(this) + "'");
                    if (field != obj_fields.Last())
                    {
                        query.Append(", ");
                    }
                }
            }
            var sql = $"UPDATE `{this.tableName}` SET {query.ToString()}  WHERE `{this.tableName}`.`id` = {this.Id}";
            Console.WriteLine(sql);
            this.dataBase.ExecuteCommand (sql);
        }

        public void Delite()
        {
            var sql = $"DELETE FROM `{this.tableName}` WHERE `{tableName}`.`id` = {this.GetType().GetProperty("Id").GetValue(this)}";
            Console.WriteLine(sql);
            this.dataBase.ExecuteCommand(sql);
        }

        public List<DB_object> Read(Dictionary<string, string> conditions = null, int? limit = null)
        {
            var list = new List<DB_object>();
            var query = new StringBuilder();
            var sql = $"SELECT * FROM `{this.tableName}`";

            if (conditions != null && conditions.Count > 0)
            {
                foreach (var condition in conditions.Keys)
                {
                    query.Append($"`{condition}` = '{conditions[condition]}' AND ");
                }
                query.Length -= 5; // Удаление последнего " AND "
                sql += " WHERE " + query;
            }
            if (limit != null)
            {
                sql += " LIMIT " + limit;
            }

            var response = this.dataBase.GetResponse(sql);
            var obj_fields = this.GetType().GetProperties().OrderBy(p => p.Name != "Id").ToArray();

            foreach (var item in response)
            {
                var obj = (DB_object)Activator.CreateInstance(this.GetType());

                for (int i = 0; i < item.Length; i++)
                {
                    var property = obj_fields[i];
                    var value = item[i];

                    if (value != null && property.CanWrite)
                    {
                        try
                        {
                            if (property.PropertyType == typeof(int?))
                            {
                                property.SetValue(obj, string.IsNullOrEmpty(value) ? (int?)null : int.Parse(value));
                            }
                            else if (property.PropertyType == typeof(int))
                            {
                                property.SetValue(obj, int.Parse(value));
                            }
                            else if (property.PropertyType == typeof(double))
                            {
                                property.SetValue(obj, double.Parse(value));
                            }
                            else if (property.PropertyType == typeof(bool))
                            {
                                property.SetValue(obj, bool.Parse(value));
                            }
                            else if (property.PropertyType == typeof(DateTime))
                            {
                                property.SetValue(obj, DateTime.Parse(value));
                            }
                            else
                            {
                                property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                            }
                        }
                        catch (FormatException)
                        {
                            throw new Exception($"Error parsing field {property.Name} with value {value}");
                        }
                    }
                }
                obj.dataBase = this.dataBase;
                obj.tableName = this.tableName;
                list.Add(obj);
            }

            return list;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            var properties = this.GetType().GetProperties().OrderBy(p => p.Name != "Id");

            stringBuilder.Append($"{this.GetType().Name} [");

            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                if (property.PropertyType == typeof(DateTime))
                {
                    stringBuilder.Append($"{property.Name}={((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss")}");
                }
                else
                {
                    stringBuilder.Append($"{property.Name}={value}");
                }

                if (property != properties.Last())
                {
                    stringBuilder.Append(", ");
                }
            }

            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }


    public class Runner : DB_object
    {
        public string Name { get; set; }
        private int age;
        public int Age
        {
            get => this.age;
            set
            {
                if ((value <= 0) || (value >= 200))
                {
                    throw new Exception("Некорректный возраст, укажите возраст в промежутке от 0 до 200");
                }
                this.age = value;
            }
        }
        private string sex;
        public string Sex
        {
            get => this.sex;
            set
            {
                if ((value == "F") || (value == "M"))
                    this.sex = value;
                else throw new Exception("Не корректный пол, укажите 'F'-для женщины и 'M'-для мужчины");
            }
        }

        public Runner(): base() { }

        public Runner(DataBase dataBase, string tableName, string name, int age, string sex, int? id = null) : base(dataBase, tableName, id)
        {
            this.Name = name;
            this.Age = age;
            this.Sex = sex;
        }
    }

    public class Race : DB_object
    {
        public int? Id { get; set; }
        public DateTime Beginning_time { get; set; }

        public Race(): base() {}

        public Race(DataBase dataBase, string tableName, DateTime beginning_time, int? id=null): base(dataBase, tableName, id)
        {
            Beginning_time = beginning_time;
        }
    }

    public class RunnersTime: DB_object
    {
        public int? Runner_id {  get; set; }
        public int? Race_id {  get; set; }
        public DateTime Start_time { get; set; }
        public DateTime Finish_time { get; set; }
        public string Status {  get; set; }

        public RunnersTime(): base() { }

        public RunnersTime(DataBase dataBase, string tableName, Runner runner, Race race, DateTime start_time,
            DateTime finish_time, string status, int? id = null): base(dataBase, tableName, id)
        {
            Runner_id = runner.Id;
            Race_id = race.Id;
            Start_time = start_time;
            Finish_time = finish_time;
            Status = status;
        }
    }
}
