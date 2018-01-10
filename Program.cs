using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WuManberNet
{
	class Program
	{
		static void Main(string[] args)
		{
			FunctionTest();
			PerfTest();
			Console.Read();
		}

		private static void PerfTest()
		{
			var target = "Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; Infospiders .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0";
			var patterns = new List<string>{
	"googlebot","bingbot","yandexbot","ahrefsbot","msnbot","linkedinbot","exabot","compspybot",
	"yesupbot","paperlibot","tweetmemebot","semrushbot","gigabot","voilabot","adsbot-google",
	"botlink","alkalinebot","araybot","undrip bot","borg-bot","boxseabot","yodaobot","admedia bot",
	"ezooms.bot","confuzzledbot","coolbot","internet cruiser robot","yolinkbot","diibot","musobot",
	"dragonbot","elfinbot","wikiobot","twitterbot","contextad bot","hambot","iajabot","news bot",
	"irobot","socialradarbot","ko_yappo_robot","skimbot","psbot","rixbot","seznambot","careerbot",
	"simbot","solbot","mail.ru_bot","spiderbot","blekkobot","bitlybot","techbot","void-bot",
	"vwbot_k","diffbot","friendfeedbot","archive.org_bot","woriobot","crystalsemanticsbot","wepbot",
	"spbot","tweetedtimes bot","mj12bot","who.is bot","psbot","robot","jbot","bbot","bot",
	"baiduspider","80legs","baidu","yahoo! slurp","ia_archiver","mediapartners-google","lwp-trivial",
	"nederland.zoek","ahoy","anthill","appie","arale","araneo","ariadne","atn_worldwide","atomz",
	"bjaaland","ukonline","bspider","calif","christcrawler","combine","cosmos","cusco","cyberspyder",
	"cydralspider","digger","grabber","downloadexpress","ecollector","ebiness","esculapio","esther",
	"fastcrawler","felix ide","hamahakki","kit-fireball","fouineur","freecrawl","desertrealm",
	"gammaspider","gcreep","golem","griffon","gromit","gulliver","gulper","whowhere","portalbspider",
	"havindex","hotwired","htdig","ingrid","informant","infospiders","inspectorwww","iron33",
	"jcrawler","teoma","ask jeeves","jeeves","image.kapsi.net","kdd-explorer","label-grabber",
	"larbin","linkidator","linkwalker","lockon","logo_gif_crawler","marvin","mattie","mediafox",
	"merzscope","nec-meshexplorer","mindcrawler","udmsearch","moget","motor","muncher","muninn",
	"muscatferret","mwdsearch","sharp-info-agent","webmechanic","netscoop","newscan-online",
	"objectssearch","orbsearch","packrat","pageboy","parasite","patric","pegasus","perlcrawler",
	"phpdig","piltdownman","pimptrain","pjspider","plumtreewebaccessor","getterrobo-plus","raven",
	"roadrunner","robbie","robocrawl","robofox","webbandit","scooter","search-au","searchprocess",
	"senrigan","shagseeker","site valet","skymob","slcrawler","slurp","snooper","speedy",
	"spider_monkey","spiderline","curl_image_client","suke","www.sygol.com","tach_bw","templeton",
	"titin","topiclink","udmsearch","urlck","valkyrie libwww-perl","verticrawl","victoria",
	"webscout","voyager","crawlpaper","wapspider","webcatcher","t-h-u-n-d-e-r-s-t-o-n-e",
	"webmoose","pagesinventory","webquest","webreaper","webspider","webwalker","winona","occam",
	"robi","fdse","jobo","rhcs","gazz","dwcp","yeti","crawler","fido","wlm","wolp","wwwc","xget",
	"legs","curl","webs","wget","sift","cmc"
};

			var search = new WuManber();
			search.Initialize(patterns.Select(p => new WordMatch { Word = p }).ToList());
			var t = Stopwatch.StartNew();
			var count = 1_00_000;
			for (int i = 0; i < count; i++)
			{
				var result = search.Search(target).ToArray();
			}
			var perf = count / (t.ElapsedMilliseconds / 1000.0);
			Console.WriteLine($"{count} @ {t.Elapsed}, {perf}/s");
		}

		private static void FunctionTest()
		{
			var target = "This is some text I made up.  This will be testing\nmulti-pattern matching from Wu/Manber's paper called\n'A Fast Algorithm for Multi-Pattern Searching'. Manber is\ncurrently at Google.";
			var patterns = new List<string>();
			patterns.Add("this is");
			patterns.Add("pattern");
			patterns.Add("google!");
			patterns.Add("anber");

			var search = new WuManber();
			search.Initialize(patterns.Select(p => new WordMatch { Word = p }).ToList());
			foreach (var item in search.Search(target))
			{
				Console.WriteLine($"{item.Word}: {item.Index}");
			}
		}
	}
}
