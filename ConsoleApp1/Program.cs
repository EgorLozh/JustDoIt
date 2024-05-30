using System;
using System.Reflection;

namespace testing_class
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var db = new DataBase("sql7.freemysqlhosting.net", "sql7709198", "sql7709198", "7FFlVW1Jn2");
            //var race = new Race(db, DateTime.Now);
            //race.Save();
            //Console.WriteLine(race);
            //var runner = new Runner(db, "Defoult", 1, "M");
            //var runner1 = (Runner) runner.Read()[0];
            //var race1 = (Race)race.Read()[0];
            //Console.WriteLine("OK");
            //var runnerTime = new RunnersTime(db, runner1, race1, DateTime.Now, DateTime.Now, "Wait");
            //runnerTime.Save();

            //var objects = db.Read(typeof(RunnersTime));
            //foreach (var obj in objects)
            //{
            //    Console.WriteLine(obj.ToString());
            //}
            db.Delete(typeof(Runner), 17);
        }
    }
}