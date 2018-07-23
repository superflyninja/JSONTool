using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSON_Tool
{
    public abstract class ControlType
    {
        [JsonProperty(Order = -5)]
        public string controlType { get; set; }

        [JsonProperty(Order = -3)]
        public string key { get; set; }

        [JsonProperty(Order = -4)]
        public string label { get; set; }
        
        public bool visible { get; set; }
        public bool required { get; set; }

        public ControlType(string name)
        {
            init();

            label = name;
        }

        void init()
        {
            label = "Default";
            visible = true;
            required = false;
            key = JSONFormController.GenerateGUID();
        }
    }

    //step and section use just 5 properties. The question types use all the rest
    //split the properties between the two types. minimal props in the base and the rest in the question subtype
    public abstract class QuestionControlType : ControlType
    {
        [JsonProperty(Order = -2)]

        public string placeholder { get; set; }
        public string tip { get; set; }
        public string value { get; set; }
        [JsonProperty(Order = 2)]
        public string localizationkey { get; set; }
        public string placeholderLocalizationkey { get; set; }
        public string tipLocalizationkey { get; set; }
        public string regSysKey { get; set; }
        public string regSysType { get; set; }

        public QuestionControlType(string name) : base(name)
        {
            init();
        }

        void init()
        {
            placeholder = "";
            tip = "";
            tipLocalizationkey = "";
            placeholderLocalizationkey = "";
            localizationkey = "";
            value = "";
            regSysKey = "";
            regSysType = "";
        }

    }

    #region structs

    public struct ConditionalPropertyList
    {
        public List<ConditionalProperty> visible;//each conditional property effects an attribute of the question. For example visible or required

        public ConditionalPropertyList(List<ConditionalProperty> theList)
        {
            visible = theList;
        }
    }

    public struct ConditionalProperty
    {
        public string propertyValue;
        public string left;
        public string theOperator;//needs to be operator in JSON -> do searhc and replace at point of returnign the JSON as string
        public string right;

        public ConditionalProperty(string theValue, string _left, string _operator, string _right)
        {
            propertyValue = theValue;
            left = _left;
            theOperator = _operator;
            right = _right;
        }
    }

    public struct option
    {
        public string key;
        public string value;

        public option(string theKey,string theValue)
        {
            key = theKey;
            value = theValue;
        }
    }

    public struct QuestionBase
    {
        public string regSysKey;
        public List<ControlType> questions;

        public QuestionBase(string theRegKey, List<ControlType> theQuestions)
        {
            regSysKey = theRegKey;
            questions = theQuestions;
        }
    }

    public struct Descriptor
    {
        public string label;
        public string order;
        public List<DescriptorKey> keys;

        public Descriptor(string theLabel, string theOrder)
        {
            label = theLabel;
            order = theOrder;
            keys = new List<DescriptorKey>();
        }

    }

    public struct DescriptorKey
    {
        public DescriptorKeyCondition condition;
        public string key;

        public DescriptorKey(DescriptorKeyCondition theCondition, string theKey)
        {
            condition = theCondition;
            key = theKey;
        }
    }

    public struct DescriptorKeyCondition
    {
        public string key;
        public string value;

        public DescriptorKeyCondition(string theKey,string theValue)
        {
            key = theKey;
            value = theValue;
        }
    }
    #endregion

    #region Control Types
    public class Step : ControlType
    {
        [JsonProperty(Order = -50)]//appear first
        public int order { get; set; }

        [JsonProperty(Order = 50)]//appear last
        public List<ControlType> questions { get => theQuestions; set => theQuestions = value; }//child controlTypes

        private List<ControlType> theQuestions;

        public Step (string name) : base(name){
            controlType = "step";
            questions = new List<ControlType>();
        }

    }

    public class Section : ControlType
    {
        [JsonProperty(Order = 50)]
        public List<ControlType> questions { get => theQuestions; set => theQuestions = value; }//child controlTypes
        private List<ControlType> theQuestions;

       // public override string controlType => "section";

        public Section(string name) : base(name)
        {
            controlType = "section";
            questions = new List<ControlType>();
        }

    }

    public class ListControl : ControlType
    {

        public bool defaultOpen { get; set; }

        public string itemName { get; set; }

        [JsonProperty(Order = 49)]
        public QuestionBase questionBase;

        [JsonProperty(Order = 50)]
        public List<Descriptor> descriptors { get => theDescriptors; set => theDescriptors = value; }//child controlTypes
        private List<Descriptor> theDescriptors;

        public ListControl(string name) : base(name)
        {
            controlType = "list";

            questionBase = new QuestionBase("", new List<ControlType>());
            questionBase.questions.Add(new Section("Section One"));
            descriptors = new List<Descriptor>();
            //List of descriptors 
            //Each descriptor contains list of keys and label,order string
            for(int i = 0; i < 2; i++)
            {
                Descriptor desc = new Descriptor("", (i + 1) + "");
                desc.keys.Add(new DescriptorKey(new DescriptorKeyCondition("",""),""));
                desc.keys.Add(new DescriptorKey(new DescriptorKeyCondition("", ""), ""));
                descriptors.Add(desc);
            }
            itemName = name;//"List Item";
            defaultOpen = true;
        }
    }
    #endregion

    public class GenericQuestion : QuestionControlType
    {
        [JsonProperty(Order = 50)]
        public List<option> options { get => theOptions; set => theOptions = value; }
        private List<option> theOptions;

        [JsonIgnore]
        public bool conditions { get; set; }

        [JsonProperty(Order = 60)]
        public ConditionalPropertyList conditionalProperties{ get; set; }

    public GenericQuestion(string theName, string theType) : base(theName)
        {
            if (theType == JSONFormController.QuestionTypes.freeNote.ToString().ToLower())
            {
                theType = "free-note";
                value = theName;
            }

            controlType = theType;
        }

        public bool ShouldSerializeoptions()//if options has not been initialised then it will not be serialized
        {
            return options != null;
        }

        public bool ShouldSerializeconditionalProperties()//if condition not active then it will not be serialized
        {
            return conditionalProperties.visible != null;//.visible.Count > 0;
        }
    }

}
