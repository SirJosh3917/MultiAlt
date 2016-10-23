# MultiAlt
MultiAlt allows you to use multiple alts on an EE world to do different tasks at once, and make alt control much easier.

`Install-Package MultiAlt`

# Documentation
How to use MultiAlt:

Step 1: You need to define a Client for it to use, so it can use a Client to join worlds

Step 2: You'll need an AltManager before you can have any alts join the world. You can do `var alts = new AltManager();` to get one

Step 3: Create a new alt, by doing `var alt = new Alt("ThisAlt", client, alts);`.

That's how you get it setup to work.

Next, you can do `alts.CreateJoinRoom();` to make every alt CreateJoinRoom, or do `alt.CreateJoinRoom();` for each individual alt to make it CreateJoinRoom. This also works for JoinRoom.

Next, add a message handler.

`
alts.GotMessge += alts_GotMessage
... Some code later...
void alts_GotMessage(Message e, Connection con, Alt id)
{

}
`

This is so you can recieve messages. Next, you can use these functions to send messages:

`alts.Send("message", "example");`

When you use an AltManager to send messages, all the alts will send it.

`alt.Send("message", "example");`

When you use an individual alt, it'll only send that message.

If you add `alt.Heard += alt_Heard;` somewhere, you'll be able to recieve messages that other alts told that alt by doing `alt2.TellAlt(alt, "message", "example");`. An example message handler looks like `void alt_Heard(Alt Whisperer, Alt me, string Message, params object[] Told)`. Whisperer is who told you the message, me is the alt that recieved it, and the other two parameters are the message params.

You can also do `alt.Disconnect();` if you decide to disconnect from the world.

Please look towards MultiAlt_Demo for an example usage, there's some stuff not documentated.
Also read the source code while you're at it.

# Uses
You could add 5 alts to a world, and fill the entire world 5 times faster than normally.

You can make a big list of messages to send, and then all the AltManager will split up the work and tell each alt what to do.

AltManager allows for an easy OnMessage, and it is very easy to tell which alt recieved the message, and provides an easy function to send messages from that alt.

Alts also have a built in storage, so you can do `Alt["key"] = "value"` to store data inside of the alt for easy access.
