using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace WenHelltide
{
    class Program
    {
        static void Main(string[] args)
        {
            //           const bool IS_DEV = true;
            const bool IS_DEV = false;

            HttpClient client = new();
              if (args.Length > 1)
            {
                // Consider changing formatting for these
                // - probably look at cmd arg docs 🤔
                Console.WriteLine("-- USAGE --\n");
                Console.WriteLine("1: WenHelltide - Get the time to the next helltide");
                Console.WriteLine("2: WenHelltide{ANY_SECOND_CHARACTER} - Get the time to the next helltide with no animations and no fluff");
                Console.WriteLine("Press Any Key to Exit...");
                Console.ReadKey();
                return;
            }

            // Gets mad annoying bug testing.
            bool doAnimations = true;
            if (args.Length == 1 || IS_DEV)
            {
                doAnimations = false;
            }
            GetInfo(client, doAnimations).Wait();

        }

        public static async Task GetInfo(HttpClient client, bool doAnimations)
        {

            const string FIRST_MESSAGE = "Connecting to sever... ";
            const string FORMATTED_MESSAGE = "{0} hours, {1} minutes, {2} seconds";
            const string LOCATION = "https://game8.co/games/Diablo-4/archives/410034";

            Console.WriteLine("-- STARTING TASK --");
            Console.Write(FIRST_MESSAGE);

            var response = await client.GetAsync(new Uri(LOCATION));
            var content = response.Content.ReadAsStream();

            // Sue me for thinking that streams are so easy and convenient 🤷‍♂
            StreamReader reader = new(content);
            string text = reader.ReadToEnd();
            
            for(var i = 1; i < FIRST_MESSAGE.Length; i++)
            {
                if (doAnimations)
                {
                    int rand = new Random().Next(80, 130);
                    Thread.Sleep(rand);
                }

                Console.Write("\b\b ");
            }
            Console.Write("\b");

            if (doAnimations)
            {
                Thread.Sleep(200);
            }

            Console.WriteLine("Dialed in to the hellscape >:)");

            Regex regex = new(@"data-date=.(\d{10})");
            MatchCollection matches = regex.Matches(text);

            var successMatch = matches.Select(match => match.Groups[1].Success ? match.Groups[1].Value : null);
            DateTime? firstTimeStamp = null;

            foreach (string match in successMatch)
            {
                if (double.TryParse(match, out double outDouble))
                {
                    // 9 hour offset for some unknown reason? crazy stuff.
                    firstTimeStamp = UnixTimeStampToDateTime(outDouble - 32_400);
                }
            }

            if(firstTimeStamp == null)
            {
                Exception exception = new Exception("Could not find valid time stamp.");
                throw exception;
            }


            if (System.DateTime.Now < firstTimeStamp)
            {
                // Safe static cast cuz of the exception up there
                TimeSpan diff = (TimeSpan)(firstTimeStamp - DateTime.Now);
                string formatted = string.Format((FORMATTED_MESSAGE), diff.Hours, diff.Minutes, diff.Seconds);
                if (doAnimations)
                {
                    Console.WriteLine("Next Helltide at: {0}", firstTimeStamp);
                    Thread.Sleep(1300);
                    Console.WriteLine("Or");
                    Thread.Sleep(700);
                }

               for (var i = 0; i < 999; i++)
                {
                    // See comment above 😛<-"cheeky"
                    diff = (TimeSpan)(firstTimeStamp - DateTime.Now);
                    formatted = string.Format(FORMATTED_MESSAGE, diff.Hours, diff.Minutes, diff.Seconds);
                    Console.Write(" {0}",formatted);
                    Thread.Sleep(1000);

                    for (var ii = 0; ii < FORMATTED_MESSAGE.Length; ii++)
                    {
                        Console.Write("\b\b ");
                    }
                }

            }

        }
        public static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

}
