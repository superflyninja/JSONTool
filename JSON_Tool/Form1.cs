using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSON_Tool
{
    public partial class JSONTool : Form
    {
        JSONFormController formController;

       // private string formNameLabel = "Form Name : ";
        private string numStepsLabel = "Number of Steps : ";

        private bool formInProgress;

        public JSONTool(JSONFormController theController)
        {
            InitializeComponent();
            PopulateQuestionComboBox();

            formController = theController;
            formInProgress =  false;
            visibleCheckBox.CheckState = CheckState.Checked;
        }

        private void testBtn_Click(object sender, EventArgs e)
        {

            formController.NewForm("Test Form");
            formController.Test();

            string json = formController.GetFormJSON();
            Clipboard.SetText(json);
            // string json = JsonConvert.SerializeObject(jf);
            MessageBox.Show($"JSON has been pasted to the clipboard","Information",MessageBoxButtons.OK, MessageBoxIcon.Information);
            
        }

        private void newFormBtn_Click(object sender, EventArgs e)
        {

            if (infoFormNameLabel.Text.Length > 0)//if a form name already entered, clear all and start fresh
            {
                ResetInfo();
                formInProgress = false;
            }
            else
            {
                if (formNameTextBox.Text.Length <= 0)//if no form name entered
                {
                    MessageBox.Show($"Form name cannot be blank", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else//form name entered -> create new form
                {
                    formController.NewForm(formNameTextBox.Text);
                    UpdateInfo();
                    formInProgress = true;
                }

            }

        }

        private void addStepBtn_Click(object sender, EventArgs e)
        {
            if (formInProgress)
            {
                if (stepNameTextBox.Text.Length <= 0)
                {
                    // formController.AddStep("Tab");
                    MessageBox.Show($"Step name cannot be blank", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    formController.AddStep(stepNameTextBox.Text);
                    stepNameTextBox.Text = "";//clear the box so that another step can be added
                }

                UpdateInfo();
            }
            else
            {
                MessageBox.Show($"To add a step start a new form first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void addSectionBtn_Click(object sender, EventArgs e)
        {
            if (formInProgress)
            {
                if (sectionNameTextBox.Text.Length <= 0)
                {
                    MessageBox.Show($"Section name cannot be blank", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    formController.AddSection(sectionNameTextBox.Text);
                    sectionNameTextBox.Text = "";//clear the box so that another step can be added
                }

                formController.totalNumSections++;
                UpdateInfo();
            }
            else
            {
                MessageBox.Show($"To add a section start a new form first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void addListButton_Click(object sender, EventArgs e)
        {
            if (formInProgress)
            {
                if (listNameTextBox.Text.Length <= 0)
                {
                    MessageBox.Show($"List name cannot be blank", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    formController.AddList(listNameTextBox.Text);
                    listNameTextBox.Text = "";//clear the box so that another list can be added
                }

                UpdateInfo();
            }
            else
            {
                MessageBox.Show($"To add a list start a new form first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            if (formInProgress)
            {
                string json = formController.GetFormJSON();
              //  Clipboard.SetText(json);
              //  MessageBox.Show($"JSON has been pasted to the clipboard", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                CopyJSONToClipboard(json);
                ResetInfo();
            }
            else
            {
                MessageBox.Show($"Start a new form to generate JSON", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            
        }

        private void addQuestionBtn_Click(object sender, EventArgs e)
        {
            if (formInProgress)
            {
                if (questionNameTextBox.Text.Length <= 0)
                {
                    MessageBox.Show($"Question name cannot be blank and type must be selected", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else//now allowed to add a question.
                {
                    formController.AddQuestion(questionNameTextBox.Text, questionComboBox.SelectedItem.ToString(),conditionsCheckbox.Checked,requiredCheckBox.Checked,visibleCheckBox.Checked,true);
                    questionNameTextBox.Text = "";
                    conditionsCheckbox.CheckState = CheckState.Unchecked;
                    requiredCheckBox.CheckState = CheckState.Unchecked;
                    visibleCheckBox.CheckState = CheckState.Checked;
                }

                UpdateInfo();
            }
            else
            {
                //MessageBox.Show($"To add a section start a new form first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //if no form in progress then just copy the question json to the clipboard
                string json = formController.GetQuestionJSON(questionNameTextBox.Text, questionComboBox.SelectedItem.ToString(), conditionsCheckbox.Checked, requiredCheckBox.Checked, visibleCheckBox.Checked,false);
                CopyJSONToClipboard(json);
            }
        }

        #region Helper Methods

        private void CopyJSONToClipboard(string theJSON)
        {
            Clipboard.SetText(theJSON);
            MessageBox.Show($"JSON has been pasted to the clipboard", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void UpdateInfo()
        {
            //Update Current Form Panel
            infoFormNameLabel.Text = formNameTextBox.Text;
            totalStepsLabel.Text = formController.numSteps + "";
            totalNumsectionsLabel.Text = formController.totalNumSections + "";
            totalNumQuestionsLabel.Text = formController.totalNumQuestions + "";

            //Update the Current Step Panel
            currentStepOrderLabel.Text = formController.numSteps + "";
            currentStepNameLabel.Text = formController.GetCurrentStepName();

            //Update Current Section Panel
            currentSectionNameLabel.Text = formController.GetCurrentSectionName();
        }

        private void ResetInfo()
        {
            formNameTextBox.Text = "";
            stepNameTextBox.Text = "";
            sectionNameTextBox.Text = "";
            listNameTextBox.Text = "";
            questionNameTextBox.Text = "";
            infoFormNameLabel.Text = "";
            totalStepsLabel.Text = "";
            totalNumsectionsLabel.Text = "";
            totalNumQuestionsLabel.Text = "";

            currentStepOrderLabel.Text = "";
            currentStepNameLabel.Text = "";

            currentSectionNameLabel.Text = "";

            conditionsCheckbox.CheckState = CheckState.Unchecked;
            requiredCheckBox.CheckState = CheckState.Unchecked;
            visibleCheckBox.CheckState = CheckState.Checked;
        }

        private void PopulateQuestionComboBox()
        {  
            questionComboBox.DataSource = Enum.GetValues(typeof(JSONFormController.QuestionTypes)).Cast<JSONFormController.QuestionTypes>();//this converts the enum to list of strings
        }
        #endregion

        private void newGuidButton_Click(object sender, EventArgs e)
        {
            string guid = JSONFormController.GenerateGUID();
            Clipboard.SetText(guid);

            MessageBox.Show($"GUID\n{guid}\nhas been pasted to the clipboard", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void conditionsCheckbox_CheckedChanged(object sender, EventArgs e)
        {

        }

    }
}
