using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FC.MarkLocator
{
    public class InputManager : Bandit.UI.MVVM.NotifyObject
    {
        private static InputManager _Instance;

        public static InputManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new InputManager();
                }
                return _Instance;
            }
        }

        private string _markImageFile;

        private double _bondpadCenterX;
        private double _bondpadCenterY;
        private double _probe2CCDX;
        private double _probe2CCDY;
        private double _probe2CCDSita;

        private int _areaStartX;
        private int _areaStartY;
        private int _areaEndX;
        private int _areaEndY;

        private string _logFolder;

        private double _probeCenterX;
        private double _probeCenterY;

        /// <summary>
        /// imported file of mark image
        /// </summary>
        public string MarkImageFile
        {
            get
            {
                return _markImageFile;
            }

            set
            {
                _markImageFile = value;
            }
        }

        /// <summary>
        /// bondpad中心相对mark中心的水平位移
        /// </summary>
        public double BondpadCenterX
        {
            get
            {
                return _bondpadCenterX;
            }

            set
            {
                _bondpadCenterX = value;

                OnPropertyChanged("BondpadCenterX");
            }
        }

        /// <summary>
        /// bondpad中心相对mark中心的垂直位移
        /// </summary>
        public double BondpadCenterY
        {
            get
            {
                return _bondpadCenterY;
            }

            set
            {
                _bondpadCenterY = value;
                OnPropertyChanged("BondpadCenterY");
            }
        }

        /// <summary>
        /// 探针,斑马条等contact pad与CCD中心距离的水平位移
        /// </summary>
        public double Probe2CCDX
        {
            get
            {
                return _probe2CCDX;
            }

            set
            {
                _probe2CCDX = value;
                OnPropertyChanged("Probe2CCDX");
            }
        }

        /// <summary>
        /// 探针,斑马条等contact pad与CCD中心距离的垂直位移
        /// </summary>
        public double Probe2CCDY
        {
            get
            {
                return _probe2CCDY;
            }

            set
            {
                _probe2CCDY = value;
                OnPropertyChanged("Probe2CCDY");
            }
        }

        /// <summary>
        /// 探针,斑马条等contact pad与CCD中心的水平夹角
        /// </summary>
        public double Probe2CCDSita
        {
            get
            {
                return _probe2CCDSita;
            }

            set
            {
                _probe2CCDSita = value;
                OnPropertyChanged("Probe2CCDSita");
            }
        }

        /// <summary>
        /// 待截取区域的左上角X坐标
        /// </summary>
        public int AreaStartX
        {
            get
            {
                return _areaStartX;
            }

            set
            {
                _areaStartX = value;
            }
        }

        /// <summary>
        /// 待截取区域的左上角Y坐标
        /// </summary>
        public int AreaStartY
        {
            get
            {
                return _areaStartY;
            }

            set
            {
                _areaStartY = value;
            }
        }

        /// <summary>
        /// 待截取区域的宽度
        /// </summary>
        public int AreaEndX
        {
            get
            {
                return _areaEndX;
            }

            set
            {
                _areaEndX = value;
            }
        }

        /// <summary>
        /// 待截取区域的高度
        /// </summary>
        public int AreaEndY
        {
            get
            {
                return _areaEndY;
            }

            set
            {
                _areaEndY = value;
            }
        }

        /// <summary>
        /// log存储的路径
        /// </summary>
        public string LogFolder
        {
            get
            {
                return _logFolder;
            }

            set
            {
                _logFolder = value;
            }
        }

        //压头上针座/斑马条/FPC金手指的中心与靶标的相对距离 -- Y向
        public double ProbeCenterY
        {
            get
            {
                return _probeCenterY;
            }

            set
            {
                _probeCenterY = value;
            }
        }

        //压头上针座/斑马条/FPC金手指的中心与靶标的相对距离 -- X向
        public double ProbeCenterX
        {
            get
            {
                return _probeCenterX;
            }

            set
            {
                _probeCenterX = value;
            }
        }
    }
}
