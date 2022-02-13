using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SEModCreateTool
{


    /// <summary>
    /// EasySurvival
    /// </summary>
    internal class EasySurvival
    {
        /// <summary>
        /// Create
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        public void Create(string srcPath, string dstPath)
        {
            //Blueprints
            {
                var easySurvivalCreate = new EasySurvivalCreateBlueprints();
                easySurvivalCreate.Create(srcPath, dstPath);
            }

            //OxygenGenerator
            {
                var easySurvivalCreate = new EasySurvivalCreateOxygenGenerator();
                easySurvivalCreate.Create(srcPath, dstPath);
            }

            //Battery
            {
                var easySurvivalCreate = new EasySurvivalCreateBattery();
                easySurvivalCreate.Create(srcPath, dstPath);
            }

            //Characters
            {
                var easySurvivalCreate = new EasySurvivalCharacters();
                easySurvivalCreate.Create(srcPath, dstPath);
            }

            //Power
            {
                var easySurvivalCreate = new EasySurvivalCreatePower();
                easySurvivalCreate.Create(srcPath, dstPath);
            }



        }
    }
}
