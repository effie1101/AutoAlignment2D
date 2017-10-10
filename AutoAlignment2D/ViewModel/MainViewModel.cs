using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAlignment2D.ViewModel
{
    public class MainViewModel :Bandit.UI.MVVM.NotifyObject
    {

        public FC.MarkLocator.InputManager InputManager
        {
            get
            {
                return FC.MarkLocator.InputManager.Instance;
            }
        }

        public FC.MarkLocator.OutputManager OutputManager
        {
            get
            {
                return FC.MarkLocator.OutputManager.Instance;
            }
        }


        public MainViewModel()
        {
            this.InputManager.BondpadCenterX = 11.4;
            this.InputManager.BondpadCenterY = -0.2;
            this.InputManager.CCD2ProbeX = 0;
            this.InputManager.CCD2ProbeY = 54.5;
            this.InputManager.CCD2ProbeSita = 0;

            this.InputManager.ProbeHeadRotateR = 37.5;

            this.InputManager.AreaStartX = 450;
            this.InputManager.AreaEndX = 850;
            this.InputManager.AreaStartY = 350;
            this.InputManager.AreaEndY = 650;

            this.InputManager.LogFolder = @"D:\My Project\AutoAlignment2D\AutoAlignment2D\bin\Debug\CCD_Images\";

        }
    }
}