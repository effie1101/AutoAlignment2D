﻿using Emgu.CV;
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
        //设定待检测的闭环区域的最小面积
        int _minContourArea = 2000;
        int _imgWidth = 0;
        int _imgHeight = 0;

        public MarkLocator.InputManager InputManager { get { return MarkLocator.InputManager.Instance; } }

        public MarkLocator.OutputManager OutputManager { get { return MarkLocator.OutputManager.Instance; } }

        public bool FindMark(string imgFile)
        {
            try
            {
            bool result = false;
            Mat originalImg = CvInvoke.Imread(imgFile, ImreadModes.AnyColor);

            //获取原始图像的宽和高
            this._imgWidth = originalImg.Width;
            this._imgHeight = originalImg.Height;
            //截取对象区域
            Mat cutImg = new Mat(originalImg, new Range(InputManager.AreaStartY, InputManager.AreaEndY), new Range(InputManager.AreaStartX, InputManager.AreaEndX));
            cutImg.Save(InputManager.LogFolder + "cutImg.png");
            //Convert the image to grayscale and filter out the noise
            Mat binaryImg = new Mat();

            //use image pyr to remove noise
            UMat pyrDown = new UMat();
            CvInvoke.PyrDown(cutImg, pyrDown);
            CvInvoke.PyrUp(pyrDown, cutImg);

            //convert to binary image 
            CvInvoke.Threshold(cutImg, binaryImg, 100, 255, ThresholdType.BinaryInv);
            //save binary image
            binaryImg.Save(InputManager.LogFolder + "BinaryImg.png");

            #region Canny and edge detection

            Stopwatch watch = Stopwatch.StartNew();
            double cannyThreshold = 180.0;

            watch.Reset();
            watch.Start();
            double cannyThresholdLinking = 120.0;
            UMat cannyEdges = new UMat();
            CvInvoke.Canny(cutImg, cannyEdges, cannyThreshold, cannyThresholdLinking);
            cannyEdges.Save(InputManager.LogFolder + "cannyEdges.png");

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
            List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle
            List<VectorOfPoint> contourList = new List<VectorOfPoint>();
            VectorOfPoint markContour = new VectorOfPoint();

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
                        if (CvInvoke.ContourArea(approxContour, false) > _minContourArea) //only consider contours with area greater than 250
                        {
                            if (approxContour.Size == 6) //The contour has 6 vertices.
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
                                    contourList.Add(approxContour);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            watch.Stop();
            #endregion

            #region draw rectangles

            Mat markImage = cutImg;

            PointF markCenter = new PointF();
            double markAngle = 0;

            if (boxList.Count > 0)
            {
                result = true;
                markCenter = boxList[0].Center;
                markAngle = Math.Round(boxList[0].Angle, 3);

                //重新映射至原始图像并计算相对测头的位移
                this.OutputManager.AlignmentX = markCenter.X + this.InputManager.AreaStartX + this.InputManager.BondpadCenterX - this._imgWidth / 2.0 - this.InputManager.Probe2CCDX;
                this.OutputManager.AlignmentY = markCenter.Y + this.InputManager.AreaStartY + this.InputManager.BondpadCenterY - this._imgHeight / 2.0 - this.InputManager.Probe2CCDY;
                this.OutputManager.AlignmentSita = markAngle - this.InputManager.Probe2CCDSita;
                this.OutputManager.MarkImgFile = InputManager.LogFolder + "outputMarkImage.png";

                Point[] markContours = RemapMarkContours(boxList);
                Mat outputImg = new Mat();
                CvInvoke.CvtColor(originalImg, outputImg, ColorConversion.Gray2Bgr);
                CvInvoke.Polylines(outputImg, markContours, true, new Bgr(Color.Red).MCvScalar, 2);
                outputImg.Save(this.OutputManager.MarkImgFile);
            }
            #endregion
            return result;
            }
            catch (Exception e)
            {

                throw e;
            }
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
