using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;




class Konna : PlatformCharacter
{
    public int HP { get; set; }
  

    public Konna(double leveys, double korkeus, int elamat)
        : base(leveys, korkeus)
    {
        HP = 3;
    }
}


