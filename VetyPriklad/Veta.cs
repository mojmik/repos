using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VetyPriklad
{
    public class Veta
    {
        String[] slova;
        public Veta(String veta)
        {
            InitZeStringu(veta);
        }

        public void InitZeStringu(String veta)
        {
            slova = veta.Split(new char[] { '.', ',', '!', ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public void Mapuj(Func <string, string> operace)
        {
            for (int n=0;n<slova.Count();n++)
            {
                slova[n] = operace(slova[n]);
            }
        }

        public String Agreguj(Func <string, string, string> operace)
        {
            String veta2 = "";            
            for (int n = 0; n < slova.Count(); n++)
            {
                if (n==0)
                {
                    veta2 = slova[n];
                }
                else veta2 = operace(veta2, slova[n]);

            }

            return veta2;
        }

        public override string ToString()
        {
            String outStr = "";
            int cnt=0;
            foreach (string s in slova)
            {
                if (cnt > 0) outStr = outStr + " ";
                outStr = outStr + s;
                cnt++;
            }
            return outStr;
        }
    }
}
