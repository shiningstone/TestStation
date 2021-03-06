﻿using Hardware;
using JbImage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using Utils;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TestStation.core
{
    public class CameraController
    {
        private Logger _log = new Logger(typeof(CameraController));
        public Camera mCamera;
        public int ImageBits = 8;

        public Bitmap LatestImage;
        public Bitmap AnalyzedImage;

        public CameraController()
        {
            _log.Debug(Config.ToString());
        }

        string _filePath;
        private List<double> _distances = new List<double>();
        public List<CircleImage> Imgs = new List<CircleImage>();
        private string _testType = "";
        public Result Open(string cameraType, string triggerType = "SoftwareTrigger")
        {
            _testType = cameraType;

            if (mCamera == null)
            {
                if (!string.IsNullOrEmpty(Config.ForceCameraType))
                {
                    HardwareSrv.GetInstance().Add(new M8051(triggerType));
                }
                else
                {
                    switch (cameraType)
                    {
                        case "NFT":
                            HardwareSrv.GetInstance().Add(new M8051(triggerType));
                            break;
                        case "FFT":
                            HardwareSrv.GetInstance().Add(new Vcxu("Camera"));
                            break;
                        default:
                            return new Result("Fail", "Unknown camera type");
                    }
                }

                mCamera = HardwareSrv.GetInstance().Get("Camera") as Camera;

                try
                {
                    Result ret = mCamera.Execute(new Command("Open"));
                    if (ret.Id != "Ok")
                    {
                        mCamera = null;
                    }
                    return ret;
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to open the camera", ex);
                    mCamera = null;
                    return new Result("Fail", ex.ToString());
                }
            }
            else
            {
                return new Result("Dummy", $"Camera({mCamera.Name}) is already opened");
            }
        }
        public Result Read(string tag)
        {
            double distance;
            if (!double.TryParse(tag, out distance))
            {
                distance = double.NaN;
            }

            Parameters param = EmguParameters.Params.Find(x => x.Tag == tag);
            if (param == null)
            {
                param = EmguParameters.Params.Find(x => x.Tag == "Default");
            }

            SetGain(param.Gain);
            SetExposure(param.ExposureTime);
            Thread.Sleep(2000);

            return Read(distance);
        }
        private Image<Gray, ushort> CurImage;
        public Result Read(double distance = double.NaN)
        {
            if (mCamera != null)
            {
                Result ret;

                if (ImageBits == 8)
                {
                    ret = mCamera.Execute(new Command("Read", new Dictionary<string, string> { { "Type", "Bmp" } }));
                    if (ret.Id == "Ok")
                    {
                        CurImage = null;
                        InsertImg(ret.Param as Bitmap, distance, true);
                    }
                }
                else
                {
                    ret = mCamera.Execute(new Command("Read", new Dictionary<string, string> { { "Type", "Raw" } }));
                    if (ret.Id == "Ok")
                    {
                        CurImage = EmguIntfs.ToImage(ret.Param as ushort[]);
                        InsertImg(CurImage.Bitmap, distance, true);
                    }
                }

                return ret;
            }
            else
            {
                return new Result("Fail", "Please open camera first");
            }
        }
        public Result Load(string filename, double distance = double.NaN)
        {
            CurImage = null;

            LatestImage = new Bitmap(filename);
            InsertImg(LatestImage, distance, false);
            return new Result("Ok");
        }
        public Result Analyze(string testType, double distance)
        {
            AnalyzerIntf analyzer = AnalyzerIntf.GetAnalyzer(testType);

            CircleImage i;

            if (CurImage != null)
            {
                i = analyzer.FindCircle(CurImage, HoughParams(distance.ToString()));
            }
            else
            {
                i = analyzer.FindCircle(_filePath, HoughParams(distance.ToString()));
            }

            Imgs.Add(i);
            AnalyzedImage = i.RetImg;

            CurImage = null;
            return new Result("Ok", "", AnalyzedImage);
        }
        public Result Calculate(string testType)
        {
            Result ret = AnalyzerIntf.GetAnalyzer(testType).Calculate(Imgs, _distances);

            _distances.Clear();
            Imgs.Clear();

            return ret;
        }
        public Result Close()
        {
            mCamera?.Execute(new Command("Close"));
            mCamera = null;
            return new Result("Ok");
        }
        public Result SetGain(int gain)
        {
            return mCamera?.Execute(new Command("Config", new Dictionary<string, string>() {
                { "Gain", gain.ToString() }
            }));
        }
        public Result SetExposure(int ms)
        {
            return mCamera?.Execute(new Command("Config", new Dictionary<string, string>() {
                { "Exposure", ms.ToString() }
            }));
        }
        private void InsertImg(Bitmap img, double distance, bool shouldSave = true)
        {
            if (!double.IsNaN(distance))
            {
                _distances.Add(distance);
            }

            _filePath = @"data/" + $"Img_{distance}_{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.bmp";
            img.Save(_filePath, ImageFormat.Bmp);
        }
        private class Config
        {
            public static string ForceCameraType
            {
                get
                {
                    return ConfigurationManager.AppSettings["ForceCameraType"];
                }
            }
            public static double CountThreshold
            {
                get
                {
                    return double.Parse(ConfigurationManager.AppSettings["CountThreshold"]);
                }
            }
            public static int[] RadiusLimit(double distance)
            {
                int[] ret = new int[2];

                double value = distance;
                if (!double.IsNaN(value))
                {
                    string index = "Min" + ((int)value).ToString("D2");
                    ret[0] = Int32.Parse(ConfigurationManager.AppSettings[index]);
                    index = "Max" + ((int)value).ToString("D2");
                    ret[1] = Int32.Parse(ConfigurationManager.AppSettings[index]);
                }
                else
                {
                    ret[0] = Int32.Parse(ConfigurationManager.AppSettings["Min"]);
                    ret[1] = Int32.Parse(ConfigurationManager.AppSettings["Max"]);
                }

                return ret;
            }
            public static new string ToString()
            {
                string output = "";
                foreach (string key in ConfigurationManager.AppSettings.Keys)
                {
                    output += $"{key}:{ConfigurationManager.AppSettings[key]}" + ",";
                }
                output.Substring(0, output.Length - 1);
                return output;
            }
        }
        private int[] RadiusLimits(double distance)
        {
            int[] ret = new int[2];

            double value = distance;
            if (!double.IsNaN(value))
            {
                string index = "Min" + ((int)value).ToString("D2");
                ret[0] = Int32.Parse(ConfigurationManager.AppSettings[index]);
                index = "Max" + ((int)value).ToString("D2");
                ret[1] = Int32.Parse(ConfigurationManager.AppSettings[index]);
            }
            else
            {
                ret[0] = Int32.Parse(ConfigurationManager.AppSettings["Min"]);
                ret[1] = Int32.Parse(ConfigurationManager.AppSettings["Max"]);
            }

            return ret;
        }
        private Parameters HoughParams(string distance)
        {
            Parameters param = EmguParameters.Params.Find(x => x.Tag == distance);
            if (param == null)
            {
                param = EmguParameters.Params.Find(x => x.Tag == "Default");
            }

            return param;
        }
        /* to be obsoleted */
        private void ProcessWithCircleFinder()
        {
            //var rawData = mCamera.Execute(new Command("Read", new Dictionary<string, string> { { "Type", "Raw" } })).Param as Bitmap;

            string resultBmp = Utils.String.FilePostfix(_filePath, "-result");
            string resultTxt = resultBmp.Replace(".bmp", ".txt");

            Bitmap bmpFile = ImgProcess.Binarize(_filePath);
            CirclesFinder f = new CirclesFinder(bmpFile);

            LatestImage = f.Draw(resultBmp);

            ImgProcess.Count(f.Rounds);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(resultTxt, true))
            {
                file.Write(string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                    "ID", "X", "Y",
                    "LengthOnX", "DeviationOnX",
                    "LengthOnY", "DeviationOnY",
                    "Weight", "DeviationOnWeight") + Environment.NewLine);

                foreach (var round in f.Rounds)
                {
                    file.Write(string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                        round.Id.ToString("D3"), round.CenterX, round.CenterY,
                        round.MaxLenLine.Length, round.LenXDiff.ToString("F4"),
                        round.EndY - round.StartY, round.LenYDiff.ToString("F4"),
                        round.Weight.ToString(), round.WeightDiff.ToString("F4")));
                    file.Write(Environment.NewLine);
                }

                double radiusStdEv = Utils.Math.StdEv(f.Rounds.Select(x => (double)x.MaxLenLine.Length).ToList());
                file.Write(string.Format("StdEv of Radius: {0}", radiusStdEv));
                file.Write(Environment.NewLine);

                double weightStdEv = Utils.Math.StdEv(f.Rounds.Select(x => (double)x.Weight).ToList());
                file.Write(string.Format("StdEv of Weight: {0}", weightStdEv));
                file.Write(Environment.NewLine);
            }
        }
    }
}