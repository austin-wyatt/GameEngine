using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    public enum ResponseType
    {
        None,
        Custom,
        Ok,
        Yes,
        No
    }

    public enum DialogueStates
    {
        CreateDialogue = -10000, //Opens a dialogue window with the ID of the state value's State ID

    }

    [Serializable]
    public class Dialogue
    {
        [XmlElement("entry")]
        public DialogueNode EntryPoint;

        public int ID = 0;

        [XmlElement("outcome")]
        /// <summary>
        /// An outcome of greater than zero indicates that the outcome is significant and must be stored in the ledger. <para />
        /// An outcome of 0 implies that the dialogue is purely for flavor <para />
        /// An outcome of less than zero can be used for doing things in the scene that aren't relevant to the overall game state (like opening a shop window).
        /// </summary>
        public int DialogueOutcome = 0;

        [XmlElement("name")]
        public string Name = "";

        public Dialogue() { }
        public Dialogue(DialogueNode node)
        {
            EntryPoint = node;
        }
    }

    [XmlType(TypeName = "dn")]
    [Serializable]
    public class DialogueNode
    {
        [XmlElement("s")]
        /// <summary>
        /// 0 will be who initiated the dialogue, 1 will be the first to join after, and so on.<para />
        /// -1 indicates internal dialogue/observations. Text will be italicized or a different color or something.
        /// </summary>
        public int Speaker = 0;
        [XmlElement("m")]
        public int TextEntry = 0;

        [XmlElement("d")]
        /// <summary>
        /// The delay in ms until the dialogue advance when the node has no response.
        /// </summary>
        public int Delay = 0;


        [XmlElement("res")]
        /// <summary>
        /// The response options that can be taken to the dialogue node. If the option is None then the dialogue will automatically advance
        /// </summary>
        public List<Response> Responses = new List<Response>();

        public DialogueNode() { }
        public DialogueNode(int speaker, int messageID)
        {
            Speaker = speaker;
            TextEntry = messageID;
        }

        public Response AddResponse(Response node)
        {
            Responses.Add(node);

            return node;
        }

        public Response AddResponse(out Response res, ResponseType type = ResponseType.None, int messageID = 0, int outcome = 0)
        {
            Response node = new Response(type, messageID, outcome);
            Responses.Add(node);

            res = node;

            return node;
        }

        public Response AddResponse(ResponseType type = ResponseType.None, int messageID = 0, int outcome = 0)
        {
            return AddResponse(out var val, type, messageID, outcome);
        }

        public string GetMessage()
        {
            return TextTableManager.GetTextEntry(0, TextEntry);
        }
    }





    [XmlType(TypeName = "r")]
    [Serializable]
    public class Response
    {
        [XmlElement("rm")]
        public int TextTableEntry = 0;
        [XmlElement("t")]
        public ResponseType ResponseType = ResponseType.Ok;

        [XmlElement("out")]
        /// <summary>
        /// This value will be stored in the DialogueOutcome field and be used to determine which branch was taken in a dialogue
        /// </summary>
        public int Outcome = 0;

        [XmlElement("N")]
        public DialogueNode Next;

        [XmlElement("rSTv")]
        public List<StateIDValuePair> StateValues = new List<StateIDValuePair>();

        public Response() { }
        public Response(ResponseType type = ResponseType.None, int messageID = 0, int outcome = 0, int questStart = -1)
        {
            if (messageID != 0)
            {
                ResponseType = ResponseType.Custom;
            }
            else
            {
                ResponseType = type;
            }

            TextTableEntry = messageID;

            Outcome = outcome;
        }

        public DialogueNode AddNext(DialogueNode next)
        {
            Next = next;
            return next;
        }

        public DialogueNode AddNext(int speaker, int messageID)
        {
            Next = new DialogueNode(speaker, messageID);
            return Next;
        }

        public override string ToString()
        {
            return ResponseType == ResponseType.Custom ? TextTableManager.GetTextEntry(0, TextTableEntry) : ResponseType.ToString();
        }
    }
}
