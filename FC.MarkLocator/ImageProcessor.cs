using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Diagnostics;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Collections.Generic;

namespace FC.MarkLocator
{
    public class ProcessImage
    {
        const double CCD_W = 4.8;
        const double CCD_H = 3.6;
        string logPath = string.Empty;
        string DUTNo = string.Empty;
        public MarkLocator.InputManager InputManager { get { return MarkLocator.InputManager.Instance; } }

        public MarkLocator.OutputManager OutputManager { get { return MarkLocator.OutputManager.Instance; } }

        /// <summary>
        /// 计算对位需要调整的距离，输出参数见OutputManager
        /// </summary>
        /// <param name="imgFile">输入产品的靶标的原始图片</param>
        /// <returns>bool result</returns>
        public bool MarkAlignment(string imgFile)
        {
                bool result = false;
            //设定待检测的闭环区域的最小面积
            int minContourArea = 2000;
                int  contourSize = 6;
                 int bThresh = 180;
            DUTNo = System.IO.Path.GetFileNameWithoutExtension(imgFile);
            logPath = System.IO.Path.GetDirectoryName(imgFile) + @"\" + DUTNo+"_";


            //find mark contours
            Mat cutImg, originalImg;
                List<RotatedRect> boxList = this.GetContours(imgFile, bThresh, minContourArea, contourSize, out originalImg, out cutImg);

                #region draw rectangles

                Mat markImage = cutImg;

                PointF markCenter = new PointF();
                double markAngle = 0;

                if (boxList.Count > 0)
                {
                    result = true;
                    markCenter = boxList[0].Center;
                    markAngle = boxList[0].Angle;

                //重新映射至原始图像并计算相对测头的位移
                this.OutputManager.AlignmentSita = Math.Round(markAngle + this.InputManager.CCD2ProbeSita, 2);
                this.OutputManager.AlignmentX = Math.Round(0-this.InputManager.ProbeHeadRotateR * Math.Sin(this.OutputManager.AlignmentSita/180 *Math.PI) + this.InputManager.CCD2ProbeX + (markCenter.X / originalImg.Width * CCD_W - CCD_W / 2)+ this.InputManager.BondpadCenterX, 2);
                this.OutputManager.AlignmentY = Math.Round(this.InputManager.ProbeHeadRotateR *(1-Math.Cos(this.OutputManager.AlignmentSita/180*Math.PI)) + this.InputManager.CCD2ProbeY + (markCenter.Y / originalImg.Height * CCD_H - CCD_H / 2) +this.InputManager.BondpadCenterY, 2);

                this.OutputManager.MarkImgFile =logPath +"output.png";

                   OutputManager.MarkContours= RemapMarkContours(boxList);
                    Mat outputImg = new Mat();
                    CvInvoke.CvtColor(originalImg, outputImg, ColorConversion.Gray2Bgr);
                    CvInvoke.Polylines(outputImg, OutputManager.MarkContours, true, new Bgr(Color.Red).MCvScalar, 1);
                    outputImg.Save(this.OutputManager.MarkImgFile);
                }
                #endregion
                return result;
        }

        public bool FindProbeMark(string imgFile)
        {
            bool result =false;
            int minContourArea = 8000;
            int contourSize = 4;
            //find mark contours
            Mat cutImg, originalImg;
            List<RotatedRect> boxList = this.GetContours(imgFile, 180,minContourArea, contourSize, out originalImg, out cutImg);

            #region draw rectangles

            Mat markImage = cutImg;

            PointF markCenter = new PointF();
            double markAngle = 0;

            if (boxList.Count > 0)
            {
                result = true;
                markCenter = boxList[0].Center;
                markAngle = Math.Round(boxList[0].Angle, 3);

                //重新映射至原始图像并计算探针座中心的图像坐标
                this.OutputManager.ProbeCenterX = markCenter.X + this.InputManager.AreaStartX + this.InputManager.BondpadCenterX;
                this.OutputManager.ProbeCenterX = markCenter.X + this.InputManager.AreaStartX + this.InputManager.BondpadCenterX;

                //this.OutputManager.AlignmentX = markCenter.X + this.InputManager.AreaStartX + this.InputManager.BondpadCenterX - originalImg.Width / 2.0 - this.InputManager.CCD2ProbeX;
                //this.OutputManager.AlignmentY = markCenter.Y + this.InputManager.AreaStartY + this.InputManager.BondpadCenterY - originalImg.Height / 2.0 - this.InputManager.CCD2ProbeY;
                this.OutputManager.AlignmentSita = markAngle - this.InputManager.CCD2ProbeSita;
                this.OutputManager.MarkImgFile = logPath + "outputMarkImage.png";

                OutputManager.MarkContours = RemapMarkContours(boxList);
                Mat outputImg = new Mat();
                CvInvoke.CvtColor(originalImg, outputImg, ColorConversion.Gray2Bgr);
                CvInvoke.Polylines(outputImg, OutputManager.MarkContours, true, new Bgr(Color.Red).MCvScalar, 2);
                outputImg.Save(this.OutputManager.MarkImgFile);
                originalImg.Save(logPath + "original.png");
            }
            #endregion

            return result;           
        }

        /// <summary>
        /// 提取mark的轮廓
        /// </summary>
        /// <param name="imgFile">image file name </param>
        /// <param name="minContourArea">mark的最小识别面积</param>
        /// <param name="contourSize">mark轮廓边数</param>
        /// <returns></returns>
        public List<RotatedRect> GetContours(string imgFile, int bThresh, int minContourArea, int contourSize, out Mat originalImg, out Mat cutImg)
        {
            //a box is a rotated rectangle
            List<RotatedRect> boxList = new List<RotatedRect>();
            //从文件读入图像
            originalImg = CvInvoke.Imread(imgFile, ImreadModes.AnyColor);

            #region 图像预处理

            //获取原始图像的宽和高
            int imgWidth = originalImg.Width;
            int imgHeight = originalImg.Height;
            //截取对象区域
            cutImg = new Mat(originalImg, new Range(InputManager.AreaStartY, InputManager.AreaEndY), new Range(InputManager.AreaStartX, InputManager.AreaEndX));
            //Convert the image to grayscale and filter out the noise
            Mat binaryImg = new Mat();

            //use image pyr to remove noise
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(cutImg, pyrDown);
            CvInvoke.PyrUp(pyrDown, cutImg);

            //convert to binary image 
            CvInvoke.Threshold(cutImg, binaryImg, bThresh, 255, ThresholdType.Binary);
            //save binary image
            binaryImg.Save(logPath + "BinaryImg.png");
            #endregion

            #region Canny and edge detection

            Stopwatch watch = Stopwatch.StartNew();
            double cannyThreshold = 180.0;

            watch.Reset();
            watch.Start();
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(binaryImg, cannyEdges, cannyThreshold, cannyThresholdLinking);
            cannyEdges.Save(logPath + "cannyEdges.png");

            LineSegment2D[] lines = CvInvoke.HoughLinesP(
               cannyEdges,
               1, //Distance resolution in pixel-related units
               Math.PI / 45.0, //Angle resolution measured in radians.
               20, //threshold
               30, //min Line width
               10); //gap between lines

            watch.Stop();
            #endregion

            #region Find rectangles
            watch.Reset();
            watch.Start();

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    using (VectorOfPoint approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        if (CvInvoke.ContourArea(approxContour, false) > minContourArea) //only consider contours with area greater than minContourArea
                        {
                            if (approxContour.Size == contourSize) 
                            {
                                #region determine if all the angles in the contour are within [80, 100] degree
                                bool isRectangle = true;
                                Point[] pts = approxContour.ToArray();
                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                                for (int j = 0; j < edges.Length; j++)
                                {
                                    double angle = Math.Abs(
                                       edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                    if (angle < 80 || angle > 100)
                                    {
                                        isRectangle = false;
                                        break;
                                    }
                                }
                                #endregion

                                if (isRectangle)
                                {
                                    boxList.Add(CvInvoke.MinAreaRect(approxContour));
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            watch.Stop();
            #endregion

            return boxList;
        }

        private Point[] RemapMarkContours(List<RotatedRect> boxList)
        {
            Point[] pts = Array.ConvertAll(boxList[0].GetVertices(), Point.Round);
            Point[] contourPts = new Point[pts.Length];
            for (int i = 0; i < contourPts.Length; i++)
            {
                contourPts[i] = new Point(pts[i].X + this.InputManager.AreaStartX, pts[i].Y + this.InputManager.AreaStartY);
            }
            return contourPts;
        }

        /// <summary>
        /// 从原始图像截图目标分析区域，以缩小范围
        /// </summary>
        /// <param name="sourceImg">原始采集的靶标图像</param>
        /// <param name="startX">截取区域的左上角X坐标，横向</param>
        /// <param name="startY">截取区域的左上角Y坐标，纵向</param>
        /// <param name="width">截取矩形区域的宽度</param>
        /// <param name="height">截取矩形区域的高度</param>
        /// <returns></returns>
        public Image<Bgr, byte> CutImage(Image<Bgr, byte> sourceImg, int startX, int startY, int width, int height)
        {
            Image<Bgr, byte> outputImg = new Image<Bgr, byte>(width, height);
            int x = sourceImg.Width - width;
            int y = sourceImg.Height - height;
            Size roiSize = new Size(width, height);//image size that needs to be cutted out
            //IntPtr dst = CvInvoke.cvCreateImage(roiSize, Emgu.CV.CvEnum.IplDepth.IplDepth_8U, 3);
            Rectangle rect = new Rectangle(startX, startY, width, height);
            CvInvoke.cvSetImageROI(sourceImg.Ptr, rect);
            CvInvoke.cvCopy(sourceImg.Ptr, outputImg.Ptr, IntPtr.Zero);
            return outputImg;
        }

        /// <summary>
        /// 将mark轮廓点还原至原始图像
        /// </summary>
        /// <param name="sourcePts">截取图像中mark的轮廓点</param>
        /// <param name="startX">截取区域的起始X</param>
        /// <param name="startY">截取区域的起始Y</param>
        /// <returns>映射到原始图像中的mark轮廓点</returns>
        public Point[] RemapCoutours(Point[] sourcePts, int startX, int startY)
        {
            Point[] pts = new Point[(sourcePts.Length)];
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = new Point(sourcePts[i].X + startX, sourcePts[i].Y + startY);
            }
            return pts;
        }

    }
}
