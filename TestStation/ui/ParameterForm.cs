﻿using System;
using System.Configuration;
using System.Windows.Forms;
using JbImage;
using Utils;

namespace TestStation.ui
{
    public partial class ParameterForm : Form
    {
        public ParameterForm(string distance)
        {
            InitializeComponent();

            try
            {
                if (bool.Parse(Properties.Settings.Default.HideParameterName))
                {
                    HideParameterName();
                }
            }
            catch (Exception ex)
            {
                HideParameterName();
            }

            tbTag.Text = distance;
            LoadEmguParameters(distance);
        }
        private void HideParameterName()
        {
            int count = 1;

            foreach (Control c in Controls)
            {
                if (c is Label || c is CheckBox)
                {
                    c.Text = $"Param.{count}";
                    count++;
                }
                else if (c is GroupBox)
                {
                    GroupBox g = c as GroupBox;
                    foreach (Control cs in g.Controls)
                    {
                        cs.Text = $"Param.{count}";
                        count++;
                    }
                }
            }
        }
        private void BTN_Set_Click(object sender, EventArgs e)
        {
            Parameters param = EmguParameters.Params.Find(x => x.Tag == tbTag.Text);
            if (param == null)
            {
                param = new Parameters
                {
                    Tag = tbTag.Text
                };
                EmguParameters.Params.Add(param);
            }

            param.Gain = Int32.Parse(tbGain.Text);
            param.ExposureTime = Int32.Parse(tbExpo.Text);

            param.BinThreshold = Int32.Parse(tbBinThreshold.Text);
            param.FilterSquareExtra = Int32.Parse(tbFilterSizeExtra.Text);

            param.Canny1Threshold1 = double.Parse(tbCanny1Threshold1.Text);
            param.Canny1Threshold2 = double.Parse(tbCanny1Threshold2.Text);
            param.Canny1ApertureSize = Int32.Parse(tbCanny1ApertureSize.Text);
            param.Canny1I2Gradient = bool.Parse(tbCanny1I2Gradient.Text);

            param.Hough1Dp = double.Parse(tbHough1Dp.Text);
            param.Hough1MinDist = double.Parse(tbHough1MinDist.Text);
            param.Hough1Param1 = double.Parse(tbHough1Param1.Text);
            param.Hough1Param2 = double.Parse(tbHough1Param2.Text);
            param.Hough1MinRadius = Int32.Parse(tbHough1MinRadius.Text);
            param.Hough1MaxRadius = Int32.Parse(tbHough1MaxRadius.Text);

            param.Canny2Threshold1 = double.Parse(tbCanny2Threshold1.Text);
            param.Canny2Threshold2 = double.Parse(tbCanny2Threshold2.Text);
            param.Canny2ApertureSize = Int32.Parse(tbCanny2ApertureSize.Text);
            param.Canny2I2Gradient = bool.Parse(tbCanny2I2Gradient.Text);

            param.Hough2Dp = double.Parse(tbHough2Dp.Text);
            param.Hough2MinDist = double.Parse(tbHough2MinDist.Text);
            param.Hough2Param1 = double.Parse(tbHough2Param1.Text);
            param.Hough2Param2 = double.Parse(tbHough2Param2.Text);
            param.Hough2MinRadius = Int32.Parse(tbHough2MinRadius.Text);
            param.Hough2MaxRadius = Int32.Parse(tbHough2MaxRadius.Text);

            param.ExtraStrengthen = CB_ExtraStrengthen.Checked;
            param.UseCanny = CB_UseCanny.Checked;
            param.SaveFile = CB_Save.Checked;
            param.ShowFirstResult = CB_ShowFirstResult.Checked;
        }

        private void BTN_Save_Click(object sender, EventArgs e)
        {
            BTN_Set_Click(null, null);
            XmlSerializer.Save("EmguParameters.xml", EmguParameters.Params);
            Close();
        }
        private void BTN_Reset_Click(object sender, EventArgs e)
        {
            LoadEmguParameters(tbTag.Text);
        }
        private void LoadEmguParameters(string distance)
        {
            Parameters param = EmguParameters.Params.Find(x => x.Tag == distance);
            if (param == null)
            {
                MessageBox.Show($"Failed to find parameters for {distance}, load default parameters instead");
                param = EmguParameters.Params.Find(x => x.Tag == "Default");
            }

            tbGain.Text = param.Gain.ToString();
            tbExpo.Text = param.ExposureTime.ToString();

            tbBinThreshold.Text = param.BinThreshold.ToString();
            tbFilterSizeExtra.Text = param.FilterSquareExtra.ToString();

            tbCanny1Threshold1.Text = param.Canny1Threshold1.ToString();
            tbCanny1Threshold2.Text = param.Canny1Threshold2.ToString();
            tbCanny1ApertureSize.Text = param.Canny1ApertureSize.ToString();
            tbCanny1I2Gradient.Text = param.Canny1I2Gradient.ToString();

            tbHough1Dp.Text = param.Hough1Dp.ToString();
            tbHough1MinDist.Text = param.Hough1MinDist.ToString();
            tbHough1Param1.Text = param.Hough1Param1.ToString();
            tbHough1Param2.Text = param.Hough1Param2.ToString();
            tbHough1MinRadius.Text = param.Hough1MinRadius.ToString();
            tbHough1MaxRadius.Text = param.Hough1MaxRadius.ToString();

            tbCanny2Threshold1.Text = param.Canny2Threshold1.ToString();
            tbCanny2Threshold2.Text = param.Canny2Threshold2.ToString();
            tbCanny2ApertureSize.Text = param.Canny2ApertureSize.ToString();
            tbCanny2I2Gradient.Text = param.Canny2I2Gradient.ToString();

            tbHough2Dp.Text = param.Hough2Dp.ToString();
            tbHough2MinDist.Text = param.Hough2MinDist.ToString();
            tbHough2Param1.Text = param.Hough2Param1.ToString();
            tbHough2Param2.Text = param.Hough2Param2.ToString();
            tbHough2MinRadius.Text = param.Hough2MinRadius.ToString();
            tbHough2MaxRadius.Text = param.Hough2MaxRadius.ToString();

            CB_ExtraStrengthen.Checked = param.ExtraStrengthen;
            CB_UseCanny.Checked = param.UseCanny;
            CB_Save.Checked = param.SaveFile;
            CB_ShowFirstResult.Checked = param.ShowFirstResult;
        }
    }
}
