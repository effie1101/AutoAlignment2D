using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace FC.MarkLocator
{
    public class OutputManager:Bandit.UI.MVVM.NotifyObject
    {
        private static OutputManager _Instance;

        public static OutputManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new OutputManager();
                }

                return _Instance;
            }
        }

        #region final alignment output
        private double _alignmentX;
        private double _alignmentY;
        private double _alignmentSita;
        private Point[] _markContours;

        private string _markImgFile;

        #endregion

        #region CCD Calibration Output

        #endregion

        #region CCDs Adjustment Output
        private double _lower2UpperX;
        private double _lower2UpperY;
        private double _lower2UpperSita;
        private double _probeHeadSita;
        #endregion

        #region probe head location output
        private double probeCenterX;
        private double probeCenterY;
        #endregion

        /// <summary>
        /// 测头于X轴需要调整的距离
        /// </summary>
        public double AlignmentX
        {
            get
            {
                return _alignmentX;
            }

            set
            {
                _alignmentX = value;
                OnPropertyChanged("AlignmentX");
            }
        }

        /// <summary>
        /// 测头于Y轴需要调整的距离
        /// </summary>
        public double AlignmentY
        {
            get
            {
                return _alignmentY;
            }

            set
            {
                _alignmentY = value;
                OnPropertyChanged("AlignmentY");
            }
        }

        /// <summary>
        /// 测头于Sita轴需要调整的转角
        /// </summary>
        public double AlignmentSita
        {
            get
            {
                return _alignmentSita;
            }

            set
            {
                _alignmentSita = value;
                OnPropertyChanged("AlignmentSita");
            }
        }

        /// <summary>
        /// 下CCD相对于上CCD的水平X位移
        /// </summary>
        public double Lower2upperX
        {
            get
            {
                return _lower2UpperX;
            }

            set
            {
                _lower2UpperX = value;
            }
        }

        /// <summary>
        /// 下CCD相对于上CCD的Y方向位移
        /// </summary>
        public double Lower2UpperY
        {
            get
            {
                return _lower2UpperY;
            }

            set
            {
                _lower2UpperY = value;
            }
        }

        /// <summary>
        /// 下CCD相对于上CCD的水平夹角
        /// </summary>
        public double Lower2UpperSita
        {
            get
            {
                return _lower2UpperSita;
            }

            set
            {
                _lower2UpperSita = value;
            }
        }

        /// <summary>
        /// 测头模块的水平偏移转角
        /// </summary>
        public double ProbeHeadSita
        {
            get
            {
                return _probeHeadSita;
            }

            set
            {
                _probeHeadSita = value;
            }
        }

        /// <summary>
        /// full name of image that with the found mark
        /// </summary>
        public string MarkImgFile
        {
            get
            {
                return _markImgFile;
            }

            set
            {
                _markImgFile = value;
            }
        }

        //探针，斑马条，金手指中心在下CCD的图像坐标
        public double ProbeCenterX
        {
            get
            {
                return probeCenterX;
            }

            set
            {
                probeCenterX = value;
            }
        }

        //探针，斑马条，金手指中心在下CCD的图像坐标
        public double ProbeCenterY
        {
            get
            {
                return probeCenterY;
            }

            set
            {
                probeCenterY = value;
            }
        }

        public Point[] MarkContours
        {
            get
            {
                return _markContours;
            }

            set
            {
                _markContours = value;
            }
        }
    }
}
