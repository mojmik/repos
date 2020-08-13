using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VlastniList
{
    class Program
    {
        static void Main(string[] args)
        {
            MujList mujList = new MujList();
            mujList.Pridej("První");
            for (int n=2;n<10000;n++)
            {
                mujList.Pridej(n);
            }
            mujList[3] = "čtvrtý";
            for (int n=0;n<100;n++)
            {
                Console.WriteLine(mujList.objPole[n].ToString());
            }
            Console.ReadKey();
        }
    }
}
