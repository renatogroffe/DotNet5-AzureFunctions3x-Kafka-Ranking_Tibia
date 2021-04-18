using System.Collections.Generic;

namespace FunctionAppExtracaoTibia.Documents
{
    public class RankingTibia
    {
        public string id { get; set; }
        public string time { get; set; }
        public Highscores highscores { get; set; }
        public Information information { get; set; }        
    }

    public class Information
    {
        public int api_version { get; set; }
        public double execution_time { get; set; }
        public string last_updated { get; set; }
        public string timestamp { get; set; }
    }

    public class Highscores
    {
        public Filters filters { get; set; }
        public List<Score> data { get; set; }
    }

    public class Filters
    {
        public string world { get; set; }
        public string category { get; set; }
        public string vocation { get; set; }
    }

    public class Score
    {
        public string name { get; set; }
        public int rank { get; set; }
        public string world { get; set; }
        public string vocation { get; set; }
        public object points { get; set; }
        public int level { get; set; }
    }
}