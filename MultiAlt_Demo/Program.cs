using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerIOClient;
using MultiAlt;
using System.Threading;

namespace MultiAlt_Demo
{
	class Program
	{
		static AltManager alts;
		static uint[, ,] Stuff = new uint[2, 0, 0];

		static void Main(string[] args)
		{
			//Create a big list of stuff to send
			List<Message> m = new List<Message>();
			for (int x = 1; x < 199; x++)
			{
				for (int y = 5; y < 199; y++)
				{
					m.Add(Message.Create("b", 0, x, y, 14));
				}
			}

			alts = new AltManager();

			string GameId = "everybody-edits-su9rn58o40itdbnw69plyw";

			string WorldId = "OW_test";

			string WorldType = "Everybodyedits215";

			string Alt1Email = "ALT 1 EMAIL";
			string Alt1Password = "ALT 1 PASSWORD";
			string Alt2Email = "ALT 2 EMAIL";
			string Alt2Password = "ALT 2 PASSWORD";

			Console.WriteLine("Joining...");

			Client alt1 = PlayerIO.QuickConnect.SimpleConnect(GameId, Alt1Email, Alt1Password, null);
			Console.WriteLine("Alt1 Joined");
			Client alt2 = PlayerIO.QuickConnect.SimpleConnect(GameId, Alt2Email, Alt2Password, null);
			Console.WriteLine("Alt2 Joined");

			alts.AddAlt(new Alt("IAMALT1", alt1, alts));
			alts.AddAlt(new Alt("IAMALT2", alt2, alts));

			alts.GotMessage += alts_GotMessage;
			alts.CreateJoinRoom(WorldId, WorldType, true, null, null);

			alts.Send("init");

			Console.ReadLine();

			alts.SplitWork(m, delegate()
			{
				//After a message is sent, sleep.
				Thread.Sleep(5);
			}, delegate(Message e)
			{
				//If the world at the message X and Y isn't already 14, we will send it.
				if (Stuff[0, e.GetInt(1), e.GetInt(2)] != 14) return true;
				return false;
			});

			Console.ReadLine();
		}

		static void alts_GotMessage(Message e, Connection con, Alt id)
		{
			switch (e.Type)
			{
				case "init":
					id["id"] = e.GetInt(5).ToString();
					con.Send("init2");
					break;
				case "init2":
					con.Send("say", "Hello World, from Alt " + id.AltId);
					break;
			}
		}
	}
}
