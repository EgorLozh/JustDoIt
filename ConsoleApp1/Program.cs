using System;

namespace testing_class
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var db = new DataBase("sql7.freemysqlhosting.net", "sql7709198", "sql7709198", "7FFlVW1Jn2");
            var race = new Race(db, "race", DateTime.Now);
            var runner = new Runner(db, "runner", "Defoult", 1, "M");
            var runner1 = (Runner) runner.Read()[0];
            var race1 = (Race)race.Read()[0];
            Console.WriteLine("OK");
            var runnerTime = new RunnersTime(db, "runners_time", runner1, race1, DateTime.Now, DateTime.Now, "Wait");
            runnerTime.Save();
            //var objects = runner.Read();
            //foreach (var obj in objects)
            //{
            //    Console.WriteLine(obj.ToString());
            //}
        }
    }
}