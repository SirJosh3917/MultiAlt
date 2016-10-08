using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerIOClient;

namespace MultiAlt
{
    public class Alt
	{
		private AltManager master;
		internal Client client;
		internal Connection connection;
		internal bool inworld = false;
		public string AltId { get; private set; }
		private Dictionary<string, string> privData = new Dictionary<string, string>();

		public Alt(string altId, Client alt, AltManager master_)
		{
			AltId = altId;
			master = master_;
			client = alt;
		}

		public void Send(Message e)
		{
			connection.Send(e);
		}

		public void Send(string type, params object[] args)
		{
			connection.Send(type, args);
		}

		internal void JoinWorld(Connection _joinWorld)
		{
			connection = _joinWorld;
			connection.OnMessage += connection_OnMessage;
			connection.OnDisconnect += connection_OnDisconnect;
		}

		void connection_OnDisconnect(object sender, string message)
		{
			Console.WriteLine(sender.ToString());
			Console.WriteLine(sender.GetType().ToString());
			master.AltGotDisconnected(sender, message, this);
		}

		void connection_OnMessage(object sender, Message e)
		{
			master.AltGotMessage(e, this);
		}

		public string this[string index]
		{
			get
			{
				if (privData.ContainsKey(index))
				{
					return privData[index];
				}
				else
				{
					return null;
				}
			}
			set
			{
				privData[index] = value;
			}
		}
	}
}
