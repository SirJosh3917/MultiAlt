/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerIOClient;
using MultiAlt;
using System.Threading;
using Yonom.EE;

//Credit to processor for init parse

namespace MultiAlt_Demo
{
	class Program
	{
		static AltManager alts;
		static uint[, ,] Stuff = new uint[2, 0, 0];

		static void AddAlt(string GameId, string Email, string Password, string AltId)
		{
			Client alt = PlayerIO.QuickConnect.SimpleConnect(GameId, Email, Password, null);
			alts.AddAlt(new Alt(AltId, alt, alts));
		}

		static void Main(string[] args)
		{
			//Fill a 200x200 are with green basic blocks
			List<Message> m = new List<Message>();
			for (int x = 1; x < 199; x++)
			{
				for (int y = 5; y < 199; y++)
				{
					m.Add(Message.Create("b", 0, x, y, 14));
				}
			}

			//Initiate the alt manager
			alts = new AltManager();

			string GameId = "everybody-edits-su9rn58o40itdbnw69plyw";
			string WorldId = "OW MultiAlt Test";
			string WorldType = "Everybodyedits216";

			//Add all the alts
			AddAlt(GameId, "alt1@email.com", "alt1password", "alt1");
			AddAlt(GameId, "alt2@email.com", "alt2password", "alt2");
			AddAlt(GameId, "alt3@email.com", "alt3password", "alt3");
			AddAlt(GameId, "alt4@email.com", "alt4password", "alt4");
			AddAlt(GameId, "alt5@email.com", "alt5password", "alt5");

			//Add event handler
			alts.GotMessage += alts_GotMessage;

			//Make alts all join a world
			alts.CreateJoinRoom(WorldId, WorldType, true, null, null);

			//Make all the alts send "init"
			alts.Send("init");
			
			Console.ReadLine();

			//Make each alt spit their work
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
					//Set each alt's id
					id["id"] = e.GetInt(5).ToString();

					//Parse room data
					var roomData = new uint[2, e.GetInt(18), e.GetInt(19)];
					var chunks = InitParse.Parse(e);
					foreach (var chunk in chunks)
						foreach (var pos in chunk.Locations)
							roomData[chunk.Layer, pos.X, pos.Y] = chunk.Type;
					Stuff = roomData;

					//Send "init2"
					con.Send("init2");
					break;
				case "init2":
					con.Send("say", "Hello World, from Alt " + id.AltId);
					break;
			}
		}
	}
}
*/