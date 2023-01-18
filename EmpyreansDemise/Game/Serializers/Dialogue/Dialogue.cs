using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    public enum ResponseType
    {
        None,
        Custom,
    }

    public enum DialogueStates
    {
        CreateDialogue = -10000, //Opens a dialogue window with the ID of the state value's State ID

    }

    [Serializable]
    public class Dialogue : ISerializable
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

        public double Scale = 1;

        public Dialogue() 
        {
            
        }
        public Dialogue(DialogueNode node)
        {
            EntryPoint = node;
        }

        public void CompleteDeserialization()
        {
            EntryPoint.CompleteDeserialization();
        }

        public void PrepareForSerialization()
        {
            EntryPoint.PrepareForSerialization();
        }
    }

    [XmlType(TypeName = "dn")]
    [Serializable]
    public class DialogueNode : ISerializable
    {
        [XmlElement("s")]
        /// <summary>
        /// 0 will be who initiated the dialogue, 1 will be the first to join after, and so on.<para />
        /// -1 indicates internal dialogue/observations. Text will be italicized or a different color or something.
        /// </summary>
        public int Speaker = 0;
        [XmlElement("m")]
        public TextId TextInfo = new TextId();

        [XmlElement("d")]
        /// <summary>
        /// The delay in ms until the dialogue advance when the node has no response.
        /// </summary>
        public int Delay = 0;

        [XmlElement("dno")]
        public int Outcome = 0;

        [XmlElement("drp")]
        public Vector2 RelativePosition = new Vector2();

        [XmlElement("res")]
        /// <summary>
        /// The response options that can be taken to the dialogue node. If the option is None then the dialogue will automatically advance
        /// </summary>
        public List<Response> Responses = new List<Response>();

        [XmlElement("dnDesc")]
        public string Description = "";

        public DialogueNode() { }
        //public DialogueNode(int speaker, int messageID)
        //{
        //    Speaker = speaker;
        //    TextEntry = messageID;
        //}

        public string GetMessage()
        {
            return TextInfo.ToString();
        }

        public void CompleteDeserialization()
        {
            foreach(var response in Responses)
            {
                response.CompleteDeserialization();
            }
        }

        public void PrepareForSerialization()
        {
            foreach (var response in Responses)
            {
                response.PrepareForSerialization();
            }
        }
    }





    [XmlType(TypeName = "r")]
    [Serializable]
    public class Response : ISerializable
    {
        [XmlElement("rm")]
        public TextId TextInfo = new TextId();

        [XmlElement("out")]
        /// <summary>
        /// This value will be stored in the DialogueOutcome field and be used to determine which branch was taken in a dialogue
        /// </summary>
        public int Outcome = 0;

        [XmlElement("rt")]
        public ResponseType ResponseType = ResponseType.Custom;

        [XmlElement("rDesc")]
        public string Description = "";

        [XmlElement("N")]
        public DialogueNode Next;

        [XmlElement("rSTv")]
        public List<Instructions> Instructions = new List<Instructions>();

        [XmlElement("rc")]
        public Conditional Conditional = new Conditional(Conditional.TRUE);

        public Response() { }

        public override string ToString()
        {
            return TextInfo.ToString();
        }

        public void CompleteDeserialization()
        {
            Conditional.CompleteDeserialization();
            Next?.CompleteDeserialization();
        }

        public void PrepareForSerialization()
        {
            Conditional.PrepareForSerialization();
            Next?.PrepareForSerialization();
        }
    }
}
