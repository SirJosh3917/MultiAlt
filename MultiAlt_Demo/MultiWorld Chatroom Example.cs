
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
			var alt_add = new Alt(AltId, alt, alts);
			alt_add.Heard += alt_add_Heard;
			alts.AddAlt(alt_add);
		}

		static void alt_add_Heard(Alt Whisperer, Alt me, string Message, params object[] Told)
		{
			if (Whisperer.AltId != me.AltId)
			{
				if (Message == "said")
				{
					me.Send("say", Told[0] + " > " + Told[1]);
				}
			}
		}

		static void Main(string[] args)
		{
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
			alts.GetAlt("alt1").CreateJoinRoom(WorldId, WorldType, true, null, null);
			alts.GetAlt("alt2").CreateJoinRoom(WorldId, WorldType, true, null, null);
			alts.GetAlt("alt3").CreateJoinRoom(WorldId, WorldType, true, null, null);
			alts.GetAlt("alt4").CreateJoinRoom(WorldId, WorldType, true, null, null);
			alts.GetAlt("alt5").CreateJoinRoom(WorldId, WorldType, true, null, null);

			//Make all the alts send "init"
			alts.Send("init");

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
				case "add":
					id["player_" + e.GetInt(0)] = e.GetString(1);
					break;

					//I always forget which one it is :P
				case "leave":
				case "left":
					id["player_" + e.GetInt(0)] = null;
					break;

				case "init2":
					con.Send("say", "I am a multi chatroom bot. Say something and you can say it to other rooms!");
					break;

				case "say":
					//Make sure that the person saying it isn't one of the alts
					if (alts.AltsContain("id", e.GetInt(0)))
					{
						alts.TellAllAlts("said", id["player_" + e.GetInt(0)], e.GetString(1));
					}
					break;
			}
		}
	}
}
