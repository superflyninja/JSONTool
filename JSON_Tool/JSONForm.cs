using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSON_Tool
{
    class JSONForm
    {

        [JsonProperty(Order = 0)]
        public string version { get; set; }

        [JsonProperty(Order = 1)]
        public string displayName { get; set; }

        #region Structs

        public struct SubmissionDetails
        {
            public string submissionTypeId;
            public string submissionTypeDesc;
            public string relatedRegisterId;
            public List<RegEdit> submissionRegisterEdits;

            public SubmissionDetails(string subTypeID,string subDesc,string regID,List<RegEdit>theRegEdits)
            {
                submissionTypeId = subTypeID;
                submissionTypeDesc = subDesc;
                relatedRegisterId = regID;
                submissionRegisterEdits = theRegEdits;

                if (submissionRegisterEdits.Count <= 0)
                {
                    submissionRegisterEdits.Add(new RegEdit(JSONFormController.GenerateGUID(),"name","id"));
                    submissionRegisterEdits.Add(new RegEdit(JSONFormController.GenerateGUID(), "name", "id"));
                }
            }
        }

        public struct RegEdit
        {
            public string key;
            public string name;
            public string id;

            public RegEdit(string theKey, string theName, string theID)
            {
                key = theKey;
                name = theName;
                id = theID;
            }
        }
        #endregion

        [JsonProperty(Order = 2)]
        public SubmissionDetails subDetails;

        [JsonProperty(Order = 3)]
        public List<Step> steps { get => theSteps; set => theSteps = value; }
        private List<Step> theSteps;

        public JSONForm(string name)
        {
            version = "1.0.0";
            displayName = name;
            steps = new List<Step>();

            subDetails = new SubmissionDetails("null", "null", "null", new List<RegEdit>());

        }

    }

    public class JSONFormController
    {
        JSONForm currentForm;
        public bool FormInProgress { get; set; }
        public int totalNumSections { get; set; }
        public int totalNumQuestions { get; set; }

        public int numSteps {
            get {
                return (currentForm != null) ? currentForm.steps.Count : 0;
            }
            set { }
        }

        public enum QuestionTypes
        {
            textbox,
            textArea,
            display,
            date,
            freeNote,
            radio,
            dropdown
        };

        Step currentlyActiveStep;
        Section currentlyActiveSection;

        public JSONFormController()
        {
            FormInProgress = false;
        }

        public void NewForm(string theName)
        {
            totalNumSections = 0;
            totalNumQuestions = 0;
            currentlyActiveStep = null;
            currentlyActiveSection = null;
            currentForm = new JSONForm(theName);
            FormInProgress = true;
        }

        //Set form properties
        public void SetVersion(string theVersion)
        {
            currentForm.version = theVersion;
        }

        public void Test()
        {
            EnsureFormExists();
            AddStep("Test Step 1");
            AddSection("Section 1");

            AddQuestion("Display",JSONFormController.QuestionTypes.display.ToString(),true);
            AddQuestion("TextArea", JSONFormController.QuestionTypes.textArea.ToString(),true,true,true, true);
            AddQuestion("TextBox", JSONFormController.QuestionTypes.textbox.ToString(), true);
            AddQuestion("Radio", JSONFormController.QuestionTypes.radio.ToString(), true);
            AddQuestion("Free Note", JSONFormController.QuestionTypes.freeNote.ToString(), true);
            AddQuestion("Drop Down", JSONFormController.QuestionTypes.dropdown.ToString(), true);

        }

        public string GetCurrentStepName()
        {
            if (currentForm.steps.Count <= 0)
            {
                return "";
            }

            return currentForm.steps.Last<Step>().label;
        }

        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public string GetCurrentSectionName()
        {
            if (currentlyActiveSection == null)
            {
                return "";
            }

            return currentlyActiveSection.label;
        }

        public string GetFormJSON()
        {
            EnsureFormExists();

            if (numSteps == 1)//need to add a second invisible tab to get the tab 1 description working
            {
                AddInvisibleStep();
            }

            //at this point done with the form. so wipe it 
            FormInProgress = false;

            return JsonConvert.SerializeObject(currentForm, Formatting.Indented).Replace("theOperator","operator");
        }

        #region Add Control Containers To Form
        public void AddStep(string theName)
        {
            EnsureFormExists();

            Step step = new Step(theName);
            step.order = currentForm.steps.Count + 1;//if 0 steps added, then order will be 1. if 3 steps already added then order will be 4
            currentForm.steps.Add(step);

            currentlyActiveStep = step;
            numSteps++;
        }

        public void AddSection(string theName)
        {
            EnsureStepExists();

            //at the moment just add a section to the current step
            Section section = new Section(theName);
            currentForm.steps.Last<Step>().questions.Add(section);

            currentlyActiveSection = section;
        }

        public void AddList(string theName)
        {
            EnsureStepExists();
            EnsureSectionExists();
            currentlyActiveSection.questions.Add(new ListControl(theName));
        }
        #endregion

        public GenericQuestion AddQuestion(string theName, string theType,bool partOfForm)
        {
          
            
            theType = theType.ToLower();

            var q = new GenericQuestion(theName, theType);
            q.label = theName;
            if (theType == JSONFormController.QuestionTypes.radio.ToString() || theType == JSONFormController.QuestionTypes.dropdown.ToString())
            {
                List<option> options = new List<option>();
                options.Add(new option(GenerateGUID(), "value1"));//if no options added then set defaults
                options.Add(new option(GenerateGUID(), "value2"));
                q.options = options;
            }

            if (partOfForm)
            {
                EnsureSectionExists();
                currentlyActiveSection.questions.Add(q);
            }
            

            //QuestionTypes typeID = (QuestionTypes)Enum.Parse(typeof(QuestionTypes), theType);//Attempt to convert the string to question type id

            totalNumQuestions++;

            return q;
        }

        public GenericQuestion AddQuestion(string theName, string theType,bool conditions, bool required, bool visible,bool soloQuestion)
        {
            var q = AddQuestion(theName,theType,soloQuestion);

            q.required = required;
            q.visible = visible;

            if (conditions)
            {
                var list = new List<ConditionalProperty>();
                list.Add(new ConditionalProperty("true","key1","EQUAL_TO","key2"));
                q.conditionalProperties = new ConditionalPropertyList(list);
            }

            return q;
        }

        public string GetQuestionJSON(string theName, string theType, bool conditions, bool required, bool visible,bool soloQuestion)
        {
            var q = AddQuestion(theName, theType, conditions, required, visible, soloQuestion);
            string theJSON = "";
            theJSON = JsonConvert.SerializeObject(q, Formatting.Indented).Replace("theOperator", "operator");

            return theJSON;
        }

            #region Helper Methods

            private void EnsureSectionExists()//questions MUST be added to a section
        {
            if (currentlyActiveSection == null)
            {
                AddSection("");

            }
        }

        private void EnsureStepExists()//questions MUST be added to a section
        {
            if (currentlyActiveStep == null)
            {
                AddStep("Step");

            }
        }

        private void EnsureFormExists()//if no form in progress create default blank form
        {
            if (!FormInProgress)
            {
                NewForm("Default");
            }
        }

        private void AddInvisibleStep()
        {
            if (currentForm != null)
            {
                AddStep("");
                currentlyActiveStep.questions.Add(new Section(""));
                currentlyActiveStep.visible = false;
            }

        }

        #endregion
    }
}
